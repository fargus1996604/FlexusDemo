using System.Collections.Generic;
using System.Text;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using Test;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Playable.Characters.State.Server
{
    public class CharacterBaseState : BaseMovementState
    {
        private PlayerController _playerController;
        private BaseCharacterController.PlayerData _data;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private UnityEvent _interactionPressed;

        private NetworkTimer _networkTimer;
        private MovementState _movementState;

        private Queue<PlayerInputData.State> _serverInputStateQueue;
        public Queue<PlayerInputData.State> ServerInputStateQueue => _serverInputStateQueue;

        public CharacterBaseState(PlayerController context, BaseCharacterController.PlayerData data,
            CharacterController characterController, CharacterAnimationController characterAnimationController,
            UnityEvent interactionPressed) : base(context, data, characterController, characterAnimationController)
        {
            _playerController = context;
            _data = data;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _interactionPressed = interactionPressed;
        }

        public override void Tick(float deltaTime)
        {
            _networkTimer.Update(deltaTime);
            if (_networkTimer.ShouldTick())
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
        }

        public override void Enter()
        {
            _networkTimer = new NetworkTimer();
            _serverInputStateQueue = new Queue<PlayerInputData.State>();
            _movementState.Position = _characterController.transform.position;
            _characterController.enabled = true;
            _characterAnimationController.SwitchToBaseLayer();
            _interactionPressed.AddListener(FindClosestVehicles);
        }

        public override void Exit()
        {
            _interactionPressed.RemoveListener(FindClosestVehicles);
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