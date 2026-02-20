using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterSeatParamState : ParamBaseState<CharacterSeatParamState.VehicleData>
    {
        public struct VehicleData : IStateNetworkData
        {
            public CarVehicle Vehicle;
            public Seat Seat;

            public void Boxing(NetworkBehaviourReference[] references)
            {
                references[0].TryGet(out Vehicle);
                references[1].TryGet(out Seat);
            }

            public NetworkBehaviourReference[] Unboxing()
            {
                return new NetworkBehaviourReference[]
                {
                    Vehicle,
                    Seat
                };
            }

            public bool IsValid()
            {
                return Vehicle != null && Seat != null;
            }
        }

        private PlayerController _playerController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private NetworkAnimator _networkAnimator;
        private VehicleInputData _inputData;

        public CharacterSeatParamState(PlayerController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, NetworkAnimator networkAnimator,
            VehicleInputData inputData) :
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
            var data = new CharacterChangeSeatParamState.VehicleData()
            {
                Vehicle = Data.Vehicle,
                Seat = seat
            };

            Context.SwitchStateWithData<CharacterChangeSeatParamState, CharacterChangeSeatParamState.VehicleData>(data);
        }

        private void ExitVehicle()
        {
            Context.SwitchStateWithData<CharacterExitVehicleParamState, CarVehicle>(Data.Vehicle);
        }
    }
}