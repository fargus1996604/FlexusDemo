using UnityEngine;

namespace GamePlay.Vehicle.Car
{
    [RequireComponent(typeof(CarEngine))]
    public class CarController : MonoBehaviour
    {
        private CarEngine _engine;
        public CarEngine Engine => _engine ??= GetComponent<CarEngine>();

        [SerializeField]
        private float _maxSteeringAngle;

        [SerializeField]
        private WheelCollider[] _wheelColliders;

        [SerializeField]
        private WheelCollider[] _steeringWheels;

        [SerializeField]
        private WheelCollider[] _brakeWheels;

        [SerializeField]
        private Transform[] _wheelsTransforms;

        [SerializeField]
        private float _steeringSpeed;

        [SerializeField]
        private float _brakeTorque;
        
        private float _throttleInput;
        private float _steeringInput;
        private float _steeringAngle;
        private bool _brake;

        private void Update()
        {
            UpdateSteeringWheels();
            AppleBrake();
            UpdateWheelsVisual();
            Engine.SetThrottle(_throttleInput);
        }

        private void AppleBrake()
        {
            foreach (var wheelCollider in _brakeWheels)
            {
                wheelCollider.brakeTorque = _brake ? _brakeTorque : 0;
            }
        }

        private void UpdateSteeringWheels()
        {
            _steeringAngle = Mathf.Lerp(_steeringAngle, _steeringInput * _maxSteeringAngle,
                _steeringSpeed * Time.deltaTime);
            foreach (var steeringWheel in _steeringWheels)
            {
                steeringWheel.steerAngle = _steeringAngle;
            }
        }

        private void UpdateWheelsVisual()
        {
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                if (_wheelsTransforms.Length - 1 < i)
                    break;
                _wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion rotation);
                _wheelsTransforms[i].SetPositionAndRotation(position, rotation);
            }
        }

        public void SetThrottle(float throttle)
        {
            _throttleInput = throttle;
        }

        public void SetSteering(float steering)
        {
            _steeringInput = steering;
        }

        public void SetBrake(bool brake)
        {
            _brake = brake;
        }
    }
}