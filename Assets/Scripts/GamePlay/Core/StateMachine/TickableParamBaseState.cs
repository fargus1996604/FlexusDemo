using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Playable;
using UnityEngine;

namespace Gameplay.Core.StateMachine
{
    public abstract class TickableParamBaseState<T> : ParamBaseState<T>, ITickable
    {
        protected TickableParamBaseState(IStateContext context) : base(context)
        {
        }

        public abstract void Tick(float deltaTime);
    }
}