using Shared.Packet;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerController : PlayerController
{
    private bool firePressed;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private Vector2 lastInput = Vector2.zero;
    private Vector3 targetPosition;
    private bool inputUpdated;

    protected override void Start()
    {
        InputAction moveAction = InputSystem.actions.FindAction("Move");
        moveAction.performed += OnMoveInput;
        moveAction.canceled += OnMoveInput;
        moveAction.Enable();

        InputAction fireAction = InputSystem.actions.FindAction("Jump");
        fireAction.performed += OnFireInput;
        fireAction.canceled += OnFireInput;
        fireAction.Enable();

        targetPosition = transform.position;
    }

    private void OnFireInput(InputAction.CallbackContext context)
    {
        firePressed = context.ReadValue<float>() > 0f;
        if (firePressed)
        {
            AnimationSetTrigger(ANIM_TRIGGER_SWORD_ATTACK);
        }
        AnimationSetBool(ANIM_BOOL_SWORD_ATTACKING, firePressed);
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (moveInput.normalized != lastInput)
        {
            inputUpdated = true;
            lastInput = moveInput;
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDir = moveInput;
        }
    }

    protected override void OnUpdate()
    {
        ApplyFacingDirection(lastMoveDir);

        if (firePressed == false)
        {
            AnimationSetFloat(ANIM_FLOAT_SPEED, moveInput.sqrMagnitude);

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                5f * Time.deltaTime
            );
        }
    }

    protected override void Tick()
    {
        if (firePressed == false)
        {
            targetPosition += (Vector3)moveInput * 5f * Time.fixedDeltaTime;
        }
    }

    public override void OnUpdateMoveState(S_Move move)
    {
    }
}
