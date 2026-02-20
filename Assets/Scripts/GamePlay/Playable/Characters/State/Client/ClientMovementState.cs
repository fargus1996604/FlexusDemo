using System.Collections.Generic;
using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Playable.Characters.Animation;
using Test;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Playable.Characters.State.Client
{
    public class ClientMovementState : BaseMovementState
    {
        private PlayerController _playerController;
        private BaseCharacterController.PlayerData _data;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private PlayerInputData _inputData;
        private NetworkAnimator _networkAnimator;

        private NetworkTimer _networkTimer;
        private CircularBuffer<PlayerInputData.State> _inputsBuffer;
        private CircularBuffer<MovementState> _movementBuffer;
        private MovementState _movementState;

        public ClientMovementState(PlayerController context, BaseCharacterController.PlayerData data,
            CharacterController characterController, CharacterAnimationController characterAnimationController,
            PlayerInputData inputData, NetworkAnimator networkAnimator) : base(context, data, characterController,
            characterAnimationController)
        {
            _playerController = context;
            _data = data;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputData = inputData;
            _networkAnimator = networkAnimator;
        }

        public override void Tick(float deltaTime)
        {
            _networkTimer.Update(deltaTime);
            var inputState = _inputData.GetState();
            if (_networkTimer.ShouldTick())
            {
                inputState.Tick = _networkTimer.CurrentTick;
                _inputsBuffer.Add(inputState, inputState.Tick);
                _movementState = ProcessMovementState(inputState, _networkTimer.MinTimeBetweenTicks);
                _movementBuffer.Add(_movementState, _movementState.Tick);
                _playerController.SendInputServerRpc(inputState);
            }
        }

        public void Reconcile(MovementState serverMovementState)
        {
            float error = Vector3.Distance(serverMovementState.Position,
                _movementBuffer.Get(serverMovementState.Tick).Position);

            if (error < 0.1f)
                return;

            Debug.LogWarning(
                $"Reconciled Start at: {serverMovementState.Tick} network Tick: {_networkTimer.CurrentTick} error: {error} server: {serverMovementState.Position} client:{_movementBuffer.Get(serverMovementState.Tick).Position}");

            uint maxReconciliationTicks = NetworkManager.Singleton.NetworkTickSystem.TickRate;
            if (_networkTimer.CurrentTick - serverMovementState.Tick > maxReconciliationTicks)
            {
                Debug.LogWarning("High Latency: Snapping to server position to prevent teleport.");
                _characterController.transform.position = serverMovementState.Position;
                _movementState = serverMovementState;
                return;
            }

            _movementBuffer.Add(serverMovementState, serverMovementState.Tick);
            _movementState = serverMovementState;
            _characterController.transform.position = _movementState.Position;
            var tick = serverMovementState.Tick + 1;
            while (tick < _networkTimer.CurrentTick)
            {
                Debug.LogWarning($"Reconciled tick: {tick} delta: {_movementBuffer.Get(tick).MoveDelta}");
                _movementState = ProcessMovementState(_inputsBuffer.Get(tick),
                    _networkTimer.MinTimeBetweenTicks);
                _movementBuffer.Add(_movementState, tick);
                tick++;
            }
        }

        public override void Enter()
        {
            _networkTimer = new NetworkTimer();
            _inputsBuffer = new CircularBuffer<PlayerInputData.State>(1024);
            _movementBuffer = new CircularBuffer<MovementState>(1024);
            _movementState.Position = _characterController.transform.position;
            _characterController.enabled = true;
            _characterAnimationController.SwitchToBaseLayer();
            _networkAnimator.enabled = false;
        }

        public override void Exit()
        {
        }
    }
}