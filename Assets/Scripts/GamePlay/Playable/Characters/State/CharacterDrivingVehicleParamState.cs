using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State.StateParam;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using Test;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using NetworkTimer = Utils.NetworkTimer;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterDrivingVehicleParamState : TickableParamBaseState<VehicleSeatData>
    {
        private PlayerController _playerController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private NetworkAnimator _networkAnimator;
        private PlayerInputData _inputData;

        private NetworkTimer _networkTimer;

        public CharacterDrivingVehicleParamState(PlayerController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, NetworkAnimator networkAnimator,
            PlayerInputData inputData) :
            base(context)
        {
            _playerController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _networkAnimator = networkAnimator;
            _inputData = inputData;
        }
        
        public override void Enter()
        {
            _characterController.enabled = false;
            _networkAnimator.enabled = true;
            _networkTimer = new NetworkTimer();
            if (_playerController.IsServer)
            {
                _characterAnimationController.SwitchToDrivingLayer();
                _characterAnimationController.ResetBodyOrientation();
                _inputData.InteractPressed.AddListener(ExitVehicle);
                _inputData.ChangeSeatPressed.AddListener(ChangeSeat);
            }
            else
            {
                _playerController.transform.localPosition = Data.DriverSeat.Pivot.localPosition;
                _playerController.transform.localRotation = Data.DriverSeat.Pivot.localRotation;
            }
        }

        public override void Exit()
        {
            _characterController.enabled = true;
            _inputData.InteractPressed.RemoveListener(ExitVehicle);
            _inputData.ChangeSeatPressed.RemoveListener(ChangeSeat);
            Data.DriverSeat.SetInputData(default);
            _networkAnimator.enabled = false;
        }
        
        public override void Tick(float deltaTime)
        {
            _networkTimer.Update(deltaTime);
            if (_networkTimer.ShouldTick())
            {
                var inputState = _inputData.GetState();
                if (_playerController.IsServer)
                {
                    Data.DriverSeat.SetInputData(inputState.VehicleInputData);
                }
                else
                {
                    inputState.Tick = _networkTimer.CurrentTick;
                    _playerController.SendInputServerRpc(inputState);
                }
            }
        }

        private void ChangeSeat()
        {
            var seat = Data.Vehicle.TryChangeSeat(_playerController);
            var data = new ChangeSeatData
            {
                Vehicle = Data.Vehicle,
                Seat = seat
            };

            Context.SwitchStateWithData<CharacterChangeSeatParamState, ChangeSeatData>(data);
        }

        private void ExitVehicle()
        {
            var data = new LeaveVehicleSeat()
            {
                Vehicle = Data.Vehicle,
                Seat = Data.DriverSeat
            };
            Context.SwitchStateWithData<CharacterExitVehicleParamState, LeaveVehicleSeat>(data);
        }
    }
}