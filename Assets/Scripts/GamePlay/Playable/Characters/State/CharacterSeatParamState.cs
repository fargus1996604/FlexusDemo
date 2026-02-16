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
        private VehicleInputData _inputData;

        public CharacterSeatParamState(BaseCharacterController context, CharacterController characterController,
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
            _characterAnimationController.SwitchToSeatLayer();
            _characterAnimationController.ResetBodyOrientation();

            _inputData.InteractPressed.AddListener(ExitVehicle);
            _inputData.ChangeSeatPressed.AddListener(ChangeSeat);
        }

        public override void Exit()
        {
            _inputData.InteractPressed.RemoveListener(ExitVehicle);
            _inputData.ChangeSeatPressed.RemoveListener(ChangeSeat);
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