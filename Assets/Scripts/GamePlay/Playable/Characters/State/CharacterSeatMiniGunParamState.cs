using Gameplay.Core.StateMachine;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using GamePlay.Vehicle.Car.Weapons;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterSeatMiniGunParamState : TickableParamBaseState<CharacterSeatMiniGunParamState.SeatData>
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

        private PlayerController _playerController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private NetworkAnimator _networkAnimator;
        private VehicleInputData _inputData;

        public CharacterSeatMiniGunParamState(PlayerController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, NetworkAnimator networkAnimator,
            VehicleInputData inputData) : base(context)
        {
            _playerController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _networkAnimator = networkAnimator;
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

            if (_playerController.IsServer)
            {
                _characterAnimationController.SwitchToMiniGunLayer();
                _characterAnimationController.ResetBodyOrientation();
                _characterAnimationController.SetMiniGunIKTargetsRpc(Data.MiniGunController);
                _inputData.InteractPressed.AddListener(ExitVehicle);
                _inputData.ChangeSeatPressed.AddListener(ChangeSeat);
                Data.MiniGunController.Activate();
            }
            else
            {
                _playerController.transform.localPosition = Data.MiniGunSeat.Pivot.localPosition;
                _playerController.transform.localRotation = Data.MiniGunSeat.Pivot.localRotation;
                _networkAnimator.enabled = true;
            }
        }

        public override void Exit()
        {
            _characterAnimationController.ResetAllIkTargetsRpc();
            _inputData.InteractPressed.RemoveListener(ExitVehicle);
            _inputData.ChangeSeatPressed.RemoveListener(ChangeSeat);
            _networkAnimator.enabled = false;
            if (_playerController.IsServer)
            {
                Data.MiniGunController.Deactivate();
            }
        }

        private void ChangeSeat()
        {
            var seat = Data.Vehicle.TryChangeSeat(_playerController);
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