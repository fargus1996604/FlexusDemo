using System;
using GamePlay.Playable;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Input
{
    public class InputController : MonoBehaviour , UserInputSystem.IPlayerActions
    {
        [SerializeField]
        private PlayerController _playerController;
        
        private IInputHandler _handler;
        private UserInputSystem _inputSystem;

        
        private void Start()
        {
            _inputSystem = new UserInputSystem();
            _inputSystem.Player.SetCallbacks(this);
            _inputSystem.Player.Enable();
            _handler = _playerController;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _handler.OnMove(context.ReadValue<Vector2>()); 
        }

        public void OnLook(InputAction.CallbackContext context)
        {
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _handler.OnSprint(context.ReadValueAsButton());
        }
    }

    public interface IInputHandler
    {
        void OnMove(Vector2 axes);
        void OnSprint(bool engaged);
    }
}
