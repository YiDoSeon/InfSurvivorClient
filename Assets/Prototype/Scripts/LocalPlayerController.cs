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
        public Vector3 velocity;
    }
    private bool firePressed;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private Vector2 lastInput = Vector2.zero;
    private Vector3 targetPosition;
    private bool needsSyncMove;
    private uint seqNumber = 0;
    private List<PendingMove> pendingMoves = new List<PendingMove>();

    protected override void Start()
    {
        // TODO: 데이터 로드 방식으로 변경
        collisionOffset = new Vector2(0, 0.5f);
        collisionSize = new Vector2(1f, 1.76f);
        searchRange = 2.5f;

        InputAction moveAction = InputSystem.actions.FindAction("Move");
        moveAction.performed += OnMoveInput;
        moveAction.canceled += OnMoveInput;
        moveAction.Enable();

        InputAction fireAction = InputSystem.actions.FindAction("Jump");
        fireAction.performed += OnFireInput;
        fireAction.canceled += OnFireInput;
        fireAction.Enable();

        targetPosition = transform.position;
        StartCoroutine(CoSyncMovement());
    }

    private void OnDisable()
    {
        InputAction moveAction = InputSystem.actions.FindAction("Move");
        moveAction.performed -= OnMoveInput;
        moveAction.canceled -= OnMoveInput;
        moveAction.Disable();

        InputAction fireAction = InputSystem.actions.FindAction("Jump");
        fireAction.performed -= OnFireInput;
        fireAction.canceled -= OnFireInput;
        fireAction.Disable();
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

        if (moveInput != lastInput)
        {
            lastInput = moveInput;
            needsSyncMove = true;
        }

        // 방향이 바뀌었을 때만 변경, 정지시 마지막 방향을 사용
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
        if (needsSyncMove)
        {
            SendMovePacket();
        }
        else
        {
            CreatePendingMove(false);
        }

        if (firePressed == false)
        {
            targetPosition += (Vector3)moveInput * 5f * Time.fixedDeltaTime;
        }
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
                    Velocity = 5f * lastInput.ToCVector2(),
                }
            };
            Managers.Network.Send(movePacket);
        }
        PendingMove move = new PendingMove()
        {
            seqNumber = seqNum,
            velocity = 5 * lastInput
        };
        seqNumber++;
        pendingMoves.Add(move);
    }

    public override void OnUpdateMoveState(S_Move move)
    {
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
