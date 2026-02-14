using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Playable.Characters;
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

        [SerializeField]
        private List<Seat> _seats;

        private void Update()
        {
            if (_driverSeat.InputData != null)
            {
                Controller.SetThrottle(_driverSeat.InputData.Throttle);
                Controller.SetSteering(_driverSeat.InputData.Steering);
                Controller.SetBrake(_driverSeat.InputData.IsBraking);
            }
            else
            {
                Controller.SetThrottle(0);
                Controller.SetSteering(0);
                Controller.SetBrake(false);
            }
        }

        public bool HasFreeSeat()
        {
            return GetFreeSeat() != null;
        }

        public Seat TryEnterCar(BaseCharacterController player)
        {
            foreach (var seat in _seats)
            {
                if (seat.TryAttach(player))
                {
                    return seat;
                }
            }

            return null;
        }

        public Seat TryChangeSeat(BaseCharacterController player)
        {
            var currentSeat = _seats.FirstOrDefault(s => s.Player == player);
            if (currentSeat == null)
                return null;

            var nextSeat = GetFreeSeat();
            if (nextSeat == null)
                return null;

            if (currentSeat.TransferTo(nextSeat) == false)
                return null;

            return nextSeat;
        }

        public void ExitCar(BaseCharacterController player)
        {
            foreach (var seat in _seats)
            {
                if (seat.Player == player)
                {
                    seat.Detach();
                    return;
                }
            }
        }

        private Seat GetFreeSeat()
        {
            foreach (var seat in _seats)
            {
                if (seat.HasFree)
                {
                    return seat;
                }
            }

            return null;
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