using Gameplay.Core;
using UnityEngine;

namespace GamePlay.Vehicle.Car.Weapons
{
    public class MiniGunController : MonoBehaviour, ITickable
    {
        public class InputData
        {
            public Vector3 LookDirection;
            public bool Fire;
        }

        private InputData _data;
        public InputData Data => _data;

        [SerializeField]
        private Transform _leftHandTarget;

        public Transform LeftHandTarget => _leftHandTarget;

        [SerializeField]
        private Transform _rightHandTarget;

        public Transform RightHandTarget => _rightHandTarget;

        [SerializeField]
        private ParticleSystem _muzzleFlashParticle;

        [SerializeField]
        private Transform _gunMesh;

        [SerializeField]
        private Transform _barrel;

        [SerializeField]
        private int _maxRpm;

        [SerializeField]
        private float _power;

        [SerializeField]
        private float _decreaseSpeed;

        [SerializeField]
        private float _currentRpm;

        [SerializeField]
        private MiniGunSound _sound;

        public bool CanFire => _currentRpm >= _maxRpm;
        public float Charge => _currentRpm / _maxRpm;

        public void SetInputData(InputData inputData)
        {
            _data = inputData;
        }

        public void ResetGun()
        {
            _gunMesh.transform.forward = transform.forward;
            _muzzleFlashParticle.Stop();
            _sound.StopAllSounds();
        }

        public void Tick(float deltaTime)
        {
            _gunMesh.transform.forward = _data.LookDirection;
            _currentRpm += (_data.Fire ? _power : -_decreaseSpeed) * deltaTime;
            _currentRpm = Mathf.Clamp(_currentRpm, 0, _maxRpm);
            _barrel.localRotation *= Quaternion.Euler(0, 0, _currentRpm * deltaTime);

            if (CanFire)
            {
                if (Physics.Raycast(_gunMesh.position, _gunMesh.forward, out RaycastHit hit, 30))
                {
                    if (hit.rigidbody != null)
                    {
                        Vector3 force = (hit.transform.position - hit.point).normalized * 300;
                        hit.rigidbody.AddForce(force, ForceMode.Impulse);
                    }
                }
                
                
                if (_muzzleFlashParticle.isPlaying == false)
                {
                    _muzzleFlashParticle.Play();
                }
            }
            else
            {
                if (_muzzleFlashParticle.isPlaying)
                {
                    _muzzleFlashParticle.Stop();
                }
            }

            _sound.Tick(deltaTime, this);
        }
    }
}