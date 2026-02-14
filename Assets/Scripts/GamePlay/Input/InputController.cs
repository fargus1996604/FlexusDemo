using System;
using GamePlay.Playable;
using GamePlay.Playable.Characters;
using GamePlay.Vehicle.Car;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace GamePlay.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField]
        private BaseCharacterController _playerController;

        private UserInputSystem _inputSystem;

        private void Awake()
        {
            _inputSystem = new UserInputSystem();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public PlayerInputHandler GetPlayerInputHandler()
        {
            return new PlayerInputHandler(_inputSystem);
        }

        public VehicleInputHandler GetVehicleInputHandler()
        {
            return new VehicleInputHandler(_inputSystem);
        }
    }

    public interface IInputHandler
    {
    }

    public class PlayerInputHandler : IInputHandler, UserInputSystem.IPlayerActions
    {
        public UnityEvent InteractPressed = new UnityEvent();

        private UserInputSystem _inputSystem;

        private Vector2 _move;
        public Vector2 Move => _move;

        private bool _isSprinting;
        public bool IsSprinting => _isSprinting;

        public PlayerInputHandler(UserInputSystem userInputSystem)
        {
            _inputSystem = userInputSystem;
            _inputSystem.Player.SetCallbacks(this);
            _inputSystem.Player.Interact.performed += delegate
            {
                InteractPressed?.Invoke();
            };
        }

        public void Enable()
        {
            _inputSystem.Player.Enable();
        }

        public void Disable()
        {
            _inputSystem.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _move = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValueAsButton();
        }
    }

    public class VehicleInputHandler : IInputHandler, UserInputSystem.IVehicleActions
    {
        public UnityEvent InteractPressed = new UnityEvent();
        public UnityEvent ChangeSeatPressed = new UnityEvent();

        private CarVehicleInputData _carVehicleInputData;
        public CarVehicleInputData CarVehicleInputData => _carVehicleInputData;

        private UserInputSystem _inputSystem;
        public UserInputSystem InputSystem => _inputSystem;

        public VehicleInputHandler(UserInputSystem userInputSystem)
        {
            _inputSystem = userInputSystem;
            _inputSystem.Vehicle.SetCallbacks(this);
            _inputSystem.Vehicle.Interact.performed += delegate
            {
                InteractPressed?.Invoke();
            };
            _inputSystem.Vehicle.ChangeSeat.performed += delegate
            {
                ChangeSeatPressed?.Invoke();
            };
            _carVehicleInputData = new CarVehicleInputData();
        }

        public void Enable()
        {
            _inputSystem.Vehicle.Enable();
        }

        public void Disable()
        {
            _inputSystem.Vehicle.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            var axes = context.ReadValue<Vector2>();
            _carVehicleInputData.Throttle = axes.y;
            _carVehicleInputData.Steering = axes.x;
        }

        public void OnLook(InputAction.CallbackContext context)
        {
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
        }

        public void OnBrake(InputAction.CallbackContext context)
        {
            _carVehicleInputData.IsBraking = context.ReadValueAsButton();
        }

        public void OnChangeSeat(InputAction.CallbackContext context)
        {
        }
    }
}