using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Playable.Characters.State;
using GamePlay.Playable.Characters.State.StateParam;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Playable.Characters.Extensions
{
    public static class StateExtensions
    {
        public static void SwitchSeatState(this Seat seat, IStateContext context, CarVehicle vehicle)
        {
            if (seat is DriverSeat driverSeat)
            {
                var data = new VehicleSeatData
                {
                    Vehicle = vehicle,
                    DriverSeat = driverSeat
                };
                context
                    .SwitchStateWithData<CharacterDrivingVehicleParamState,
                        VehicleSeatData>(data);
            }
            else if (seat is MiniGunSeat miniGunSeat)
            {
                var data = new MiniGunSeatData()
                {
                    Vehicle = vehicle,
                    MiniGunSeat = miniGunSeat,
                    MiniGunController = miniGunSeat.Controller
                };
                context
                    .SwitchStateWithData<CharacterSeatMiniGunParamState,
                        MiniGunSeatData>(data);
            }
            else if (seat != null)
            {
                var data = new SeatData
                {
                    Vehicle = vehicle,
                    Seat = seat
                };
                context
                    .SwitchStateWithData<CharacterSeatParamState,
                        SeatData>(data);
            }
            else
            {
                context.SwitchState<CharacterBaseState>();
            }
        }
    }
}