using Gameplay.Core.StateMachine.Interfaces;
using UnityEngine;

namespace Gameplay.Core.StateMachine
{
    public abstract class ParamBaseState<T> : BaseState
    {
        protected T Data;

        protected ParamBaseState(IStateContext context) : base(context)
        {
        }

        public void PutData(T data)
        {
            Data = data;
        }
    }
}