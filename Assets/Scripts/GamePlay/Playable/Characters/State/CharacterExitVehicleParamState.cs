using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Vehicle.Car;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterExitVehicleParamState : ParamBaseState<CarVehicle>
    {
        private PlayerController _playerController;

        public CharacterExitVehicleParamState(PlayerController context) : base(context)
        {
            _playerController = context;
        }

        public override void Enter()
        {
            Data.ExitCar(_playerController);
            Context.SwitchState<CharacterExploringState>();
        }

        public override void Exit()
        {
        }
    }
}