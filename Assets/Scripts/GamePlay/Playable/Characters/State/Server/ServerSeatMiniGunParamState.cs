using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using GamePlay.Vehicle.Car.Weapons;
using Unity.Netcode;
using UnityEngine;

namespace GamePlay.Playable.Characters.State.Server
{
    public class ServerSeatMiniGunParamState : ParamBaseState<ServerSeatMiniGunParamState.SeatData>
    {
        public struct SeatData : IStateNetworkData
        {
            public CarVehicle Vehicle;
            public MiniGunSeat MiniGunSeat;
            public MiniGunController MiniGunController;
            
            public void Boxing(NetworkBehaviourReference[] references)
            {
                references[0].TryGet(out Vehicle);
                references[1].TryGet(out MiniGunSeat);
                references[2].TryGet(out MiniGunController);
            }

            public NetworkBehaviourReference[] Unboxing()
            {
                return new NetworkBehaviourReference[]
                {
                    Vehicle,
                    MiniGunSeat,
                    MiniGunController
                };
            }

            public bool IsValid()
            {
                return Vehicle != null && MiniGunSeat != null && MiniGunController != null;
            }
        }

        private BaseCharacterController _baseCharacterController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private VehicleInputData _inputData;

        public ServerSeatMiniGunParamState(BaseCharacterController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, VehicleInputData inputData) : base(context)
        {
            _baseCharacterController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputData = inputData;
        }

        public override void Enter()
        {
            _characterController.enabled = false;
            _characterAnimationController.SwitchToMiniGunLayer();
            _characterAnimationController.ResetBodyOrientation();
            _characterAnimationController.SetMiniGunIKTargetsRpc(Data.MiniGunController);
            _inputData.InteractPressed.AddListener(ExitVehicle);
            _inputData.ChangeSeatPressed.AddListener(ChangeSeat);
            Data.MiniGunController.Activate();
        }

        public override void Exit()
        {
            _characterAnimationController.ResetAllIkTargetsRpc();
            _inputData.InteractPressed.RemoveListener(ExitVehicle);
            _inputData.ChangeSeatPressed.RemoveListener(ChangeSeat);
            Data.MiniGunController.Deactivate();
        }

        private void ChangeSeat()
        {
            var seat = Data.Vehicle.TryChangeSeat(_baseCharacterController);
            var data = new CharacterChangeSeatParamState.VehicleData()
            {
                Vehicle = Data.Vehicle,
                Seat = seat
            };

            Context.SwitchStateWithData<CharacterChangeSeatParamState, CharacterChangeSeatParamState.VehicleData>(data);
        }

        private void ExitVehicle()
        {
            Context.SwitchStateWithData<CharacterExitVehicleParamState, CarVehicle>(Data.Vehicle);
        }
    }
}