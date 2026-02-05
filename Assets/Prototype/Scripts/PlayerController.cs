using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    private readonly int ANIM_PARAM_MOVE_X = Animator.StringToHash("MoveX");
    private readonly int ANIM_PARAM_MOVE_Y = Animator.StringToHash("MoveY");
    private readonly int ANIM_PARAM_SPEED = Animator.StringToHash("Speed");
    private readonly int ANIM_PARAM_SWORD_ATTACKING = Animator.StringToHash("SwordAttacking");
    private readonly int ANIM_PARAM_SWORD_ATTACK = Animator.StringToHash("SwordAttack");
    [SerializeField] private List<Animator> animators;
    [SerializeField] private List<SpriteRenderer> spriteRenderers;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private bool firePressed;

    private void Awake()
    {
    }

    private void Start()
    {
        InputAction moveAction = InputSystem.actions.FindAction("Move");
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        moveAction.Enable();

        InputAction fireAction = InputSystem.actions.FindAction("Jump");
        fireAction.performed += OnFire;
        fireAction.canceled += OnFire;
        fireAction.Enable();
        Debug.Log(moveAction.id);
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        firePressed = context.ReadValue<float>() > 0f;
        if (firePressed)
        {
            animators.ForEach(anim => anim.SetTrigger(ANIM_PARAM_SWORD_ATTACK));
        }
        animators.ForEach(anim => anim.SetBool(ANIM_PARAM_SWORD_ATTACKING, firePressed));
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        Vector2 input = moveInput.normalized;

        if (input.sqrMagnitude > 0.01f)
        {
            lastMoveDir = input;
        }
        ApplyFacingDirection(lastMoveDir);

        if (firePressed == false)
        {
            animators.ForEach(anim => anim.SetFloat(ANIM_PARAM_SPEED, input.sqrMagnitude));
            transform.position += (Vector3)input * 5f * Time.deltaTime;
        }
    }

    private void ApplyFacingDirection(Vector2 dir)
    {
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        if (absX >= absY)
        {
            animators.ForEach(anim => anim.SetFloat(ANIM_PARAM_MOVE_X, 1f));
            animators.ForEach(anim => anim.SetFloat(ANIM_PARAM_MOVE_Y, 0f));
        }
        else
        {
            animators.ForEach(anim => anim.SetFloat(ANIM_PARAM_MOVE_X, 0f));
            animators.ForEach(anim => anim.SetFloat(ANIM_PARAM_MOVE_Y, Mathf.Sign(dir.y)));
        }

        if (dir.x != 0)
        {
            spriteRenderers.ForEach(sp => sp.flipX = dir.x > 0f);
        }
        else
        {
            spriteRenderers.ForEach(sp => sp.flipX = false);
        }
    }
}
