using GamePlay.Vehicle.Car;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace GamePlay.Input.InputHandler
{
    public class VehicleInputHandler
    {
        public readonly UnityEvent InteractPressed = new UnityEvent();
        public readonly UnityEvent ChangeSeatPressed = new UnityEvent();

        private CarVehicleInputData _carVehicleInputData;
        public CarVehicleInputData CarVehicleInputData => _carVehicleInputData;

        private bool _fireEngaged;
        public bool FireEngaged => _fireEngaged;

        private UserInputSystem _inputSystem;

        public VehicleInputHandler(UserInputSystem userInputSystem)
        {
            _carVehicleInputData = new CarVehicleInputData();
            _inputSystem = userInputSystem;
            _inputSystem.Vehicle.Move.performed += delegate(InputAction.CallbackContext context)
            {
                var axes = context.ReadValue<Vector2>();
                _carVehicleInputData.Throttle = axes.y;
                _carVehicleInputData.Steering = axes.x;
            };

            _inputSystem.Vehicle.Move.canceled += delegate
            {
                _carVehicleInputData.Throttle = 0;
                _carVehicleInputData.Steering = 0;
            };

            _inputSystem.Vehicle.Interact.performed += delegate { InteractPressed?.Invoke(); };
            _inputSystem.Vehicle.ChangeSeat.performed += delegate { ChangeSeatPressed?.Invoke(); };

            _inputSystem.Vehicle.Brake.performed += delegate { _carVehicleInputData.IsBraking = true; };
            _inputSystem.Vehicle.Brake.canceled += delegate { _carVehicleInputData.IsBraking = false; };

            _inputSystem.Vehicle.Fire.performed += delegate { _fireEngaged = true; };
            _inputSystem.Vehicle.Fire.canceled += delegate { _fireEngaged = false; };
        }

        public void Enable()
        {
            _inputSystem.Vehicle.Enable();
        }

        public void Disable()
        {
            _inputSystem.Vehicle.Disable();
        }
    }
}