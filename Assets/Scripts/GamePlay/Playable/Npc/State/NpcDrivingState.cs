using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Input;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Playable.Npc.State
{
    public class NpcDrivingState : TickableParamBaseState<NpcDrivingState.VehicleData>
    {
        public struct VehicleData
        {
            public CarVehicle Vehicle;
            public DriverSeat DriverSeat;
        }
    
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private CarVehicle.InputData _inputData;

        public NpcDrivingState(IStateContext context,CharacterController characterController,
            CharacterAnimationController characterAnimationController, CarVehicle.InputData inputData) :
            base(context)
        {
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputData = inputData;
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void Enter()
        {
            _characterController.enabled = false;
            _characterAnimationController.SwitchToDrivingLayer();
            _characterAnimationController.ResetBodyOrientation();
            Data.DriverSeat.SetInputData(_inputData);
        }

        public override void Exit()
        {
            Data.DriverSeat.SetInputData(default);
        }
    }
}
