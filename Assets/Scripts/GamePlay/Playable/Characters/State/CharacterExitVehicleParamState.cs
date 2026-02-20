using Gameplay.Core.StateMachine;
using GamePlay.Vehicle.Car;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterExitVehicleParamState : ParamBaseState<CarVehicle>
    {
        private BaseCharacterController _playerController;

        public CharacterExitVehicleParamState(BaseCharacterController context) : base(context)
        {
            _playerController = context;
        }

        public override void Enter()
        {
            if(_playerController.IsServer == false)
                return;
            
            if (Data.TryExitCar(_playerController))
            {
                Context.SwitchState<CharacterBaseState>();
            }
            else
            {
                Context.SwitchState<CharacterBaseState>();
            }
        }

        public override void Exit()
        {
        }
    }
}