using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Extensions;
using GamePlay.Playable.Characters.State.StateParam;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterChangeSeatParamState : ParamBaseState<ChangeSeatData>
    {
        private PlayerController _playerController;

        public CharacterChangeSeatParamState(PlayerController context) : base(context)
        {
            _playerController = context;
        }

        public override void Enter()
        {
            if (_playerController.IsServer == false)
                return;

            Data.Seat.SwitchSeatState(Context, Data.Vehicle);
        }

        public override void Exit()
        {
        }
    }
}