using UnityEngine;

namespace GamePlay.Playable.Characters.Animation
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimationController : MonoBehaviour
    {
        private Animator _characterAnimator;
        protected Animator CharacterAnimator => _characterAnimator ??= GetComponent<Animator>();

        [SerializeField]
        private float _movingInterpolation = 1f;
        public float MovingInterpolation => _movingInterpolation;

        [SerializeField]
        private Transform _bodyOrientationPivot;

        private Vector3 _deltaPosition;
        public Vector3 DeltaPosition => _deltaPosition;

        private readonly int MOVE_X_FLOAT_KEY = Animator.StringToHash("MoveX");
        private readonly int MOVE_Y_FLOAT_KEY = Animator.StringToHash("MoveY");
        private readonly int DASH_BOOLEAN_KEY = Animator.StringToHash("Dash");
        private readonly int FORWARD_LOKOING_FLOAT_KEY = Animator.StringToHash("ForwardLooking");

        private readonly string BASE_LAYER_NAME = "Base Layer";
        private readonly string DRIVING_LAYER_NAME = "Driving Layer";
        private readonly string SEAT_LAYER_NAME = "Seat Layer";
        

        private Vector2 _moveDirection = Vector2.zero;

        public void Move(Vector2 direction, Vector3 cameraForward)
        {
            _moveDirection = Vector2.Lerp(_moveDirection, direction, _movingInterpolation * Time.deltaTime);
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

        public void SwitchToBaseLayer()
        {
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(BASE_LAYER_NAME), 1);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(DRIVING_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(SEAT_LAYER_NAME), 0);
        }

        public void SwitchToDrivingLayer()
        {
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(BASE_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(DRIVING_LAYER_NAME), 1);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(SEAT_LAYER_NAME), 0);
        }
        
        public void SwitchToSeatLayer()
        {
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(BASE_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(DRIVING_LAYER_NAME), 0);
            CharacterAnimator.SetLayerWeight(CharacterAnimator.GetLayerIndex(SEAT_LAYER_NAME), 1);
        }
        
        public void ResetBodyOrientation()
        {
            transform.localRotation = Quaternion.identity;
        }
        
        public void SetDash(bool dash)
        {
            CharacterAnimator.SetBool(DASH_BOOLEAN_KEY, dash);
        }

        private void OnAnimatorMove()
        {
            _deltaPosition = CharacterAnimator.deltaPosition;
        }

        private float GetForwardLooking(Vector3 cameraForward)
        {
            return Vector3.Dot(_bodyOrientationPivot.forward, cameraForward) > 0 ? 0f : 1f;
        }
    }
}