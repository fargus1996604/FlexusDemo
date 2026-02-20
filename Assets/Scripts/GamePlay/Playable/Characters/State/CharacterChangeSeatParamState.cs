using Gameplay.Core.StateMachine;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using Unity.Netcode;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterChangeSeatParamState : ParamBaseState<CharacterChangeSeatParamState.VehicleData>
    {
        public struct VehicleData : IStateNetworkData
        {
            public CarVehicle Vehicle;
            public Seat Seat;

            public void Boxing(NetworkBehaviourReference[] references)
            {
                references[0].TryGet(out Vehicle);
                references[1].TryGet(out Seat);
            }

            public NetworkBehaviourReference[] Unboxing()
            {
                return new NetworkBehaviourReference[]
                {
                    Vehicle,
                    Seat
                };
            }

            public bool IsValid()
            {
                return Vehicle != null && Seat != null;
            }
        }

        private PlayerController _playerController;
        
        public CharacterChangeSeatParamState(PlayerController context) : base(context)
        {
            _playerController = context;
        }

        public override void Enter()
        {
            if(_playerController.IsServer == false)
                return;
            
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
            else if (Data.Seat is MiniGunSeat miniGunSeat)
            {
                var data = new CharacterSeatMiniGunParamState.SeatData()
                {
                    Vehicle = Data.Vehicle,
                    MiniGunSeat = miniGunSeat,
                    MiniGunController = miniGunSeat.Controller
                };
                Context
                    .SwitchStateWithData<CharacterSeatMiniGunParamState,
                        CharacterSeatMiniGunParamState.SeatData>(data);
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