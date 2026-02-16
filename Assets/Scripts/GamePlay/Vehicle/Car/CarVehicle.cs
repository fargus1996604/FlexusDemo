using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Playable.Characters;
using GamePlay.Vehicle.Car.Seats;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay.Vehicle.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarVehicle : NetworkBehaviour
    {
        [Serializable]
        public class InputData
        {
            public float Throttle;
            public float Steering;
            public bool IsBraking;
        }
        
        private CarController _controller;
        protected CarController Controller => _controller ??= GetComponent<CarController>();

        [SerializeField]
        private DriverSeat _driverSeat;

        [SerializeField]
        private List<Seat> _seats;

        public override void OnNetworkSpawn()
        {
            if (HasAuthority == false)
            {
                Controller.Engine.VehicleRigidbody.isKinematic = true;
                Controller.Engine.enabled = false;
                Controller.enabled = false;
            }
        }

        private void Update()
        {
            if(HasAuthority == false)
                return;
            
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

            var nextSeat = GetNextFreeSeat(currentSeat);
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

        private Seat GetNextFreeSeat(Seat currentSeat)
        {
            int total = _seats.Count;
            int startIdx = _seats.IndexOf(currentSeat) + 1;

            for (int i = 0; i < total; i++)
            {
                int nextIdx = (startIdx + i) % total;
                if (_seats[nextIdx].HasFree)
                {
                    return _seats[nextIdx];
                }
            }
            return null;
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
}