using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core;
using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Playable.Characters.Animation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace GamePlay.Playable.Characters
{
    [RequireComponent(typeof(CharacterController))]
    public class BaseCharacterController : NetworkBehaviour, IStateContext
    {
        public UnityEvent<Type, object> OnStateBeginChange;

        [System.Serializable]
        public class PlayerData
        {
            public float Gravity = -9.81f;
            public Vector3 Velocity;
            public float VehicleDetectionRadius;
        }

        [SerializeField]
        private PlayerData _data;

        public PlayerData Data => _data;

        [SerializeField]
        private CharacterController _characterController;

        protected CharacterController CharacterController =>
            _characterController ??= GetComponent<CharacterController>();

        [SerializeField]
        private CharacterAnimationController _characterAnimationController;

        protected CharacterAnimationController CharacterAnimationController => _characterAnimationController;

        protected List<BaseState> States;
        protected BaseState State;
        protected ITickable TickableState;

        public void SwitchStateWithData<T, TD>(TD data) where T : ParamBaseState<TD>
        {
            var pendingState = States.OfType<T>().FirstOrDefault();
            if (pendingState is ParamBaseState<TD> paramBaseState)
            {
                if (data == null)
                {
                    Debug.LogError($"Data is null: {typeof(T)}");
                    return;
                }

                paramBaseState.PutData(data);
            }

            State?.Exit();
            State = pendingState;
            TickableState = State as ITickable;
            OnStateBeginChange?.Invoke(State.GetType(), data);
            State?.Enter();
        }

        public void SwitchState<T>() where T : BaseState
        {
            var pendingState = States.OfType<T>().FirstOrDefault();
            if (pendingState is ParamBaseState<object>)
            {
                Debug.LogError(
                    $"{pendingState.GetType()} is ParamBaseState. use SwitchStateWithData instead SwitchState ");
                return;
            }

            State?.Exit();
            State = pendingState;
            TickableState = State as ITickable;
            OnStateBeginChange?.Invoke(State.GetType(), null);
            State?.Enter();
        }

        public void SwitchState(Type stateType)
        {
            var pendingState = States.Find(state => state.GetType() == stateType);
            if (pendingState is ParamBaseState<object>)
            {
                Debug.LogError(
                    $"{pendingState.GetType()} is ParamBaseState. use SwitchStateWithData instead SwitchState ");
                return;
            }

            State?.Exit();
            State = pendingState;
            TickableState = State as ITickable;
            OnStateBeginChange?.Invoke(State.GetType(), null);
            State?.Enter();
        }
    }
}