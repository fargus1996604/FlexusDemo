using Gameplay.Core.StateMachine;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;

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
            if (_playerController.IsServer == false)
                return;
            
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
                var data = new CharacterSeatMiniGunParamState.SeatData()
                {
                    Vehicle = Data,
                    MiniGunSeat = miniGunSeat,
                    MiniGunController = miniGunSeat.Controller
                };
                Context
                    .SwitchStateWithData<CharacterSeatMiniGunParamState,
                        CharacterSeatMiniGunParamState.SeatData>(data);
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