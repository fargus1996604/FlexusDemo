using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Playable;
using UnityEngine;
using Object = System.Object;

namespace Gameplay.Core.StateMachine
{
    public abstract class ParamBaseState : BaseState
    {
        protected ParamBaseState(IStateContext context) : base(context)
        {
        }


        public abstract object GetData();
        public abstract void PutData(object data);
    }
    
    public abstract class ParamBaseState<T> : ParamBaseState
    {
        public T Data;

        protected ParamBaseState(IStateContext context) : base(context)
        {
        }

        public override void PutData(object data)
        {
            Data = (T)data;
        }

        public override Object GetData()
        {
            return Data;
        }
    }
}