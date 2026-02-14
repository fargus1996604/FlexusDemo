using Gameplay.Core.StateMachine.Interfaces;
using UnityEngine;

namespace Gameplay.Core.StateMachine
{
    public abstract class BaseState : IState
    {
        protected IStateContext Context;

        protected BaseState(IStateContext context)
        {
            Context = context;
        }

        public abstract void Enter();
        public abstract void Exit();
    }
}