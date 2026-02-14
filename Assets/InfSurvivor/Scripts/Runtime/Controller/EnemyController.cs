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
    public class EnemyController : BaseController
    {
        private new SpriteRenderer renderer;
        private MaterialPropertyBlock propBlock;

        private CCircleCollider circleCollider;
        public override ColliderBase BodyCollider => circleCollider;

        private StateMachine<EnemyController, EnemyState> stateMachine;
        public override StateMachine StateMachine => stateMachine;
        public float KnockBackSpeed { get; private set; } = 10f;
        public float KnockBackTime { get; private set; } = 0.1f;
        public Vector2 PredictedLogicalPos { get; set; }

        #region Unity Events    
        protected override void Awake()
        {
            base.Awake();
            renderer = GetComponent<SpriteRenderer>();
            propBlock = new MaterialPropertyBlock();
        }

        protected override void Start()
        {
            base.Start();

            MoveSpeed = 3f;
        }
        #endregion

        #region BasePlayer Implements

        protected override void CreateBodyCollider()
        {
            circleCollider = new CCircleCollider(
                this,
                CVector2.zero,
                LogicalPos.ToCVector2(),
                0.5f);
            circleCollider.Layer = CollisionLayer.Monster;
            Managers.Collision.RegisterCollider(circleCollider);
        }
    
        protected override void CreateStateMachine()
        {
            stateMachine = new StateMachine<EnemyController, EnemyState>(this);
            stateMachine.AddState(EnemyState.Idle, new EnemyIdleState(this, stateMachine));
            stateMachine.AddState(EnemyState.Move, new EnemyMoveState(this, stateMachine));
            stateMachine.AddState(EnemyState.Damaged, new EnemyDamagedState(this, stateMachine));
            stateMachine.Initialize(EnemyState.Idle);
        }
    
        public override void InitPos(PositionInfo posInfo)
        {
            transform.position = LogicalPos = PredictedLogicalPos = posInfo.Pos.ToUnityVector2();
        }

        protected override void SyncTransform()
        {
            float speed = 1f;
            if (stateMachine.CurrentStateId == EnemyState.Damaged)
            {
                speed = KnockBackSpeed;
            }
            else if (stateMachine.CurrentStateId == EnemyState.Move)
            {
                speed = MoveSpeed;
            }
            SyncTransform(speed);
        }

        public void SyncTransform(float speed)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                LogicalPos,
                speed * Time.deltaTime
            );
        }

        protected override void UpdateLogicalPosition()
        {
            float speed = 0f;
            if (stateMachine.CurrentStateId == EnemyState.Damaged)
            {
                speed = KnockBackSpeed;
            }
            else if (stateMachine.CurrentStateId == EnemyState.Move)
            {
                speed = MoveSpeed;
            }
            LogicalPos = Vector2.MoveTowards(LogicalPos, PredictedLogicalPos, speed * Time.fixedDeltaTime);
        }
        #endregion
    
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
                LastVelocity += player.LastFacingDir;
            }
            
            stateMachine.ChangeState(EnemyState.Damaged);
            SetPredictedLogicalPos(LogicalPos + LastVelocity.normalized * KnockBackSpeed * KnockBackTime);
        }

        public void OnSyncDamagedState(CVector2 pos, CVector2 velocity)
        {
            LastVelocity = velocity.ToUnityVector2();

            stateMachine.ChangeState(EnemyState.Damaged);
            SetPredictedLogicalPos(pos.ToUnityVector2());
        }

        private void SetPredictedLogicalPos(Vector2 pos)
        {
            PredictedLogicalPos = pos;
            Debug.Log(PredictedLogicalPos);
        }

        public void ResetKnockBackDir()
        {
            LastVelocity = Vector2.zero;
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
}
