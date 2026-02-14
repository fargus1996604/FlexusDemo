using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core;
using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Input;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State;
using UnityEngine;


namespace GamePlay.Playable
{
    public class PlayerController : MonoBehaviour, IStateContext
    {
        [System.Serializable]
        public class PlayerData
        {
            public float Gravity = -9.81f;
            public Vector3 Velocity;
            public float VehicleDetectionRadius;
        }

        [SerializeField]
        private PlayerData _playerData;

        [SerializeField]
        private CharacterAnimationController _animationController;

        [SerializeField]
        private CharacterController _characterController;

        [SerializeField]
        private InputController _inputController;

        private List<BaseState> _states;
        private BaseState _state;
        private ITickable _tickableState;

        private void Start()
        {
            _states = new List<BaseState>()
            {
                new CharacterExploringState(this, _playerData, _characterController, _animationController,
                    _inputController.GetPlayerInputHandler(), Camera.main),
                new CharacterEnterVehicleParamState(this),
                new CharacterExitVehicleParamState(this),
                new CharacterDrivingVehicleParamState(this, _characterController, _animationController,
                    _inputController.GetVehicleInputHandler())
            };

            SwitchState<CharacterExploringState>();
        }

        private void Update()
        {
            if (_tickableState != null)
                _tickableState.Tick(Time.deltaTime);
        }

        public void SwitchStateWithData<T, TD>(TD data) where T : ParamBaseState<TD>
        {
            var pendingState = _states.OfType<T>().FirstOrDefault();
            if (pendingState is ParamBaseState<TD> paramBaseState)
            {
                if (data == null)
                {
                    Debug.LogError($"Data is null: {typeof(T)}");
                    return;
                }

                paramBaseState.PutData(data);
            }

            _state?.Exit();
            _state = pendingState;
            _tickableState = _state as ITickable;
            _state?.Enter();
        }

        public void SwitchState<T>() where T : BaseState
        {
            var pendingState = _states.OfType<T>().FirstOrDefault();
            if (pendingState is ParamBaseState<object>)
            {
                Debug.LogError(
                    $"{pendingState.GetType()} is ParamBaseState. use SwitchStateWithData instead SwitchState ");
                return;
            }

            _state?.Exit();
            _state = pendingState;
            _tickableState = _state as ITickable;
            _state?.Enter();
        }

        public void SwitchState(Type stateType)
        {
            var pendingState = _states.Find(state => state.GetType() == stateType);
            if (pendingState is ParamBaseState<object>)
            {
                Debug.LogError(
                    $"{pendingState.GetType()} is ParamBaseState. use SwitchStateWithData instead SwitchState ");
                return;
            }

            _state?.Exit();
            _state = pendingState;
            _tickableState = _state as ITickable;
            _state?.Enter();
        }
    }
}