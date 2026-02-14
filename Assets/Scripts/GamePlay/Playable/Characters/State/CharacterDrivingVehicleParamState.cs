using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Input;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class
        CharacterDrivingVehicleParamState : TickableParamBaseState<CharacterDrivingVehicleParamState.VehicleData>
    {
        public struct VehicleData
        {
            public CarVehicle Vehicle;
            public DriverSeat DriverSeat;
        }

        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private VehicleInputHandler _inputHandler;

        public CharacterDrivingVehicleParamState(IStateContext context,CharacterController characterController,
            CharacterAnimationController characterAnimationController, VehicleInputHandler inputHandler) :
            base(context)
        {
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputHandler = inputHandler;
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void Enter()
        {
            _characterController.enabled = false;
            _characterAnimationController.SwitchToDrivingLayer();
            _characterAnimationController.ResetBodyOrientation();
            _inputHandler.InteractPressed.AddListener(ExitVehicle);
            _inputHandler.Enable();

            Data.DriverSeat.SetInputData(_inputHandler.CarVehicleInputData);
        }

        public override void Exit()
        {
            _inputHandler.InteractPressed.RemoveListener(ExitVehicle);
            _inputHandler.Disable();

            Data.DriverSeat.SetInputData(null);
        }

        private void ExitVehicle()
        {
            Context.SwitchStateWithData<CharacterExitVehicleParamState, CarVehicle>(Data.Vehicle);
        }
    }
}