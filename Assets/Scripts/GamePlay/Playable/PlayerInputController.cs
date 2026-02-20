using System;
using GamePlay.Input;
using GamePlay.Playable.Characters.State;
using GamePlay.Vehicle.Car;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace GamePlay.Playable
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInputController : NetworkBehaviour
    {
        [System.Serializable]
        public enum ActionMapName
        {
            Player,
            Vehicle
        }

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
                    SendPlayerMoveRpc(context.ReadValue<Vector2>());
                };
                _userInputSystem.Player.Move.canceled += delegate { SendPlayerMoveRpc(Vector2.zero); };
                _userInputSystem.Player.Sprint.performed += delegate { SendSprintRpc(true); };
                _userInputSystem.Player.Sprint.canceled += delegate { SendSprintRpc(false); };
                _userInputSystem.Player.Interact.performed += delegate { InvokeInteractionServerRpc(); };

                _userInputSystem.Vehicle.Move.performed += delegate(InputAction.CallbackContext context)
                {
                    SendVehicleInputRpc(context.ReadValue<Vector2>());
                };
                _userInputSystem.Vehicle.Move.canceled += delegate { SendVehicleInputRpc(Vector2.zero); };
                _userInputSystem.Vehicle.Interact.performed += delegate { InvokeVehicleInteractionRpc(); };
                _userInputSystem.Vehicle.ChangeSeat.performed += delegate { InvokeVehicleChangeSeatRpc(); };
                _userInputSystem.Vehicle.Brake.performed += delegate { SendVehicleBrakeRpc(true); };
                _userInputSystem.Vehicle.Brake.canceled += delegate { SendVehicleBrakeRpc(false); };
                _userInputSystem.Vehicle.Fire.performed += delegate { SendVehicleFireRpc(true); };
                _userInputSystem.Vehicle.Fire.canceled += delegate { SendVehicleFireRpc(false); };
                _userInputSystem.Enable();


                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _camera = Camera.main;
            }

            if (IsServer)
            {
                Controller.OnStateBeginChange.AddListener(OnPlayerStateBeginChange);
            }

            OnPlayerStateBeginChange(typeof(CharacterBaseState));
        }

        private void Update()
        {
            if (IsOwner == false)
                return;

            UpdateCameraForwardRpc(_camera.transform.forward);
        }

        private void OnPlayerStateBeginChange(Type type, object _ = null)
        {
            if (type == typeof(CharacterBaseState))
            {
                ChangeInputStateRpc(ActionMapName.Player);
            }
            else if (type == typeof(CharacterDrivingVehicleParamState))
            {
                ChangeInputStateRpc(ActionMapName.Vehicle);
            }
        }

        [Rpc(SendTo.Owner)]
        private void ChangeInputStateRpc(ActionMapName action)
        {
            if (_userInputSystem == null)
                return;

            switch (action)
            {
                case ActionMapName.Player:
                    _userInputSystem.Player.Enable();
                    _userInputSystem.Vehicle.Disable();
                    break;
                case ActionMapName.Vehicle:
                    _userInputSystem.Player.Disable();
                    _userInputSystem.Vehicle.Enable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        #region PLAYER_INPUT

        private void SendPlayerMoveRpc(Vector2 move)
        {
            _playerInputDataState.Axes = move;
            Controller.PlayerInput.UpdateState(_playerInputDataState);
        }

        private void SendSprintRpc(bool isSprinting)
        {
            _playerInputDataState.IsSprinting = isSprinting;
            Controller.PlayerInput.UpdateState(_playerInputDataState);
        }

        private void UpdateCameraForwardRpc(Vector3 forward)
        {
            if(IsOwner == false)
                return;
            
            _playerInputDataState.MoveDirection = forward;
            Controller.PlayerInput.UpdateState(_playerInputDataState);

            Controller.VehicleInput.CameraForward = forward;
            Controller.VehicleInput.SetDirty(true);
        }

        [ServerRpc]
        private void InvokeInteractionServerRpc()
        {
            Controller.PlayerInput.InteractPressed?.Invoke();
        }

        #endregion

        #region VEHICLE_INPUT

        [Rpc(SendTo.Server)]
        private void SendVehicleInputRpc(Vector2 axis)
        {
            Controller.VehicleInput.CarVehicleInputData.Throttle = axis.y;
            Controller.VehicleInput.CarVehicleInputData.Steering = axis.x;
            Controller.VehicleInput.SetDirty(true);
        }

        [Rpc(SendTo.Server)]
        private void SendVehicleBrakeRpc(bool isBraking)
        {
            Controller.VehicleInput.CarVehicleInputData.IsBraking = isBraking;
            Controller.VehicleInput.SetDirty(true);
        }

        [Rpc(SendTo.Server)]
        private void InvokeVehicleInteractionRpc()
        {
            Controller.VehicleInput.InteractPressed?.Invoke();
        }

        [Rpc(SendTo.Server)]
        private void InvokeVehicleChangeSeatRpc()
        {
            Controller.VehicleInput.ChangeSeatPressed?.Invoke();
        }

        [Rpc(SendTo.Server)]
        private void SendVehicleFireRpc(bool isFireing)
        {
            Controller.VehicleInput.FireEngaged = isFireing;
            Controller.VehicleInput.SetDirty(true);
        }

        #endregion
    }

    [System.Serializable]
    public class PlayerInputData
    {
        public readonly UnityEvent InteractPressed = new UnityEvent();

        [System.Serializable]
        public struct State : INetworkSerializable
        {
            public int Tick;
            public Vector2 Axes;
            public Vector3 MoveDirection;
            public bool IsSprinting;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Tick);
                serializer.SerializeValue(ref Axes);
                serializer.SerializeValue(ref MoveDirection);
                serializer.SerializeValue(ref IsSprinting);
            }

            public override string ToString()
            {
                return $"{Tick}, {Axes}, {MoveDirection}, {IsSprinting}";
            }
        }

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

    [System.Serializable]
    public class VehicleInputData : NetworkVariableBase
    {
        public readonly UnityEvent InteractPressed = new();
        public readonly UnityEvent ChangeSeatPressed = new();
        public CarVehicle.InputData CarVehicleInputData = new();
        public bool FireEngaged;
        public Vector3 CameraForward;

        public override void WriteField(FastBufferWriter writer)
        {
            writer.WriteValueSafe(CarVehicleInputData.Throttle);
            writer.WriteValueSafe(CarVehicleInputData.Steering);
            writer.WriteValueSafe(CarVehicleInputData.IsBraking);
            writer.WriteValueSafe(FireEngaged);
            writer.WriteValueSafe(CameraForward);
        }

        public override void ReadField(FastBufferReader reader)
        {
            reader.ReadValueSafe(out CarVehicleInputData.Throttle);
            reader.ReadValueSafe(out CarVehicleInputData.Steering);
            reader.ReadValueSafe(out CarVehicleInputData.IsBraking);
            reader.ReadValueSafe(out FireEngaged);
            reader.ReadValueSafe(out CameraForward);
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
        }
    }
}