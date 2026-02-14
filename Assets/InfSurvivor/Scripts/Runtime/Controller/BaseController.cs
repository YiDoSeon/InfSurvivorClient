using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.FSM;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

namespace InfSurvivor.Runtime.Controller
{
    public abstract class BaseController : MonoBehaviour, IColliderTrigger
    {
        #region Stats
        public float MoveSpeed { get; protected set; }
        #endregion

        #region Network Property
        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public int Id
        {
            get => Info.ObjectId;
            set => Info.ObjectId = value;
        }
        #endregion

        public Vector2 LastVelocity { get; protected set; }
        /// <summary>
        /// 실제 로직 처리에 사용되는 위치
        /// </summary>
        /// <value></value>
        public Vector2 LogicalPos { get; protected set; }
        public virtual ColliderBase BodyCollider { get; }
        public virtual StateMachine StateMachine { get; }
        protected HashSet<CVector2Int> occupiedCells = new HashSet<CVector2Int>();

        #region Unity Events
        protected virtual void Awake() { }
        protected virtual void Start()
        {
            CreateBodyCollider();
            CreateStateMachine();
        }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable()
        {
            if (BodyCollider != null)
            {
                Managers.Collision?.UnregisterCollider(BodyCollider);
                Managers.Collision?.RemoveFromCells(occupiedCells, BodyCollider);
            }
        }
        protected virtual void Update()
        {
            SyncTransform();
            StateMachine?.Update();
        }

        protected virtual void FixedUpdate()
        {
            if (BodyCollider != null)
            {
                BodyCollider.UpdatePosition(LogicalPos.ToCVector2());
                Managers.Collision.UpdateOccupiedCells(BodyCollider, occupiedCells);
            }
            BeforeUpdateLogicalPosition();
            UpdateLogicalPosition();
            StateMachine?.FixedUpdate();
        }
        #endregion

        public abstract void InitPos(PositionInfo posInfo);
        protected virtual void CreateBodyCollider() { }
        protected virtual void CreateStateMachine() { }
        protected virtual void SyncTransform() { }
        protected virtual void BeforeUpdateLogicalPosition() {}
        protected virtual void UpdateLogicalPosition() { }
        public virtual void ApplyFacingDirection(Vector2 dir) { }

        #region Collider Trigger
        public virtual void OnCustomTriggerEnter(ColliderBase other) { }

        public virtual void OnCustomTriggerExit(ColliderBase other) { }

        public virtual void OnCustomTriggerStay(ColliderBase other) { }
        #endregion

        #region Network Response Method
        public virtual void OnUpdateMoveState(S_Move move)
        {

        }
        #endregion
    }
}
