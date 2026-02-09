using System.Collections;
using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using UnityEngine;
using UnityEngine.InputSystem;

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

    protected override void Awake()
    {
        base.Awake();
        InputHandler = gameObject.AddComponent<PlayerInputHandler>();
        AnimationEvents = gameObject.GetComponentInChildren<PlayerAnimationEvents>();
    }

    protected override void Start()
    {
        base.Start();
        AnimationEvents.SetPlayer(this);
        CreateStateMachine();
        StartCoroutine(CoSyncMovement());
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
        transform.position = TargetPosition = posInfo.Pos.ToUnityVector3();
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
            TargetPosition,
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
        TargetPosition += LastVelocity * Time.fixedDeltaTime;
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
                    Pos = TargetPosition.ToCVector2(),
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
        float before = TargetPosition.sqrMagnitude;
        pendingMoves.RemoveAll(m =>
        {
            return (int)(move.SeqNumber - m.seqNumber) >= 0;
        });

        TargetPosition = move.PosInfo.Pos.ToUnityVector2();

        foreach (var m in pendingMoves)
        {
            TargetPosition += m.velocity * Time.fixedDeltaTime;
        }
        float after = TargetPosition.sqrMagnitude;
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
            Gizmos.color = Color.red;
            Vector3 offset = transform.position + (Vector3)(CollisionOffset + Dir4 * 0.7f);
            Gizmos.DrawWireSphere(offset, 0.75f);
        }
    }
#endif
}
