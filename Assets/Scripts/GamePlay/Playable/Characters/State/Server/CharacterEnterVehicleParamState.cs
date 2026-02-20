using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.State.Client;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;

namespace GamePlay.Playable.Characters.State.Server
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
                var data = new ServerSeatMiniGunParamState.SeatData()
                {
                    Vehicle = Data,
                    MiniGunSeat = miniGunSeat,
                    MiniGunController = miniGunSeat.Controller
                };
                Context
                    .SwitchStateWithData<ServerSeatMiniGunParamState,
                        ServerSeatMiniGunParamState.SeatData>(data);
            }
            else if (freeSeat != null)
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
            else
            {
                Context.SwitchState<CharacterBaseState>();
            }
        }

        public override void Exit()
        {
        }
    }
}