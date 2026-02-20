using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Playable.Characters.Animation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Playable.Characters.State
{
    public abstract class BaseMovementState : TickableBaseState
    {
        private BaseCharacterController.PlayerData _data;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;

        protected BaseMovementState(IStateContext context, BaseCharacterController.PlayerData data,
            CharacterController characterController,
            CharacterAnimationController characterAnimationController) : base(context)
        {
            _data = data;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
        }

        private void AppleGravity(float deltaTime)
        {
            if (_characterController.isGrounded && _data.Velocity.y < 0)
            {
                _data.Velocity.y = -2f;
            }

            _data.Velocity.y += _data.Gravity * deltaTime;
        }

        public MovementState ProcessMovementState(PlayerInputData.State input, float tickDelta)
        {
            AppleGravity(tickDelta);
            float speed = input.IsSprinting ? _data.DashSpeed : _data.MoveSpeed * Mathf.Abs(input.Axes.magnitude);
            var moveDelta = new Vector3(input.Axes.x, 0, input.Axes.y) * (speed * tickDelta);
            _characterController.Move(_characterController.transform.TransformDirection(moveDelta) +
                                      (_data.Velocity * tickDelta));

            var cameraLook = input.MoveDirection;
            _characterAnimationController.Move(input.Axes, cameraLook, tickDelta);
            _characterAnimationController.SetDash(input.IsSprinting);

            if (input.Axes != Vector2.zero)
            {
                var lookRotation = Quaternion.LookRotation(cameraLook);
                lookRotation.x = 0;
                lookRotation.z = 0;
                _characterController.transform.rotation = Quaternion.LerpUnclamped(
                    _characterController.transform.rotation, lookRotation,
                    _characterAnimationController.MovingInterpolation * tickDelta);
            }

            return new MovementState()
            {
                Tick = input.Tick,
                Position = _characterController.transform.position,
                Direction = _characterController.transform.forward,
                MoveDelta = moveDelta
            };
        }
    }

    [System.Serializable]
    public struct MovementState : INetworkSerializable
    {
        public int Tick;
        public Vector3 Position;
        public Vector3 Direction;

        [FormerlySerializedAs("DeltaPosition")]
        public Vector3 MoveDelta;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Direction);
            serializer.SerializeValue(ref MoveDelta);
        }
    }
}