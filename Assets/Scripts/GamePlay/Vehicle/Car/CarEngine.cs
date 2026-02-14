using System;
using GamePlay.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GamePlay.Vehicle.Car
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarEngine : MonoBehaviour 
    {
        private Rigidbody _vehicleRigidbody;
        protected Rigidbody VehicleRigidbody => _vehicleRigidbody ??= GetComponent<Rigidbody>();
        
        
        [SerializeField]
        private Transform _centerOfMass;

        [Header("Settings")]
        [SerializeField]
        private float _minRPM;
        
        [SerializeField]
        private float _maxRPM;

        [SerializeField]
        private float _rpm;
        
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
            _rpm = _minRPM + Mathf.Abs(avgWheelRPM) * 4.5f;

            if (_rpm < _minRPM) 
                _rpm = _minRPM;
    
            if (_rpm > _maxRPM) 
            {
                _rpm = _maxRPM + Random.Range(-50f, 50f); 
            }
            else
            {
                ApplyMotor(currentMotorForce);
            }
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