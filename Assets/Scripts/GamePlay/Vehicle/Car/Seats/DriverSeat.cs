using GamePlay.Input;
using UnityEngine;

namespace GamePlay.Vehicle.Car.Seats
{
    public class DriverSeat : Seat
    {
        private CarVehicle.InputData _inputData;
        public CarVehicle.InputData InputData => _inputData;

        public void SetInputData(CarVehicle.InputData inputData)
        {
            _inputData = inputData;
        }
    }
}
