using System.Collections.Generic;
using System.Linq;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public abstract class PlayerController : MonoBehaviour
{
    #region 애니메이션 파라미터 (hash)
    public readonly int ANIM_FLOAT_MOVE_X = Animator.StringToHash("MoveX");
    public readonly int ANIM_FLOAT_MOVE_Y = Animator.StringToHash("MoveY");
    public readonly int ANIM_FLOAT_SPEED = Animator.StringToHash("Speed");
    public readonly int ANIM_BOOL_SWORD_ATTACKING = Animator.StringToHash("SwordAttacking");
    public readonly int ANIM_TRIGGER_SWORD_ATTACK = Animator.StringToHash("SwordAttack");
    #endregion

    #region SerializeField
    [Header("Component")]
    public List<Animator> Animators { get; private set; }
    public List<SpriteRenderer> Renderers { get; private set; }

    [Header("Settings")]
    public Vector2 CollisionOffset { get; private set; }
    public Vector2 CollisionSize { get; private set; }
    public float SearchRange { get; private set; }
    public float MoveSpeed { get; private set; }
    #endregion
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    public ObjectInfo Info { get; set; } = new ObjectInfo();
    public int Id
    {
        get => Info.ObjectId;
        set => Info.ObjectId = value;
    }
    public Vector2 LastFacingDir { get; protected set; } = Vector2.down;
    public Vector2 LastVelocity { get; protected set; }
    public Vector2 TargetPosition { get; protected set; }

    protected virtual void Awake()
    {
        Animators = GetComponentsInChildren<Animator>().ToList();
        Renderers = GetComponentsInChildren<SpriteRenderer>().ToList();
        MoveSpeed = 5f;
    }

    protected virtual void Start()
    {
        // TODO: 데이터 로드 방식으로 변경
        CollisionOffset = new Vector2(0, 0.5f);
        CollisionSize = new Vector2(1f, 1.76f);
        SearchRange = 2.5f;
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
        Managers.Collision?.RemoveFromCells(occupiedCells, gameObject);
    }

    protected virtual void Update()
    {
    }

    protected virtual void FixedUpdate()
    {
        UpdateOccupiedCells();
    }

    public abstract void InitPos(PositionInfo posInfo);

    private void UpdateOccupiedCells()
    {
        Managers.Collision.UpdateOccupiedCells(gameObject, occupiedCells, CollisionOffset, CollisionSize);
    }


    public void ApplyFacingDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude <= 0.01f)
        {
            return;
        }

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

        LastFacingDir = dir;
    }

    #region Animation Parameter
    public void AnimationSetTrigger(int id)
    {
        Animators.ForEach(anim => anim.SetTrigger(id));
    }

    public void AnimationSetFloat(int id, float value)
    {
        Animators.ForEach(anim => anim.SetFloat(id, value));
    }

    public void AnimationSetBool(int id, bool value)
    {
        Animators.ForEach(anim => anim.SetBool(id, value));
    }
    #endregion

    #region SpriteRenderer
    public void SetFlip(bool right)
    {
        Renderers.ForEach(sp => sp.flipX = right);
    }
    #endregion

    public virtual void OnUpdateMoveState(S_Move move)
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube((Vector3)CollisionOffset + transform.position, new Vector3(CollisionSize.x, CollisionSize.y));
        Gizmos.DrawWireSphere((Vector3)CollisionOffset + transform.position, SearchRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.black;
            List<GameObject> nearObjects = Managers.Collision.GetObjectsInRange(transform.position, CollisionOffset, SearchRange);
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
