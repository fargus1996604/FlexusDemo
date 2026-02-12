using System;
using GamePlay.Input;
using GamePlay.Playable.Characters.Animation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GamePlay.Playable
{
    public class PlayerController : MonoBehaviour, IInputHandler
    {
        [FormerlySerializedAs("animationController")]
        [FormerlySerializedAs("_animatorController")]
        [SerializeField]
        private CharacterAnimationController _animationController;

        [SerializeField]
        private CharacterController _characterController;

        private float _gravity = -9.81f;
        private bool _isGrounded;
        private Vector3 _velocity;
        private Vector2 _movement;

        private void Update()
        {
            _isGrounded = _characterController.isGrounded;
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            _velocity.y += _gravity * Time.deltaTime;
            _characterController.Move(_animationController.DeltaPosition + (_velocity * Time.deltaTime));

            var cameraLook = Camera.main.transform.forward;
            _animationController.Move(_movement, cameraLook);
        }

        public void OnMove(Vector2 axes)
        {
            _movement = axes;
        }

        public void OnSprint(bool engaged)
        {
            _animationController.SetDash(engaged);
        }
    }
}