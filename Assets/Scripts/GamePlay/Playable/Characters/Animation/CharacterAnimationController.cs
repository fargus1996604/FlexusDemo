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

        [SerializeField]
        private Transform _bodyOrientationPivot;


        private Vector3 _deltaPosition;
        public Vector3 DeltaPosition => _deltaPosition;

        private readonly int MOVE_X_FLOAT_KEY = Animator.StringToHash("MoveX");
        private readonly int MOVE_Y_FLOAT_KEY = Animator.StringToHash("MoveY");
        private readonly int DASH_BOOLEAN_KEY = Animator.StringToHash("Dash");
        private readonly int FORWARD_LOKOING_FLOAT_KEY = Animator.StringToHash("ForwardLooking");


        private Vector2 _moveDirection = Vector2.zero;

        public void Move(Vector2 direction, Vector3 cameraForward)
        {
            _moveDirection = Vector2.Lerp(_moveDirection, direction, _movingInterpolation * Time.deltaTime);
            CharacterAnimator.SetFloat(MOVE_X_FLOAT_KEY, _moveDirection.x);
            CharacterAnimator.SetFloat(MOVE_Y_FLOAT_KEY, _moveDirection.y);

            if (direction != Vector2.zero)
            {
                var lookRotation = Quaternion.LookRotation(cameraForward);
                lookRotation.x = 0;
                lookRotation.z = 0;
                transform.localRotation = Quaternion.LerpUnclamped(transform.localRotation, lookRotation,
                    _movingInterpolation * Time.deltaTime);

                if (Mathf.Abs(_moveDirection.y) > 0.9f)
                {
                    CharacterAnimator.SetFloat(FORWARD_LOKOING_FLOAT_KEY,GetForwardLooking(cameraForward));
                }
            }
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