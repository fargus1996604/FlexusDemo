using Core.Singleton;
using GamePlay.Input.InputHandler;
using UnityEngine;

namespace GamePlay.Input
{
    public class InputController : Singleton<InputController>
    {
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
}