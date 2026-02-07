using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using Shared.Packet;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    #region 애니메이션 파라미터 (hash)
    private readonly int ANIM_FLOAT_MOVE_X = Animator.StringToHash("MoveX");
    private readonly int ANIM_FLOAT_MOVE_Y = Animator.StringToHash("MoveY");
    private readonly int ANIM_FLOAT_SPEED = Animator.StringToHash("Speed");
    private readonly int ANIM_BOOL_SWORD_ATTACKING = Animator.StringToHash("SwordAttacking");
    private readonly int ANIM_TRIGGER_SWORD_ATTACK = Animator.StringToHash("SwordAttack");
    #endregion

    #region SerializeField
    [Header("Component")]
    [SerializeField] private List<Animator> animators;
    [SerializeField] private List<SpriteRenderer> spriteRenderers;

    [Header("Settings")]
    [SerializeField] private Vector2 collisionOffset;
    [SerializeField] private Vector2 collisionSize;
    [SerializeField] private float searchRange;
    #endregion
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private bool firePressed;
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    public ObjectInfo Info { get; set; } = new ObjectInfo();
    public int Id
    {
        get => Info.ObjectId;
        set => Info.ObjectId = value;
    }

    private void Awake()
    {
        Application.targetFrameRate = -1;
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
            AnimationSetFloat(ANIM_FLOAT_SPEED, input.sqrMagnitude);
            transform.position += (Vector3)input * 5f * Time.deltaTime;
        }

        UpdateOccupiedCells();
    }

    private void UpdateOccupiedCells()
    {
        Managers.Collision.UpdateOccupiedCells(gameObject, occupiedCells, collisionOffset, collisionSize);
    }

    private void ApplyFacingDirection(Vector2 dir)
    {
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        if (absX >= absY)
        {
            AnimationSetFloat(ANIM_FLOAT_MOVE_X, 1f);
            AnimationSetFloat(ANIM_FLOAT_MOVE_Y, 0f);
        }
        else
        {
            AnimationSetFloat(ANIM_FLOAT_MOVE_X, 0f);
            AnimationSetFloat(ANIM_FLOAT_MOVE_Y, Mathf.Sign(dir.y));
        }

        if (dir.x != 0)
        {
            SetFlip(dir.x > 0f);
        }
        else
        {
            SetFlip(false);
        }
    }

    #region Animation Parameter
    private void AnimationSetTrigger(int id)
    {
        animators.ForEach(anim => anim.SetTrigger(id));
    }

    private void AnimationSetFloat(int id, float value)
    {
        animators.ForEach(anim => anim.SetFloat(id, value));
    }

    private void AnimationSetBool(int id, bool value)
    {
        animators.ForEach(anim => anim.SetBool(id, value));
    }
    #endregion

    #region SpriteRenderer
    private void SetFlip(bool right)
    {
        spriteRenderers.ForEach(sp => sp.flipX = right);
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube((Vector3)collisionOffset + transform.position, new Vector3(collisionSize.x, collisionSize.y));
        Gizmos.DrawWireSphere((Vector3)collisionOffset + transform.position, searchRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.black;
            List<GameObject> nearObjects = Managers.Collision.GetObjectsInRange(transform.position, collisionOffset, searchRange);
            foreach (GameObject obj in nearObjects)
            {
                if (obj == gameObject)
                {
                    continue;
                }
                Gizmos.DrawWireSphere(obj.transform.position, 0.5f);
            }
        }
    }
}
