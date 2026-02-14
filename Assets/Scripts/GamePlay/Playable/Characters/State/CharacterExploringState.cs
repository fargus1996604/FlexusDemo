using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Input;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterExploringState : TickableBaseState
    {
        private PlayerController.PlayerData _data;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private PlayerInputHandler _inputHandler;
        private Camera _camera;

        public CharacterExploringState(IStateContext context, PlayerController.PlayerData data,
            CharacterController characterController, CharacterAnimationController characterAnimationController,
            PlayerInputHandler inputHandler,
            Camera camera) : base(context)
        {
            _data = data;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputHandler = inputHandler;
            _camera = camera;
        }

        public override void Tick(float deltaTime)
        {
            if (_characterController.isGrounded && _data.Velocity.y < 0)
            {
                _data.Velocity.y = -2f;
            }

            _data.Velocity.y += _data.Gravity * Time.deltaTime;
            _characterController.Move(_characterAnimationController.DeltaPosition + (_data.Velocity * deltaTime));

            var cameraLook = _camera.transform.forward;

            if (_inputHandler.Move != Vector2.zero)
            {
                var lookRotation = Quaternion.LookRotation(cameraLook);
                lookRotation.x = 0;
                lookRotation.z = 0;
                _characterController.transform.rotation = Quaternion.LerpUnclamped(_characterController.transform.rotation, lookRotation,
                    _characterAnimationController.MovingInterpolation * Time.deltaTime);
            }
            
            _characterAnimationController.Move(_inputHandler.Move, cameraLook);
            _characterAnimationController.SetDash(_inputHandler.IsSprinting);
        }

        public override void Enter()
        {
            _characterController.enabled = true;
            _characterAnimationController.SwitchToBaseLayer();
            _inputHandler.InteractPressed.AddListener(FindClosestVehicles);
            _inputHandler.Enable();
        }

        public override void Exit()
        {
            _inputHandler.InteractPressed.RemoveListener(FindClosestVehicles);
            _inputHandler.Disable();
        }
        
        private void FindClosestVehicles()
        {
            var colliders =
                Physics.OverlapSphere(_characterController.transform.position, _data.VehicleDetectionRadius);
            if (colliders.Length == 0)
                return;

            CarVehicle nearVehicle = null;
            foreach (var collider in colliders)
            {
                if (collider.attachedRigidbody == null)
                    continue;

                if (collider.attachedRigidbody.TryGetComponent(out CarVehicle vehicle) == false)
                    continue;

                nearVehicle = vehicle;
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