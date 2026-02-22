using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.State.StateParam;
using GamePlay.Vehicle.Car;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterExitVehicleParamState : ParamBaseState<LeaveVehicleSeat>
    {
        private BaseCharacterController _playerController;

        public CharacterExitVehicleParamState(BaseCharacterController context) : base(context)
        {
            _playerController = context;
        }

        public override void Enter()
        {
            if (_playerController.IsServer)
            {
                Data.Vehicle.TryExitCar(_playerController);
                Context.SwitchState<CharacterBaseState>();
            }
            else
            {
                _playerController.transform.position = Data.Seat.DoorPlayerPivot.position;
            }
        }

        public override void Exit()
        {
        }
    }
}