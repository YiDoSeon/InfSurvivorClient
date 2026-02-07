using Shared.Packet;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerController : PlayerController
{
    private bool firePressed;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;

    protected override void Start()
    {
        InputAction moveAction = InputSystem.actions.FindAction("Move");
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        moveAction.Enable();

        InputAction fireAction = InputSystem.actions.FindAction("Jump");
        fireAction.performed += OnFire;
        fireAction.canceled += OnFire;
        fireAction.Enable();
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        firePressed = context.ReadValue<float>() > 0f;
        if (firePressed)
        {
            AnimationSetTrigger(ANIM_TRIGGER_SWORD_ATTACK);
        }
        AnimationSetBool(ANIM_BOOL_SWORD_ATTACKING, firePressed);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    protected override void OnUpdate()
    {
        Vector2 input = moveInput.normalized;

        if (input.sqrMagnitude > 0.01f)
        {
            lastMoveDir = input;
        }
        ApplyFacingDirection(lastMoveDir);

        if (firePressed == false)
        {
            AnimationSetFloat(ANIM_FLOAT_SPEED, input.sqrMagnitude);
            transform.position += (Vector3)input * 5f * Time.deltaTime;
        }
    }

    protected override void Tick()
    {
    }

    public override void OnUpdateMoveState(S_Move move)
    {
    }
}
