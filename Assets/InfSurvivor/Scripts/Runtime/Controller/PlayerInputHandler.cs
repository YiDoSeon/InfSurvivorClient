using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InfSurvivor.Runtime.Controller
{
    [DisallowMultipleComponent]
    public class PlayerInputHandler : MonoBehaviour
    {
        public InputAction MoveAction { get; private set; }
        public InputAction FireAction { get; private set; }
        public bool FirePressed { get; private set; }
        public Vector2 MoveInput { get; private set; }
    
        private void Awake()
        {
            MoveAction = InputSystem.actions.FindAction("Move");
            FireAction = InputSystem.actions.FindAction("Jump");
        }
    
        private void OnEnable()
        {
            MoveAction.performed += OnMoveInput;
            MoveAction.canceled += OnMoveInput;
            MoveAction.Enable();
    
            FireAction.performed += OnFireInput;
            FireAction.canceled += OnFireInput;
            FireAction.Enable();
        }
    
        private void OnDisable()
        {
            MoveAction.performed -= OnMoveInput;
            MoveAction.canceled -= OnMoveInput;
            MoveAction.Disable();
    
            FireAction.performed -= OnFireInput;
            FireAction.canceled -= OnFireInput;
            FireAction.Disable();
        }
    
        private void OnFireInput(InputAction.CallbackContext context)
        {
            FirePressed = context.ReadValue<float>() > 0f;
        }
    
        private void OnMoveInput(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }
    }    
}