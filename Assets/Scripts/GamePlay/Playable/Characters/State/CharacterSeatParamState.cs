using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State.StateParam;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterSeatParamState : ParamBaseState<SeatData>
    {
        private PlayerController _playerController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private NetworkAnimator _networkAnimator;
        private PlayerInputData _inputData;

        public CharacterSeatParamState(PlayerController context, CharacterController characterController,
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
            if (_playerController.IsServer)
            {
                _characterAnimationController.SwitchToSeatLayer();
                _characterAnimationController.ResetBodyOrientation();
                _inputData.InteractPressed.AddListener(ExitVehicle);
                _inputData.ChangeSeatPressed.AddListener(ChangeSeat);
            }
            else
            {
                _playerController.transform.localPosition = Data.Seat.Pivot.localPosition;
                _playerController.transform.localRotation = Data.Seat.Pivot.localRotation;
                _networkAnimator.enabled = true;
            }
        }

        public override void Exit()
        {
            _inputData.InteractPressed.RemoveListener(ExitVehicle);
            _inputData.ChangeSeatPressed.RemoveListener(ChangeSeat);
            _networkAnimator.enabled = false;
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
                Seat = Data.Seat
            };
            Context.SwitchStateWithData<CharacterExitVehicleParamState, LeaveVehicleSeat>(data);
        }
    }
}