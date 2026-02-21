using System.Collections.Generic;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using NetworkTimer = Utils.NetworkTimer;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterBaseState : BaseMovementState
    {
        private PlayerController _playerController;
        private BaseCharacterController.PlayerData _data;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private NetworkAnimator _networkAnimator;
        private UnityEvent _interactionPressed;

        //Server
        private Queue<PlayerInputData.State> _serverInputStateQueue;
        public Queue<PlayerInputData.State> ServerInputStateQueue => _serverInputStateQueue;

        //Client
        private NetworkTimer _networkTimer;
        private CircularBuffer<PlayerInputData.State> _inputsBuffer;
        private CircularBuffer<MovementState> _movementBuffer;
        private MovementState _movementState;

        public CharacterBaseState(PlayerController context, BaseCharacterController.PlayerData data,
            CharacterController characterController, CharacterAnimationController characterAnimationController,NetworkAnimator networkAnimator,
            UnityEvent interactionPressed) : base(context, data, characterController, characterAnimationController)
        {
            _playerController = context;
            _data = data;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _networkAnimator = networkAnimator;
            _interactionPressed = interactionPressed;
        }

        public override void Enter()
        {
            _serverInputStateQueue = new Queue<PlayerInputData.State>();
            _networkTimer = new NetworkTimer();
            _inputsBuffer = new CircularBuffer<PlayerInputData.State>(1024);
            _movementBuffer = new CircularBuffer<MovementState>(1024);
            _movementState.Position = _characterController.transform.position;

            _characterController.enabled = true;
            _characterAnimationController.SwitchToBaseLayer();
            if (_playerController.IsServer)
            {
                _networkAnimator.enabled = true;
                _interactionPressed.AddListener(FindClosestVehicles);
            }
            else
            {
                _networkAnimator.enabled = false;
            }
        }

        public override void Exit()
        {
            _interactionPressed.RemoveListener(FindClosestVehicles);
        }


        public override void Tick(float deltaTime)
        {
            _networkTimer.Update(deltaTime);
            if (_networkTimer.ShouldTick())
            {
                if (_playerController.IsServer)
                {
                    ProcessServerSide();
                }
                else if (_playerController.IsOwner)
                {
                    ProcessClientSide();
                }
            }
        }

        private void ProcessServerSide()
        {
            if (_playerController.IsHost && _playerController.IsOwner)
            {
                _movementState = ProcessMovementState(_playerController.PlayerInput.GetState(),
                    _networkTimer.MinTimeBetweenTicks);
            }
            else if (_serverInputStateQueue.Count > 0)
            {
                while (_serverInputStateQueue.Count > 0)
                {
                    var inputState = _serverInputStateQueue.Dequeue();
                    ;
                    _movementState = ProcessMovementState(inputState,
                        _networkTimer.MinTimeBetweenTicks);
                }

                _playerController.SendMoveClientRpc(_movementState);
            }
        }

        private void ProcessClientSide()
        {
            var inputState = _playerController.PlayerInput.GetState();
            inputState.Tick = _networkTimer.CurrentTick;
            _inputsBuffer.Add(inputState, inputState.Tick);
            _movementState = ProcessMovementState(inputState, _networkTimer.MinTimeBetweenTicks);
            _movementBuffer.Add(_movementState, _movementState.Tick);
            _playerController.SendInputServerRpc(inputState);
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

        private void FindClosestVehicles()
        {
            var colliders =
                Physics.OverlapSphere(_characterController.transform.position, _data.VehicleDetectionRadius);
            if (colliders.Length == 0)
                return;

            CarVehicle nearVehicle = null;
            float closestDistance = float.MaxValue;
            foreach (var collider in colliders)
            {
                if (collider.attachedRigidbody == null)
                    continue;

                if (collider.attachedRigidbody.TryGetComponent(out CarVehicle vehicle) == false)
                    continue;

                if (nearVehicle != null)
                {
                    float distamce = Vector3.Distance(_characterController.transform.position,
                        vehicle.transform.position);
                    if (distamce < closestDistance)
                    {
                        nearVehicle = vehicle;
                        closestDistance = distamce;
                    }
                }
                else
                {
                    closestDistance = Vector3.Distance(_characterController.transform.position,
                        vehicle.transform.position);
                    nearVehicle = vehicle;
                }
            }

            if (nearVehicle == null)
                return;

            if (nearVehicle.HasFreeSeat())
            {
                Context.SwitchStateWithData<CharacterEnterVehicleParamState, CarVehicle>(nearVehicle);
            }
        }
    }
}