using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace GamePlay.Input.InputHandler
{
    public class PlayerInputHandler 
    {
        public readonly UnityEvent InteractPressed = new UnityEvent();

        private Vector2 _move;
        public Vector2 Move => _move;

        private bool _isSprinting;
        public bool IsSprinting => _isSprinting;
        
        private UserInputSystem _inputSystem;
        
        public PlayerInputHandler(UserInputSystem userInputSystem)
        {
            _inputSystem = userInputSystem;
            _inputSystem.Player.Move.performed += delegate(InputAction.CallbackContext context)
            {
                _move = context.ReadValue<Vector2>();
            };
            
            _inputSystem.Player.Move.canceled += delegate
            {
                _move = Vector2.zero;
            };
            
            _inputSystem.Player.Interact.performed += delegate { InteractPressed?.Invoke(); };
            _inputSystem.Player.Sprint.performed += delegate { _isSprinting = true; };
            _inputSystem.Player.Sprint.canceled += delegate { _isSprinting = false; };
        }

        public void Enable()
        {
            _inputSystem.Player.Enable();
        }
        
        public void Disable()
        {
            _inputSystem.Player.Disable();
        }
    }
}