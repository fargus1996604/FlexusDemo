using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterBaseState : TickableBaseState
    {
        private BaseCharacterController.PlayerData _data;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private PlayerInputData _inputHandlerData;

        public CharacterBaseState(IStateContext context, BaseCharacterController.PlayerData data,
            CharacterController characterController, CharacterAnimationController characterAnimationController,
            PlayerInputData inputHandlerData) : base(context)
        {
            _data = data;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputHandlerData = inputHandlerData;
        }

        public override void Tick(float deltaTime)
        {
            if (_characterController.isGrounded && _data.Velocity.y < 0)
            {
                _data.Velocity.y = -2f;
            }

            _data.Velocity.y += _data.Gravity * Time.deltaTime;
            _characterController.Move(_characterAnimationController.DeltaPosition + (_data.Velocity * deltaTime));

            var cameraLook = _inputHandlerData.MoveDirection;

            if (_inputHandlerData.Axes != Vector2.zero)
            {
                var lookRotation = Quaternion.LookRotation(cameraLook);
                lookRotation.x = 0;
                lookRotation.z = 0;
                _characterController.transform.rotation = Quaternion.LerpUnclamped(_characterController.transform.rotation, lookRotation,
                    _characterAnimationController.MovingInterpolation * Time.deltaTime);
            }
            
            _characterAnimationController.Move(_inputHandlerData.Axes, cameraLook);
            _characterAnimationController.SetDash(_inputHandlerData.IsSprinting);
        }

        public override void Enter()
        {
            _characterController.enabled = true;
            _characterAnimationController.SwitchToBaseLayer();
            _inputHandlerData.InteractPressed.AddListener(FindClosestVehicles);
        }

        public override void Exit()
        {
            _inputHandlerData.InteractPressed.RemoveListener(FindClosestVehicles);
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
                    float distamce = Vector3.Distance(_characterController.transform.position, vehicle.transform.position);
                    if (distamce < closestDistance)
                    {
                        nearVehicle = vehicle;
                        closestDistance = distamce;
                    }
                }
                else
                {
                    closestDistance = Vector3.Distance(_characterController.transform.position, vehicle.transform.position);
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