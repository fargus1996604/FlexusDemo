using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Playable.Characters.State.Server;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using GamePlay.Vehicle.Car.Weapons;
using UnityEngine;

namespace GamePlay.Playable.Characters.State.Client
{
    public class ClientSeatMiniGunParamState : TickableParamBaseState<ServerSeatMiniGunParamState.SeatData>
    {
        private BaseCharacterController _baseCharacterController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private VehicleInputData _inputData;
        
        public ClientSeatMiniGunParamState(BaseCharacterController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, VehicleInputData inputData) : base(context)
        {
            _baseCharacterController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputData = inputData;
        }

        public override void Tick(float deltaTime)
        {
            Data.MiniGunController.InputData.Fire = _inputData.FireEngaged;
            Data.MiniGunController.InputData.LookDirection = _inputData.CameraForward;
        }

        public override void Enter()
        {
            _characterController.enabled = false;
            _characterAnimationController.SwitchToMiniGunLayer();
            _characterAnimationController.ResetBodyOrientation();
            _characterAnimationController.SetMiniGunIKTargetsRpc(Data.MiniGunController);
            
            _inputData.InteractPressed.AddListener(ExitVehicle);
            _inputData.ChangeSeatPressed.AddListener(ChangeSeat);
            
            Data.MiniGunController.ResetGun();
        }

        public override void Exit()
        {
            _characterAnimationController.ResetAllIkTargetsRpc();
            _inputData.InteractPressed.RemoveListener(ExitVehicle);
            _inputData.ChangeSeatPressed.RemoveListener(ChangeSeat);
            Data.MiniGunController.ResetGun();
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