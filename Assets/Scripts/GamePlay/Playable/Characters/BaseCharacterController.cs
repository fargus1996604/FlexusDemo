using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core;
using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Playable.Characters.Animation;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace GamePlay.Playable.Characters
{
    [RequireComponent(typeof(CharacterController), typeof(NetworkTransform), typeof(NetworkAnimator))]
    public abstract class BaseCharacterController : NetworkBehaviour, IStateContext
    {
        public UnityEvent<Type, object> OnStateBeginChange;

        [System.Serializable]
        public class PlayerData
        {
            public float Gravity = -9.81f;
            public Vector3 Velocity;
            public float VehicleDetectionRadius;
            public float MoveSpeed;
            public float DashSpeed;
        }

        [SerializeField]
        private PlayerData _data;

        public PlayerData Data => _data;

        [SerializeField]
        private CharacterController _characterController;

        protected CharacterController CharacterController =>
            _characterController ??= GetComponent<CharacterController>();

        private NetworkTransform _networkTransform;
        protected NetworkTransform NetworkTransform => _networkTransform ??= GetComponent<NetworkTransform>();

        private NetworkAnimator _networkAnimator;
        protected NetworkAnimator NetworkAnimator => _networkAnimator ??= GetComponent<NetworkAnimator>();

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
            OnStateChangedForNetwork<T, TD>(data);
            OnStateBeginChange?.Invoke(State.GetType(), data);
            State?.Enter();
        }

        public void SwitchStateWithReferenceData(Type stateType, NetworkBehaviourReference[] references)
        {
            var pendingState = States.Find(state => state.GetType() == stateType);
            IStateNetworkData data = null;
            if (references != null && pendingState is ParamBaseState paramBaseState)
            {
                if (paramBaseState.GetData() is not IStateNetworkData paramData)
                {
                    Debug.LogError($"Data of {stateType} is not IStateNetworkData: {paramBaseState.GetData().GetType()}");
                    return;
                }

                if (references.Length == 0)
                {
                    Debug.LogError($"Data is empty: {pendingState.GetType()}");
                    return;
                }

                data = paramData;
                data.Boxing(references);
                if (data.IsValid() == false)
                {
                    Debug.LogError($"Can't pack references for type: {stateType} refCount: {references.Length}");
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
            OnStateChangedForNetwork<T, Object>(null);
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
            OnStateChangedForNetwork<BaseState, Object>(null);
            OnStateBeginChange?.Invoke(State.GetType(), null);
            State?.Enter();
        }

        protected abstract void OnStateChangedForNetwork<T, TD>(TD data) where T : BaseState;
    }
}