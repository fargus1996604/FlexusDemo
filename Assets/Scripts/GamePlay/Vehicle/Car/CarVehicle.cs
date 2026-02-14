using System;
using System.Collections.Generic;
using GamePlay.Input;
using GamePlay.Playable;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Vehicle.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarVehicle : MonoBehaviour
    {
        private CarController _controller;
        protected CarController Controller => _controller ??= GetComponent<CarController>();
        
        [SerializeField]
        private DriverSeat _driverSeat;
        
        private void Update()
        {
            if (_driverSeat.InputData == null)
                return;
            
            Controller.SetThrottle(_driverSeat.InputData.Throttle);
            Controller.SetSteering(_driverSeat.InputData.Steering);
            Controller.SetBrake(_driverSeat.InputData.IsBraking);
        }

        public bool HasFreeSeat()
        {
            return _driverSeat.HasFree;
        }
        
        public DriverSeat TryEnterCar(PlayerController player)
        {
            if (_driverSeat.TryAttach(player))
            {
                return _driverSeat;
            }

            return null;
        }

        public void ExitCar(PlayerController player)
        {
            _driverSeat.Detach();
        }
    }
    
    [Serializable]
    public class CarVehicleInputData
    {
        public float Throttle;
        public float Steering;
        public bool IsBraking;
    }
}