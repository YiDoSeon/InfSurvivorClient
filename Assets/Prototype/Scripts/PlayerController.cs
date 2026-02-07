using System.Collections.Generic;
using System.Linq;
using InfSurvivor.Runtime.Manager;
using Shared.Packet;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    #region 애니메이션 파라미터 (hash)
    protected readonly int ANIM_FLOAT_MOVE_X = Animator.StringToHash("MoveX");
    protected readonly int ANIM_FLOAT_MOVE_Y = Animator.StringToHash("MoveY");
    protected readonly int ANIM_FLOAT_SPEED = Animator.StringToHash("Speed");
    protected readonly int ANIM_BOOL_SWORD_ATTACKING = Animator.StringToHash("SwordAttacking");
    protected readonly int ANIM_TRIGGER_SWORD_ATTACK = Animator.StringToHash("SwordAttack");
    #endregion

    #region SerializeField
    [Header("Component")]
    private List<Animator> animators;
    private List<SpriteRenderer> spriteRenderers;

    [Header("Settings")]
    [SerializeField] private Vector2 collisionOffset;
    [SerializeField] private Vector2 collisionSize;
    [SerializeField] private float searchRange;
    #endregion
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
        animators = GetComponentsInChildren<Animator>().ToList();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
    }

    protected virtual void Start()
    {

    }

    private void Update()
    {
        OnUpdate();
    }

    private void FixedUpdate()
    {
        Tick();
        UpdateOccupiedCells();
    }

    protected virtual void OnUpdate()
    {

    }

    protected virtual void Tick()
    {

    }

    private void UpdateOccupiedCells()
    {
        Managers.Collision.UpdateOccupiedCells(gameObject, occupiedCells, collisionOffset, collisionSize);
    }

    protected void ApplyFacingDirection(Vector2 dir)
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
    protected void AnimationSetTrigger(int id)
    {
        animators.ForEach(anim => anim.SetTrigger(id));
    }

    protected void AnimationSetFloat(int id, float value)
    {
        animators.ForEach(anim => anim.SetFloat(id, value));
    }

    protected void AnimationSetBool(int id, bool value)
    {
        animators.ForEach(anim => anim.SetBool(id, value));
    }
    #endregion

    #region SpriteRenderer
    protected void SetFlip(bool right)
    {
        spriteRenderers.ForEach(sp => sp.flipX = right);
    }
    #endregion

    public virtual void OnUpdateMoveState(S_Move move)
    {
        
    }

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
