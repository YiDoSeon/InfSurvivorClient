using System.Collections;
using System.Collections.Generic;
using InfSurvivor.Runtime.Controller.FSM;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.FSM;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

namespace InfSurvivor.Runtime.Controller
{
    public class LocalPlayerController : PlayerController
    {
        public struct PendingMove
        {
            public uint seqNumber;
            public Vector2 velocity;
        }
        private bool needsSyncMove;
        private uint moveSeq = 0;
        public uint meleeAttackSeq = 0;
        private List<PendingMove> pendingMoves = new List<PendingMove>();

        public PlayerInputHandler InputHandler { get; private set; }
        public PlayerAnimationEvents AnimationEvents { get; private set; }

        private CBoxCollider bodyCollider;
        public override ColliderBase BodyCollider => bodyCollider;
        public CCircleCollider MeleeAttackCollider { get; private set; }

        private StateMachine<LocalPlayerController, PlayerState> stateMachine;
        public override StateMachine StateMachine => stateMachine;

        #region Unity Events
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
        #endregion

        #region BasePlayer Implements
        protected override void CreateBodyCollider()
        {
            // TODO: 데이터 로드 방식으로 변경
            bodyCollider = new CBoxCollider(
                this,
                new CVector2(0f, 0.5f),
                LogicalPos.ToCVector2(),
                new CVector2(0.6f, 1f));
            bodyCollider.Layer = CollisionLayer.Player;
            Managers.Collision.RegisterCollider(bodyCollider);

            MeleeAttackCollider = new CCircleCollider(
                this,
                new CVector2(0f, 0.5f),
                LogicalPos.ToCVector2(),
                0.75f);
            Managers.Collision.RegisterCollider(MeleeAttackCollider);
        }

        protected override void CreateStateMachine()
        {
            stateMachine = new StateMachine<LocalPlayerController, PlayerState>(this);
            stateMachine.AddState(PlayerState.Idle, new PlayerIdleState(this, stateMachine));
            stateMachine.AddState(PlayerState.Move, new PlayerMoveState(this, stateMachine));
            stateMachine.AddState(PlayerState.MeleeAttack, new PlayerMeleeAttackState(this, stateMachine));
            stateMachine.Initialize(PlayerState.Idle);
        }

        public override void InitPos(PositionInfo posInfo)
        {
            transform.position = LogicalPos = posInfo.Pos.ToUnityVector3();
            ApplyFacingDirection(posInfo.FacingDir.ToUnityVector2());
            AnimationSetFloat(ANIM_FLOAT_SPEED, 0f);
        }

        protected override void SyncTransform()
        {
            AnimationSetFloat(ANIM_FLOAT_SPEED, LastVelocity.sqrMagnitude);

            transform.position = Vector3.MoveTowards(
                transform.position,
                LogicalPos,
                MoveSpeed * Time.deltaTime
            );
        }

        protected override void BeforeUpdateLogicalPosition()
        {
            if (needsSyncMove)
            {
                SendMovePacket();
            }
            else
            {
                CreatePendingMove(false);
            }
        }

        protected override void UpdateLogicalPosition()
        {
            LogicalPos += LastVelocity * Time.fixedDeltaTime;
        }
        #endregion

        #region Movement
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

        private void SendMovePacket()
        {
            CreatePendingMove(true);
            needsSyncMove = false;
        }

        private void CreatePendingMove(bool needsSync)
        {
            uint seqNum = this.moveSeq;
            if (needsSync)
            {
                C_Move movePacket = new C_Move()
                {
                    SeqNumber = seqNum,
                    PosInfo = new PositionInfo()
                    {
                        Pos = LogicalPos.ToCVector2(),
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
            moveSeq++;
            pendingMoves.Add(move);
        }

        public override void OnUpdateMoveState(S_Move move)
        {
            float before = LogicalPos.sqrMagnitude;
            pendingMoves.RemoveAll(m =>
            {
                return move.SeqNumber.IsAfterOrEqual(m.seqNumber);
            });

            LogicalPos = move.PosInfo.Pos.ToUnityVector2();

            foreach (var m in pendingMoves)
            {
                LogicalPos += m.velocity * Time.fixedDeltaTime;
            }
            float after = LogicalPos.sqrMagnitude;
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

        #endregion

        #region MeleeAttack
        public void CheckMeleeAttack()
        {
            MeleeAttackCollider.Position = LogicalPos.ToCVector2() + Dir4.ToCVector2() * 0.8f;

            List<ColliderBase> colliders = Managers.Collision.GetOverlappedColliders(
                MeleeAttackCollider,
                targetMask: Shared.Utils.Extensions.CombineMask(CollisionLayer.Monster));
            SendMeleeAttackPacket();
            foreach (ColliderBase collider in colliders)
            {
                if (collider.Owner is EnemyController enemy)
                {
                    enemy.OnDamaged(this);
                }
            }
        }
        public void SendMeleeAttackPacket()
        {
            C_MeleeAttack meleeAttackPacket = new C_MeleeAttack();
            Managers.Network.Send(meleeAttackPacket);
        }

        public void OnMeleeAttackConfirm(S_MeleeAttack meleeAttack)
        {
            foreach (DamagedElement target in meleeAttack.Targets)
            {
                EnemyController ec = Managers.Object.FindEnemyById(target.EnemyId);
                if (ec == null)
                {
                    continue;
                }
                ec.OnSyncDamagedState(target.Pos, target.Velocity);
            }
        }
        #endregion

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(bodyCollider.Center.ToUnityVector3(), bodyCollider.Size.ToUnityVector3());
            }
        }
#endif
    }
}