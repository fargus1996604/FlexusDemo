using System;
using Gameplay.Core;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay.Vehicle.Car.Weapons
{
    public class MiniGunController : NetworkBehaviour
    {
        public MiniGunInputData InputData = new();
        
        [SerializeField]
        private NetworkObject _leftHandTarget;

        public NetworkObject LeftHandTarget => _leftHandTarget;

        [SerializeField]
        private NetworkObject _rightHandTarget;

        public NetworkObject RightHandTarget => _rightHandTarget;

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
        private float _lookSmooth;

        [SerializeField]
        private float _currentRpm;

        [SerializeField]
        private MiniGunSound _sound;

        public bool CanFire => _currentRpm >= _maxRpm;
        public float Charge => _currentRpm / _maxRpm;

        private NetworkVariable<bool> _activated = new NetworkVariable<bool>();

        public void ResetGun()
        {
            _gunMesh.transform.forward = transform.forward;
            _muzzleFlashParticle.Stop();
            _sound.StopAllSounds();
        }

        private void Update()
        {
            if (_activated.Value == false)
                return;

            _gunMesh.transform.forward = Vector3.Lerp(_gunMesh.transform.forward, InputData.LookDirection,
                Time.deltaTime * _lookSmooth);
            _currentRpm += (InputData.Fire ? _power : -_decreaseSpeed) * Time.deltaTime;
            _currentRpm = Mathf.Clamp(_currentRpm, 0, _maxRpm);
            _barrel.localRotation *= Quaternion.Euler(0, 0, _currentRpm * Time.deltaTime);

            if (CanFire)
            {
                if (IsServer)
                {
                    if (Physics.Raycast(_gunMesh.position, _gunMesh.forward, out RaycastHit hit, 30))
                    {
                        if (hit.rigidbody != null)
                        {
                            Vector3 force = (hit.transform.position - hit.point).normalized * 300;
                            hit.rigidbody.AddForce(force, ForceMode.Impulse);
                        }
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

            _sound.Tick(Time.deltaTime, this);
        }

        public void Activate()
        {
            _activated.Value = true;
            _activated.SetDirty(true);
        }

        public void Deactivate()
        {
            _activated.Value = false;
            _activated.SetDirty(true);
        }
    }

    [System.Serializable]
    public class MiniGunInputData 
    {
        public Vector3 LookDirection;
        public bool Fire;
    }
}