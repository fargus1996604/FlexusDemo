using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterEnterVehicleParamState : ParamBaseState<CarVehicle>
    {
        private PlayerController _playerController;
        
        public CharacterEnterVehicleParamState(PlayerController context) : base(context)
        {
            _playerController = context;
        }

        public override void Enter()
        {
            var seatDriver = Data.TryEnterCar(_playerController);
            if (seatDriver is DriverSeat driverSeat)
            {
                var data = new CharacterDrivingVehicleParamState.VehicleData
                {
                    Vehicle = Data,
                    DriverSeat = driverSeat
                };
                Context.SwitchStateWithData<CharacterDrivingVehicleParamState,CharacterDrivingVehicleParamState.VehicleData>(data);
            }
        }

        public override void Exit()
        {
            
        }
    }
}
