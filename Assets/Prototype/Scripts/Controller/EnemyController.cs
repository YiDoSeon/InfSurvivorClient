using System.Collections;
using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.FSM;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

public class EnemyController : BaseController
{
    private CCircleCollider circleCollider;
    public override ColliderBase BodyCollider => circleCollider;
    private new SpriteRenderer renderer;
    private MaterialPropertyBlock propBlock;
    private StateMachine<EnemyController, EnemyState> stateMachine;
    public Vector2 KnockBackDir { get; private set; }
    private float knockBackSpeed = 5f;

    protected override void Awake()
    {
        base.Awake();
        renderer = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
    }

    protected override void Start()
    {
        base.Start();
        circleCollider = new CCircleCollider(
            this,
            CVector2.zero,
            TargetMovePosition.ToCVector2(),
            0.5f);
        circleCollider.Layer = CollisionLayer.Monster;
        Managers.Collision.RegisterCollider(circleCollider);

        MoveSpeed = 3f;
        CreateStateMachine();
    }

    private void CreateStateMachine()
    {
        stateMachine = new StateMachine<EnemyController, EnemyState>(this);
        stateMachine.AddState(EnemyState.Idle, new EnemyIdleState(this, stateMachine));
        stateMachine.AddState(EnemyState.Move, new EnemyMoveState(this, stateMachine));
        stateMachine.AddState(EnemyState.Damaged, new EnemyDamagedState(this, stateMachine));
        stateMachine.Initialize(EnemyState.Idle);
    }

    public override void InitPos(PositionInfo posInfo)
    {
        transform.position = TargetMovePosition = posInfo.Pos.ToUnityVector2();
    }

    protected override void Update()
    {
        base.Update();
        float speed = 0f;
        if (stateMachine.CurrentStateId == EnemyState.Damaged)
        {
            speed = knockBackSpeed;
        }
        else if (stateMachine.CurrentStateId == EnemyState.Move)
        {
            speed = MoveSpeed;
        }
        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetMovePosition,
            speed * Time.deltaTime
        );

        stateMachine.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        float speed = 0f;
        if (stateMachine.CurrentStateId == EnemyState.Damaged)
        {
            speed = knockBackSpeed;
        }
        else if (stateMachine.CurrentStateId == EnemyState.Move)
        {
            speed = MoveSpeed;
        }
        TargetMovePosition += LastVelocity * speed * Time.deltaTime;
        stateMachine.FixedUpdate();
    }

    public void SetVelocity(Vector2 velocity)
    {
        LastVelocity = velocity;
    }

    public void SetFlash(float amount)
    {
        renderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat("_FlashAmount", amount);
        renderer.SetPropertyBlock(propBlock);
    }

    public void OnDamaged(BaseController sender)
    {
        if (sender is LocalPlayerController player)
        {
            KnockBackDir = player.LastFacingDir;
            //Debug.Log(KnockBackDir);
        }
        //Debug.Log(gameObject.name);
        stateMachine.ChangeState(EnemyState.Damaged);
    }

    public void KnockBack()
    {
        TargetMovePosition += KnockBackDir.normalized * knockBackSpeed * Time.fixedDeltaTime;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
        {
            return;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(circleCollider.Center.ToUnityVector3(), circleCollider.Radius);

        Gizmos.color = Color.darkGreen;
        float cellSize = Managers.Collision.CellSize;

        foreach (CVector2Int cell in occupiedCells)
        {
            Vector2 worldPos = new Vector2(cell.x * cellSize, cell.y * cellSize);
            worldPos += Vector2.one * 0.5f * cellSize;

            Gizmos.DrawWireCube(worldPos, Vector2.one * cellSize);
        }
    }

}
