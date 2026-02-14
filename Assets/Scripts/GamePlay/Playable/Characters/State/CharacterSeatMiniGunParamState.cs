using Gameplay.Core.StateMachine;
using Gameplay.Core.StateMachine.Interfaces;
using GamePlay.Input.InputHandler;
using GamePlay.Playable.Characters.Animation;
using GamePlay.Vehicle.Car;
using GamePlay.Vehicle.Car.Seats;
using GamePlay.Vehicle.Car.Weapons;
using UnityEngine;

namespace GamePlay.Playable.Characters.State
{
    public class CharacterSeatMiniGunParamState : TickableParamBaseState<CharacterSeatMiniGunParamState.OutData>
    {
        public class OutData
        {
            public CarVehicle Vehicle;
            public MiniGunSeat MiniGunSeat;
            public MiniGunController MiniGunController;
        }

        private BaseCharacterController _baseCharacterController;
        private CharacterController _characterController;
        private CharacterAnimationController _characterAnimationController;
        private VehicleInputHandler _inputHandler;
        private CameraController _cameraController;

        private MiniGunController.InputData _inputData;
        
        public CharacterSeatMiniGunParamState(BaseCharacterController context, CharacterController characterController,
            CharacterAnimationController characterAnimationController, VehicleInputHandler inputHandler,
            CameraController cameraController) : base(context)
        {
            _baseCharacterController = context;
            _characterController = characterController;
            _characterAnimationController = characterAnimationController;
            _inputHandler = inputHandler;
            _cameraController = cameraController;
        }

        public override void Tick(float deltaTime)
        {
            _inputData.Fire = _inputHandler.FireEngaged;
            _inputData.LookDirection = Camera.main.transform.forward;
            Data.MiniGunController.Tick(deltaTime);
        }

        public override void Enter()
        {
            _inputData = new MiniGunController.InputData();
            _cameraController.ActivateMiniGunCamera(Data.MiniGunController.transform);
            _characterController.enabled = false;
            
            _characterAnimationController.SwitchToMiniGunLayer();
            _characterAnimationController.ResetBodyOrientation();
            _characterAnimationController.SetLeftHandIKTarget(Data.MiniGunController.LeftHandTarget);
            _characterAnimationController.SetRightHandIKTarget(Data.MiniGunController.RightHandTarget);
            
            _inputHandler.InteractPressed.AddListener(ExitVehicle);
            _inputHandler.ChangeSeatPressed.AddListener(ChangeSeat);
            _inputHandler.Enable();
            
            Data.MiniGunController.SetInputData(_inputData);
            Data.MiniGunController.ResetLookDirection();
        }

        public override void Exit()
        {
            _characterAnimationController.ResetAllIkTargets();
            _inputHandler.InteractPressed.RemoveListener(ExitVehicle);
            _inputHandler.ChangeSeatPressed.RemoveListener(ChangeSeat);
            _inputHandler.Disable();
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