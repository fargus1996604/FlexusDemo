using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Input;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterSeatParamState : ParamBaseState<CharacterSeatParamState.VehicleData>
    {
        public struct VehicleData
        {
            public CarVehicle Vehicle;
            public Seat Seat;
        }

        private BaseCharacterController _baseCharacterController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private VehicleInputHandler _inputHandler;

        public CharacterSeatParamState(BaseCharacterController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, VehicleInputHandler inputHandler) :
            base(context)
        {
            _baseCharacterController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputHandler = inputHandler;
        }

        public override void Enter()
        {
            _characterController.enabled = false;
            _characterAnimationController.SwitchToSeatLayer();
            _characterAnimationController.ResetBodyOrientation();

            _inputHandler.InteractPressed.AddListener(ExitVehicle);
            _inputHandler.ChangeSeatPressed.AddListener(ChangeSeat);
            _inputHandler.Enable();
        }

        public override void Exit()
        {
            _inputHandler.InteractPressed.RemoveListener(ExitVehicle);
            _inputHandler.ChangeSeatPressed.RemoveListener(ChangeSeat);
            _inputHandler.Disable();
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