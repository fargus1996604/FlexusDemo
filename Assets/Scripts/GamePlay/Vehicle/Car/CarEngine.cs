using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.Vehicle.Car
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarEngine : NetworkBehaviour
    {
        private Rigidbody _vehicleRigidbody;
        public Rigidbody VehicleRigidbody => _vehicleRigidbody ??= GetComponent<Rigidbody>();

        public NetworkVariable<float> RPM = new NetworkVariable<float>();
        public NetworkVariable<bool> IsSliping = new NetworkVariable<bool>();
        
        [SerializeField]
        private Transform _centerOfMass;

        [Header("Settings")]
        [SerializeField]
        private float _minRPM;
        public float MinRPM => _minRPM;

        [SerializeField]
        private float _maxRPM;
        public float MaxRPM => _maxRPM;
        
        [SerializeField]
        private float _motorForce = 1500f;

        [Header("Wheel Colliders")]
        [SerializeField]
        private WheelCollider _frontLeft;

        [SerializeField]
        private WheelCollider _frontRight;

        [SerializeField]
        private WheelCollider _rearLeft;

        [SerializeField]
        private WheelCollider _rearRight;

        private float _throttle;

        private void Start()
        {
            VehicleRigidbody.centerOfMass = _centerOfMass.localPosition;
        }

        private void FixedUpdate()
        {
            float currentMotorForce = _throttle * _motorForce;
            float avgWheelRPM = (_frontLeft.rpm + _frontRight.rpm + _rearLeft.rpm + _rearRight.rpm) / 4f;
            RPM.Value = _minRPM + Mathf.Abs(avgWheelRPM) * 4.5f;

            if (RPM.Value < _minRPM)
                RPM.Value = _minRPM;

            if (RPM.Value > _maxRPM)
            {
                RPM.Value = _maxRPM + Random.Range(-50f, 50f);
            }
            else
            {
                ApplyMotor(currentMotorForce);
            }

            RPM.SetDirty(true);
        }

        private void ApplyMotor(float motor)
        {
            _rearLeft.motorTorque = motor;
            _rearRight.motorTorque = motor;
            _frontLeft.motorTorque = motor;
            _frontRight.motorTorque = motor;
        }

        public void SetThrottle(float throttle)
        {
            _throttle = Mathf.Clamp(throttle, -1f, 1f);
        }
    }
}