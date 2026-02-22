using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State.StateParam;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using GamePlay.Vehicle.Car.Weapons;
using Test;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using NetworkTimer = Utils.NetworkTimer;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterSeatMiniGunParamState : TickableParamBaseState<MiniGunSeatData>
    {
        private PlayerController _playerController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private NetworkAnimator _networkAnimator;
        private PlayerInputData _inputData;

        [SerializeField]
        private NetworkTimer _networkTimer;

        public CharacterSeatMiniGunParamState(PlayerController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, NetworkAnimator networkAnimator,
            PlayerInputData inputData) : base(context)
        {
            _playerController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _networkAnimator = networkAnimator;
            _inputData = inputData;
        }

        public override void Tick(float deltaTime)
        {
            _networkTimer.Update(deltaTime);
            if (_networkTimer.ShouldTick())
            {
                var inputState = _inputData.GetState();
                if (_playerController.IsServer)
                {
                    var miniGunInputData = Data.MiniGunController.InputData;
                    miniGunInputData.LookDirection = inputState.LookDirection;
                    miniGunInputData.Fire = inputState.FireEngaged;
                    miniGunInputData.SetDirty(true);
                }
                else
                {
                    inputState.Tick = _networkTimer.CurrentTick;
                    _playerController.SendInputServerRpc(inputState);
                }
            }
        }

        public override void Enter()
        {
            _networkTimer = new NetworkTimer();
            _characterController.enabled = false;
            _characterAnimationController.SwitchToMiniGunLayer();
            _characterAnimationController.ResetBodyOrientation();

            if (_playerController.IsServer)
            {
                _characterAnimationController.SetMiniGunIKTargetsRpc(Data.MiniGunController);
                _inputData.InteractPressed.AddListener(ExitVehicle);
                _inputData.ChangeSeatPressed.AddListener(ChangeSeat);
                Data.MiniGunController.Activate();
            }
            else
            {
                _playerController.transform.localPosition = Data.MiniGunSeat.Pivot.localPosition;
                _playerController.transform.localRotation = Data.MiniGunSeat.Pivot.localRotation;
                _networkAnimator.enabled = true;
            }
        }

        public override void Exit()
        {
            _characterAnimationController.ResetAllIkTargetsRpc();
            _inputData.InteractPressed.RemoveListener(ExitVehicle);
            _inputData.ChangeSeatPressed.RemoveListener(ChangeSeat);
            _networkAnimator.enabled = false;
            if (_playerController.IsServer)
            {
                Data.MiniGunController.Deactivate();
            }
        }

        private void ChangeSeat()
        {
            var seat = Data.Vehicle.TryChangeSeat(_playerController);
            var data = new ChangeSeatData()
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
                Seat = Data.MiniGunSeat
            };
            Context.SwitchStateWithData<CharacterExitVehicleParamState, LeaveVehicleSeat>(data);
        }
    }
}