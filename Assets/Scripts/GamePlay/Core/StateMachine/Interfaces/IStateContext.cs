using System;

namespace Gameplay.Core.StateMachine.Interfaces
{
    public interface IStateContext
    {
        void SwitchStateWithData<T, TD>(TD data) where T : ParamBaseState<TD>;
        void SwitchState<T>() where T : BaseState;
        void SwitchState(Type stateType);
    }
}
