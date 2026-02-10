using System.Collections;
using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.FSM;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

public class LocalPlayerController : PlayerController
{
    public struct PendingMove
    {
        public uint seqNumber;
        public Vector2 velocity;
    }
    private bool needsSyncMove;
    private uint seqNumber = 0;
    private List<PendingMove> pendingMoves = new List<PendingMove>();

    private StateMachine<LocalPlayerController, PlayerState> stateMachine;
    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerAnimationEvents AnimationEvents { get; private set; }
    private CBoxCollider bodyCollider;
    public override ColliderBase BodyCollider => bodyCollider;
    public CCircleCollider MeleeAttackCollider { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InputHandler = gameObject.AddComponent<PlayerInputHandler>();
        AnimationEvents = gameObject.GetComponentInChildren<PlayerAnimationEvents>();
    }

    protected override void Start()
    {
        base.Start();
        // TODO: 데이터 로드 방식으로 변경
        bodyCollider = new CBoxCollider(
            this,
            new CVector2(0f, 0.5f),
            TargetMovePosition.ToCVector2(),
            new CVector2(0.6f, 1f));
        bodyCollider.Layer = CollisionLayer.Player;
        Managers.Collision.RegisterCollider(bodyCollider);

        MeleeAttackCollider = new CCircleCollider(
            this,
            new CVector2(0f, 0.5f),
            TargetMovePosition.ToCVector2(),
            0.75f);
        Managers.Collision.RegisterCollider(MeleeAttackCollider);

        AnimationEvents.SetPlayer(this);
        CreateStateMachine();
        StartCoroutine(CoSyncMovement());
    }

    protected override void OnDisable()
    {
        if (MeleeAttackCollider != null)
        {
            Managers.Collision?.UnregisterCollider(MeleeAttackCollider);
            Managers.Collision?.RemoveFromCells(occupiedCells, MeleeAttackCollider);            
        }
        base.OnDisable();
    }

    private void CreateStateMachine()
    {
        stateMachine = new StateMachine<LocalPlayerController, PlayerState>(this);
        stateMachine.AddState(PlayerState.Idle, new PlayerIdleState(this, stateMachine));
        stateMachine.AddState(PlayerState.Move, new PlayerMoveState(this, stateMachine));
        stateMachine.AddState(PlayerState.MeleeAttack, new PlayerMeleeAttackState(this, stateMachine));
        stateMachine.Initialize(PlayerState.Idle);
    }

    public override void InitPos(PositionInfo posInfo)
    {
        transform.position = TargetMovePosition = posInfo.Pos.ToUnityVector3();
        ApplyFacingDirection(posInfo.FacingDir.ToUnityVector2());
        AnimationSetFloat(ANIM_FLOAT_SPEED, 0f);
    }

    public void SetDirtySyncMove()
    {
        needsSyncMove = true;
    }

    public void UpdateVelocity(PlayerState state)
    {
        if (state == PlayerState.Move)
        {
            LastVelocity = MoveSpeed * InputHandler.MoveInput;
        }
        else
        {
            LastVelocity = Vector2.zero;
        }
    }

    protected override void Update()
    {
        base.Update();
        UpdateMovement();
        stateMachine.Update();
    }

    public void UpdateMovement()
    {
        AnimationSetFloat(ANIM_FLOAT_SPEED, LastVelocity.sqrMagnitude);

        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetMovePosition,
            MoveSpeed * Time.deltaTime
        );
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (needsSyncMove)
        {
            SendMovePacket();
        }
        else
        {
            CreatePendingMove(false);
        }
        UpdateTargetPosition();
        stateMachine.FixedUpdate();
    }

    public void UpdateTargetPosition()
    {
        TargetMovePosition += LastVelocity * Time.fixedDeltaTime;
    }

    private void SendMovePacket()
    {
        CreatePendingMove(true);
        needsSyncMove = false;
    }

    private void CreatePendingMove(bool needsSync)
    {
        uint seqNum = this.seqNumber;
        if (needsSync)
        {
            C_Move movePacket = new C_Move()
            {
                SeqNumber = seqNum,
                PosInfo = new PositionInfo()
                {
                    Pos = TargetMovePosition.ToCVector2(),
                    Velocity = LastVelocity.ToCVector2(),
                    FacingDir = LastFacingDir.ToCVector2(),
                    FirePressed = InputHandler.FirePressed,
                }
            };
            Managers.Network.Send(movePacket);
        }
        PendingMove move = new PendingMove()
        {
            seqNumber = seqNum,
            velocity = LastVelocity
        };
        seqNumber++;
        pendingMoves.Add(move);
    }

    public override void OnUpdateMoveState(S_Move move)
    {
        float before = TargetMovePosition.sqrMagnitude;
        pendingMoves.RemoveAll(m =>
        {
            return (int)(move.SeqNumber - m.seqNumber) >= 0;
        });

        TargetMovePosition = move.PosInfo.Pos.ToUnityVector2();

        foreach (var m in pendingMoves)
        {
            TargetMovePosition += m.velocity * Time.fixedDeltaTime;
        }
        float after = TargetMovePosition.sqrMagnitude;
        if (before - after > 0.01f)
        {
            Debug.Log("튐");
        }
    }

    private IEnumerator CoSyncMovement()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            yield return wait;
            SetDirtySyncMove();
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bodyCollider.Center.ToUnityVector3(), bodyCollider.Size.ToUnityVector3());

            // Gizmos.color = Color.red;
            // CVector2 offset = transform.position.ToCVector2() + BodyCollider.Offset + Dir4.ToCVector2() * 0.7f;
            // Gizmos.DrawWireSphere(offset.ToUnityVector3(), 0.75f);
        }
    }
#endif
}
