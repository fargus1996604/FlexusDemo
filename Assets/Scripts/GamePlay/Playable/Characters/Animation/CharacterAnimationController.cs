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
        private Vector4 _originalRotation;

        [SerializeField]
        private float _anlge;

        private Vector3 _deltaPosition;
        public Vector3 DeltaPosition => _deltaPosition;

        private readonly int MOVE_X_FLOAT_KEY = Animator.StringToHash("MoveX");
        private readonly int MOVE_Y_FLOAT_KEY = Animator.StringToHash("MoveY");
        private readonly int DASH_BOOLEAN_KEY = Animator.StringToHash("Dash");


        private Vector2 _moveDirection = Vector2.zero;

        public void Move(Vector2 direction, Vector3 cameraForward)
        {
            _moveDirection = Vector2.Lerp(_moveDirection, direction, _movingInterpolation * Time.deltaTime);
            CharacterAnimator.SetFloat(MOVE_X_FLOAT_KEY, _moveDirection.x);
            CharacterAnimator.SetFloat(MOVE_Y_FLOAT_KEY, _moveDirection.y);

            if (direction != Vector2.zero)
            {
                float angle = 0;
                if (direction.y < 0)
                {
                    angle = direction.x < 0 ? -180 : 180;
                }

                _anlge = angle;
                var lookDirection = direction.y >= 0
                    ? Quaternion.identity
                    : Quaternion.AngleAxis(angle, Vector3.up);

                lookDirection = Quaternion.LookRotation(cameraForward) * lookDirection;
                lookDirection.x = 0;
                lookDirection.z = 0;

                transform.localRotation = Quaternion.LerpUnclamped(transform.localRotation, lookDirection,
                    _movingInterpolation * Time.deltaTime);
            }

            _originalRotation = new Vector4(transform.localRotation.x, transform.localRotation.y,
                transform.localRotation.z, transform.localRotation.z);
        }

        public void SetDash(bool dash)
        {
            CharacterAnimator.SetBool(DASH_BOOLEAN_KEY, dash);
        }

        private void OnAnimatorMove()
        {
            _deltaPosition = CharacterAnimator.deltaPosition;
        }
    }
}