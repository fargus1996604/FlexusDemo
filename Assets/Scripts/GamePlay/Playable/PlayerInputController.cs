using System;
using GamePlay.Input;
using GamePlay.Playable.Characters.State;
using GamePlay.Vehicle.Car;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GamePlay.Playable
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInputController : NetworkBehaviour
    {
        private PlayerController _controller;
        protected PlayerController Controller => _controller ??= GetComponent<PlayerController>();

        private UserInputSystem _userInputSystem;
        private Camera _camera;

        private PlayerInputData.State _playerInputDataState;

        private void Start()
        {
            if (IsOwner)
            {
                _userInputSystem = new UserInputSystem();
                _userInputSystem.Player.Move.performed += delegate(InputAction.CallbackContext context)
                {
                    UpdatePlayerMove(context.ReadValue<Vector2>());
                };
                _userInputSystem.Player.Move.canceled += delegate { UpdatePlayerMove(Vector2.zero); };
                _userInputSystem.Player.Sprint.performed += delegate { UpdateIsSprinting(true); };
                _userInputSystem.Player.Sprint.canceled += delegate { UpdateIsSprinting(false); };
                _userInputSystem.Player.Interact.performed += delegate { InvokeInteractionServerRpc(); };
                _userInputSystem.Player.ChangeSeat.performed += delegate { InvokeChangeSeatServerRpc(); };
                _userInputSystem.Player.Brake.performed += delegate { SetVehicleIsBraking(true); };
                _userInputSystem.Player.Brake.canceled += delegate { SetVehicleIsBraking(false); };
                _userInputSystem.Player.Fire.performed += delegate
                {
                    InvokeFireTriggeredServerRpc();
                    UpdateIsFire(true);
                };
                _userInputSystem.Player.Fire.canceled += delegate
                {
                    UpdateIsFire(false);
                };
                _userInputSystem.Enable();

                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            if (IsOwner == false)
                return;

            UpdateCameraForwardRpc(_camera.transform.forward);
        }

        #region PLAYER_INPUT

        private void UpdatePlayerMove(Vector2 move)
        {
            _playerInputDataState.Axes = move;
            _playerInputDataState.VehicleInputData.Throttle = move.y;
            _playerInputDataState.VehicleInputData.Steering = move.x;
            Controller.PlayerInput.UpdateState(_playerInputDataState);
        }

        private void UpdateIsSprinting(bool isSprinting)
        {
            _playerInputDataState.IsSprinting = isSprinting;
            Controller.PlayerInput.UpdateState(_playerInputDataState);
        }
        
        private void UpdateCameraForwardRpc(Vector3 forward)
        {
            if (IsOwner == false)
                return;

            _playerInputDataState.LookDirection = forward;
            Controller.PlayerInput.UpdateState(_playerInputDataState);
        }
        
        private void SetVehicleIsBraking(bool isBraking)
        {
            _playerInputDataState.VehicleInputData.IsBraking = isBraking;
            Controller.PlayerInput.UpdateState(_playerInputDataState);
        }
        
        private void UpdateIsFire(bool isFire)
        {
            _playerInputDataState.FireEngaged = isFire;
            Controller.PlayerInput.UpdateState(_playerInputDataState);
        }

        [ServerRpc]
        private void InvokeInteractionServerRpc()
        {
            Controller.PlayerInput.InteractPressed?.Invoke();
        }

        [ServerRpc]
        private void InvokeChangeSeatServerRpc()
        {
            Controller.PlayerInput.ChangeSeatPressed?.Invoke();
        }
        
        [ServerRpc]
        private void InvokeFireTriggeredServerRpc()
        {
            Controller.PlayerInput.FireTriggered?.Invoke();
        }
        
        #endregion
    }

    [System.Serializable]
    public class PlayerInputData
    {
        [System.Serializable]
        public struct State : INetworkSerializable
        {
            public int Tick;
            public Vector2 Axes;
            public Vector3 LookDirection;
            public bool IsSprinting;
            public bool FireEngaged;
            public CarVehicle.InputData VehicleInputData;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Tick);
                serializer.SerializeValue(ref Axes);
                serializer.SerializeValue(ref LookDirection);
                serializer.SerializeValue(ref IsSprinting);
                serializer.SerializeValue(ref FireEngaged);
                serializer.SerializeValue(ref VehicleInputData.Throttle);
                serializer.SerializeValue(ref VehicleInputData.Steering);
                serializer.SerializeValue(ref VehicleInputData.IsBraking);
            }

            public override string ToString()
            {
                return $"{Tick}, {Axes}, {LookDirection}, {IsSprinting}, {FireEngaged}";
            }
        }

        public readonly UnityEvent InteractPressed = new();
        public readonly UnityEvent ChangeSeatPressed = new();
        public readonly UnityEvent FireTriggered = new();

        [SerializeField]
        private State _state;

        public void UpdateState(State newState)
        {
            _state = newState;
        }

        public State GetState()
        {
            return _state;
        }
    }
}