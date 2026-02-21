using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Extensions;
using GamePlay.Playable.Characters.State.StateParam;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterEnterVehicleParamState : ParamBaseState<CarVehicle>
    {
        private BaseCharacterController _playerController;

        public CharacterEnterVehicleParamState(BaseCharacterController context) : base(context)
        {
            _playerController = context;
        }

        public override void Enter()
        {
            if (_playerController.IsServer == false)
                return;

            var freeSeat = Data.TryEnterCar(_playerController);
            freeSeat.SwitchSeatState(Context, Data);
        }

        public override void Exit()
        {
        }
    }
}