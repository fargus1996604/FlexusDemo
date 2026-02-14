using Gameplay.Core.StateMachine;
using GamePlay.Input.InputHandler;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
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
        private VehicleInputHandler _inputHandler;
        private CameraController _cameraController;

        public CharacterDrivingVehicleParamState(BaseCharacterController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, VehicleInputHandler inputHandler,CameraController cameraController) :
            base(context)
        {
            _baseCharacterController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputHandler = inputHandler;
            _cameraController = cameraController;
        }

        public override void Enter()
        {
            _cameraController.ActivateDefaultCamera(Data.Vehicle.transform);
            _characterController.enabled = false;
            _characterAnimationController.SwitchToDrivingLayer();
            _characterAnimationController.ResetBodyOrientation();
            _inputHandler.InteractPressed.AddListener(ExitVehicle);
            _inputHandler.ChangeSeatPressed.AddListener(ChangeSeat);
            _inputHandler.Enable();

            Data.DriverSeat.SetInputData(_inputHandler.CarVehicleInputData);
        }

        public override void Exit()
        {
            _inputHandler.InteractPressed.RemoveListener(ExitVehicle);
            _inputHandler.ChangeSeatPressed.RemoveListener(ChangeSeat);
            _inputHandler.Disable();

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