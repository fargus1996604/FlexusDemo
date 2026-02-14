using Gameplay.Core.StateMachine.Interfaces;
using UnityEngine;

namespace Gameplay.Core.StateMachine
{
    public abstract class TickableBaseState : BaseState, ITickable
    {
        protected TickableBaseState(IStateContext context) : base(context)
        {
            
        }
        
        public abstract void Tick(float deltaTime);
    }
}