using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class
        CharacterDrivingVehicleParamState : ParamBaseState<CharacterDrivingVehicleParamState.VehicleData>
    {
        public struct VehicleData : IStateNetworkData
        {
            public CarVehicle Vehicle;
            public DriverSeat DriverSeat;

            public void Boxing(NetworkBehaviourReference[] references)
            {
                references[0].TryGet(out Vehicle);
                references[1].TryGet(out DriverSeat);
            }

            public NetworkBehaviourReference[] Unboxing()
            {
                return new NetworkBehaviourReference[]
                {
                    Vehicle,
                    DriverSeat
                };
            }

            public bool IsValid()
            {
                return Vehicle != null && DriverSeat != null;
            }
        }

        private PlayerController _playerController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private NetworkAnimator _networkAnimator;
        private VehicleInputData _inputData;

        public CharacterDrivingVehicleParamState(PlayerController context, CharacterController characterController,
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
            _networkAnimator.enabled = true;
            if (_playerController.IsServer)
            {
                _characterAnimationController.SwitchToDrivingLayer();
                _characterAnimationController.ResetBodyOrientation();
                _inputData.InteractPressed.AddListener(ExitVehicle);
                _inputData.ChangeSeatPressed.AddListener(ChangeSeat);
                Data.DriverSeat.SetInputData(_inputData.CarVehicleInputData);
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
            Data.DriverSeat.SetInputData(null);
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