using GamePlay.Input;
using UnityEngine;

namespace GamePlay.Vehicle.Car.Seats
{
    public class DriverSeat : Seat
    {
        private CarVehicleInputData _inputData;
        public CarVehicleInputData InputData => _inputData;

        public void SetInputData(CarVehicleInputData inputData)
        {
            _inputData = inputData;
        }
    }
}
