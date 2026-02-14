using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterChangeSeatParamState : ParamBaseState<CharacterChangeSeatParamState.VehicleData>
    {
        public struct VehicleData
        {
            public CarVehicle Vehicle;
            public Seat Seat;
        }

        public CharacterChangeSeatParamState(IStateContext context) : base(context)
        {
        }

        public override void Enter()
        {
            if (Data.Seat is DriverSeat driverSeat)
            {
                var data = new CharacterDrivingVehicleParamState.VehicleData
                {
                    Vehicle = Data.Vehicle,
                    DriverSeat = driverSeat
                };
                Context
                    .SwitchStateWithData<CharacterDrivingVehicleParamState,
                        CharacterDrivingVehicleParamState.VehicleData>(data);
            }
            else
            {
                var data = new CharacterSeatParamState.VehicleData
                {
                    Vehicle = Data.Vehicle,
                    Seat = Data.Seat
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