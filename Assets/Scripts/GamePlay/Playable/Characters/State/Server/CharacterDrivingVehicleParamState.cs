using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Playable.Characters.State.Server
{
    public class
        CharacterDrivingVehicleParamState : ParamBaseState<CharacterDrivingVehicleParamState.VehicleData>
    {
        public struct VehicleData
        {
            public CarVehicle Vehicle;
            public DriverSeat DriverSeat;
        }
        
        private BaseCharacterController _baseCharacterController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private VehicleInputData _inputData;

        public CharacterDrivingVehicleParamState(BaseCharacterController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, VehicleInputData inputData) :
            base(context)
        {
            _baseCharacterController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputData = inputData;
        }

        public override void Enter()
        {
            _characterController.enabled = false;
            _characterAnimationController.SwitchToDrivingLayer();
            _characterAnimationController.ResetBodyOrientation();
            _inputData.InteractPressed.AddListener(ExitVehicle);
            _inputData.ChangeSeatPressed.AddListener(ChangeSeat);

            Data.DriverSeat.SetInputData(_inputData.CarVehicleInputData);
        }

        public override void Exit()
        {
            _inputData.InteractPressed.RemoveListener(ExitVehicle);
            _inputData.ChangeSeatPressed.RemoveListener(ChangeSeat);

            Data.DriverSeat.SetInputData(null);
        }

        private void ChangeSeat()
        {
            var seat = Data.Vehicle.TryChangeSeat(_baseCharacterController);
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