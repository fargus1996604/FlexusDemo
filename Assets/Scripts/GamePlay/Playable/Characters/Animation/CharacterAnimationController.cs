using System;
using GamePlay.Vehicle.Car.Weapons;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Playable.Characters.Animation
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimationController : NetworkBehaviour
    {
        private Animator _characterAnimator;
        protected Animator CharacterAnimator => _characterAnimator ??= GetComponent<Animator>();

        public UnityEvent OnFootStep;

        [SerializeField]
        private float _movingInterpolation = 1f;

        public float MovingInterpolation => _movingInterpolation;

        [SerializeField]
        private Transform _bodyOrientationPivot;

        private readonly int MOVE_X_FLOAT_KEY = Animator.StringToHash("MoveX");
        private readonly int MOVE_Y_FLOAT_KEY = Animator.StringToHash("MoveY");
        private readonly int DASH_BOOLEAN_KEY = Animator.StringToHash("Dash");
        private readonly int FORWARD_LOKOING_FLOAT_KEY = Animator.StringToHash("ForwardLooking");

        private readonly string BASE_LAYER_NAME = "Base Layer";
        private readonly string DRIVING_LAYER_NAME = "Driving Layer";
        private readonly string SEAT_LAYER_NAME = "Seat Layer";
        private readonly string MINIGUN_LAYER_NAME = "MiniGun Layer";

        private Vector2 _moveDirection = Vector2.zero;
        private Transform _leftHandIkTarget;
        private Transform _rightHandIkTarget;

        public void Move(Vector2 direction, Vector3 cameraForward, float deltaTime)
        {
            _moveDirection = Vector2.Lerp(_moveDirection, direction, _movingInterpolation * deltaTime);
            CharacterAnimator.SetFloat(MOVE_X_FLOAT_KEY, _moveDirection.x);
            CharacterAnimator.SetFloat(MOVE_Y_FLOAT_KEY, _moveDirection.y);

            if (direction != Vector2.zero)
            {
                if (Mathf.Abs(_moveDirection.y) > 0.9f)
                {
                    CharacterAnimator.SetFloat(FORWARD_LOKOING_FLOAT_KEY, GetForwardLooking(cameraForward));
                }
            }
        }

        public void SetDash(bool dash)
        {
            CharacterAnimator.SetBool(DASH_BOOLEAN_KEY, dash);
        }

        public void SwitchToBaseLayer()
        {
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(BASE_LAYER_NAME), 1);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(DRIVING_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(SEAT_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(MINIGUN_LAYER_NAME), 0);
        }

        public void SwitchToDrivingLayer()
        {
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(BASE_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(DRIVING_LAYER_NAME), 1);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(SEAT_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(MINIGUN_LAYER_NAME), 0);
        }

        public void SwitchToSeatLayer()
        {
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(BASE_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(DRIVING_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(SEAT_LAYER_NAME), 1);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(MINIGUN_LAYER_NAME), 0);
        }

        public void SwitchToMiniGunLayer()
        {
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(BASE_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(DRIVING_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(SEAT_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(MINIGUN_LAYER_NAME), 1);
        }

        public void ResetBodyOrientation()
        {
            transform.localRotation = Quaternion.identity;
        }

        [Rpc(SendTo.Owner)]
        public void SetLeftHandIKTargetRpc(NetworkObjectReference networkObjectReference)
        {
            if (networkObjectReference.TryGet(out NetworkObject networkObject) == false)
                return;
            _leftHandIkTarget = networkObject.transform;
        }

        [Rpc(SendTo.Owner)]
        public void SetRightHandIKTargetRpc(NetworkObjectReference networkObjectReference)
        {
            if (networkObjectReference.TryGet(out NetworkObject networkObject) == false)
                return;
            _rightHandIkTarget = networkObject.transform;
        }

        [Rpc(SendTo.Everyone)]
        public void SetMiniGunIKTargetsRpc(NetworkBehaviourReference networkBehaviourReference)
        {
            if (networkBehaviourReference.TryGet(out MiniGunController miniGunController) == false)
                return;
            _leftHandIkTarget = miniGunController.LeftHandTarget.transform;
            _rightHandIkTarget = miniGunController.RightHandTarget.transform;
        }

        [Rpc(SendTo.Everyone)]
        public void ResetAllIkTargetsRpc()
        {
            _leftHandIkTarget = null;
            _rightHandIkTarget = null;

            CharacterAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            CharacterAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        }
        
        public void CallFootStepEvent()
        {
            OnFootStep?.Invoke();
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            if (_leftHandIkTarget != null)
            {
                CharacterAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                CharacterAnimator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandIkTarget.position);
            }

            if (_rightHandIkTarget != null)
            {
                CharacterAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                CharacterAnimator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandIkTarget.position);
            }
        }

        private float GetForwardLooking(Vector3 cameraForward)
        {
            return Vector3.Dot(_bodyOrientationPivot.forward, cameraForward) > 0 ? 0f : 1f;
        }
    }
}