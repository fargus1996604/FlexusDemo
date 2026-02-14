using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterEnterVehicleParamState : ParamBaseState<CarVehicle>
    {
        private BaseCharacterController _playerController;

        public CharacterEnterVehicleParamState(BaseCharacterController context) : base(context)
        {
            _playerController = context;
        }

        public override void Enter()
        {
            var freeSeat = Data.TryEnterCar(_playerController);
            if (freeSeat is DriverSeat driverSeat)
            {
                var data = new CharacterDrivingVehicleParamState.VehicleData
                {
                    Vehicle = Data,
                    DriverSeat = driverSeat
                };
                Context
                    .SwitchStateWithData<CharacterDrivingVehicleParamState,
                        CharacterDrivingVehicleParamState.VehicleData>(data);
            }
            else if (freeSeat is MiniGunSeat miniGunSeat)
            {
                var data = new CharacterSeatMiniGunParamState.OutData()
                {
                    Vehicle = Data,
                    MiniGunSeat = miniGunSeat,
                    MiniGunController = miniGunSeat.Controller
                };
                Context
                    .SwitchStateWithData<CharacterSeatMiniGunParamState,
                        CharacterSeatMiniGunParamState.OutData>(data);
            }
            else
            {
                var data = new CharacterSeatParamState.VehicleData
                {
                    Vehicle = Data,
                    Seat = freeSeat
                };
                Context
                    .SwitchStateWithData<CharacterSeatParamState,
                        CharacterSeatParamState.VehicleData>(data);
            }
        }

        public override void Exit()
        {
        }
    }
}