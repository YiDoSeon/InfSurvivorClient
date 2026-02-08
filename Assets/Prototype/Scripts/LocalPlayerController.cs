using System.Collections;
using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerController : PlayerController
{
    public enum PlayerState
    {
        Idle,
        Move,
        Skill,
    }
    public struct PendingMove
    {
        public uint seqNumber;
        public Vector2 velocity;
    }
    private bool lastFirePressed;
    private Vector2 lastFacingDir = Vector2.down;
    private Vector2 lastMoveInput;
    private Vector2 LastVelocity
    {
        get
        {
            if (state == PlayerState.Move)
            {
                return lastMoveInput * moveSpeed;
            }            
            else
            {
                return Vector2.zero;
            }
        }        
    }

    private Vector2 targetPosition;
    private bool needsSyncMove;
    private uint seqNumber = 0;
    private PlayerState state;
    private List<PendingMove> pendingMoves = new List<PendingMove>();

    protected override void Start()
    {
        base.Start();
        // TODO: 데이터 로드 방식으로 변경
        collisionOffset = new Vector2(0, 0.5f);
        collisionSize = new Vector2(1f, 1.76f);
        searchRange = 2.5f;
        StartCoroutine(CoSyncMovement());
    }

    public override void InitPos(PositionInfo posInfo)
    {
        transform.position = targetPosition = posInfo.Pos.ToUnityVector3();
        lastFacingDir = posInfo.FacingDir.ToUnityVector2();
        ApplyFacingDirection(lastFacingDir);
        AnimationSetFloat(ANIM_FLOAT_SPEED, 0f);
    }

    protected override void OnEnable()
    {
        InputAction moveAction = InputSystem.actions.FindAction("Move");
        moveAction.performed += OnMoveInput;
        moveAction.canceled += OnMoveInput;
        moveAction.Enable();

        InputAction fireAction = InputSystem.actions.FindAction("Jump");
        fireAction.performed += OnFireInput;
        fireAction.canceled += OnFireInput;
        fireAction.Enable();
    }

    protected override void OnDisable()
    {
        InputAction moveAction = InputSystem.actions.FindAction("Move");
        moveAction.performed -= OnMoveInput;
        moveAction.canceled -= OnMoveInput;
        moveAction.Disable();

        InputAction fireAction = InputSystem.actions.FindAction("Jump");
        fireAction.performed -= OnFireInput;
        fireAction.canceled -= OnFireInput;
        fireAction.Disable();

        base.OnDisable();
    }

    private void OnFireInput(InputAction.CallbackContext context)
    {
        bool firePressed = context.ReadValue<float>() > 0f;
        if (firePressed != lastFirePressed)
        {
            if (firePressed)
            {
                state = PlayerState.Skill;
                AnimationSetTrigger(ANIM_TRIGGER_SWORD_ATTACK);
            }
            else
            {
                if (lastMoveInput.sqrMagnitude > 0.01f)
                {
                    state = PlayerState.Move;
                }
                else
                {
                    state = PlayerState.Idle;
                }
            }
            AnimationSetBool(ANIM_BOOL_SWORD_ATTACKING, firePressed);

            lastFirePressed = firePressed;
            needsSyncMove = true;
        }
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        if (state == PlayerState.Idle)
        {
            state = PlayerState.Move;
        }

        if (moveInput != lastMoveInput)
        {
            lastMoveInput = moveInput;
            needsSyncMove = true;
        }

        // 바라보는 방향은 이동 방향이 바뀌었을 때만 변경, 정지시 마지막 방향을 사용
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastFacingDir = moveInput;
        }
    }

    protected override void OnUpdate()
    {
        ApplyFacingDirection(lastFacingDir);

        switch (state)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;
            case PlayerState.Move:
                UpdateMove();
                break;
            case PlayerState.Skill:
                UpdateSkill();
                break;
        }
    }

    private void UpdateIdle()
    {
        AnimationSetFloat(ANIM_FLOAT_SPEED, LastVelocity.sqrMagnitude);
    }

    private void UpdateMove()
    {
        AnimationSetFloat(ANIM_FLOAT_SPEED, LastVelocity.sqrMagnitude);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private void UpdateSkill()
    {
    }

    protected override void Tick()
    {
        if (needsSyncMove)
        {
            SendMovePacket();
        }
        else
        {
            CreatePendingMove(false);
        }
        switch (state)
        {
            case PlayerState.Idle:
                TickIdle();
                break;
            case PlayerState.Move:
                TickMove();
                break;
            case PlayerState.Skill:
                TickSkill();
                break;
        }
    }

    private void TickIdle()
    {

    }

    private void TickMove()
    {
        targetPosition += LastVelocity * Time.fixedDeltaTime;
    }

    private void TickSkill()
    {

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
                    Pos = targetPosition.ToCVector2(),
                    Velocity = LastVelocity.ToCVector2(),
                    FacingDir = lastFacingDir.ToCVector2(),
                    FirePressed = lastFirePressed,
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
        //
        pendingMoves.RemoveAll(m =>
        {
            /*  [SeqNumber 순환 처리]

                햇갈릴 수 있어 몇 가지 시나리오 작성함!
                diff: (서버 검증 패킷 번호 - 팬딩 패킷 번호)
                A: 서버 100, 팬딩 0 => diff (int)100 (제거 O)
                B: 서버 uint.MaxValue, 팬딩 0 => diff (int)(uint.MaxValue) == -1 (제거 X)
                C: 서버 0, 팬딩 uint.MaxValue => diff (int)(-uint.MaxValue) == 1 (제거 O)

                diff > 0 : 팬딩 패킷이 더 오래됨
                diff == 0 : 이번에 서버에서 넘겨준 검증된 패킷
                diff < 0 : 서버에서 아직 검증하지않은 패킷
            */
            return (int)(move.SeqNumber - m.seqNumber) >= 0;
        });

        targetPosition = move.PosInfo.Pos.ToUnityVector2();

        foreach (var m in pendingMoves)
        {
            targetPosition += m.velocity * Time.fixedDeltaTime;
        }
    }

    private IEnumerator CoSyncMovement()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            yield return wait;
            needsSyncMove = true;
        }
    }

}
