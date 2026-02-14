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
        
        [SerializeField]
        private Transform _leftHandTarget;
        public Transform LeftHandTarget => _leftHandTarget;
        
        [SerializeField]
        private Transform _rightHandTarget;
        public Transform RightHandTarget => _rightHandTarget;
        
        [SerializeField]
        private Transform _gunMesh;

        [SerializeField]
        private Transform _barrel;
        
        [SerializeField]
        private int _maxRpm;

        [SerializeField]
        private float _power;
        
        [SerializeField]
        private float _currentRpm;
        
        private InputData _inputData;
        
        public void SetInputData(InputData inputData)
        {
            _inputData = inputData;
        }

        public void ResetLookDirection()
        {
            _gunMesh.transform.forward = transform.forward;
        }

        public void Tick(float deltaTime)
        {
            _gunMesh.transform.forward = _inputData.LookDirection;
            _currentRpm += _inputData.Fire ? _power : -_power;
            _currentRpm = Mathf.Clamp(_currentRpm, 0, _maxRpm);
            _barrel.localRotation *= Quaternion.Euler(0, 0, _currentRpm * deltaTime);
        }
    }
}