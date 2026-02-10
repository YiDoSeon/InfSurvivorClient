using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

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
    public Vector2 TargetMovePosition { get; protected set; }
    public virtual ColliderBase BodyCollider { get; }
    protected HashSet<CVector2Int> occupiedCells = new HashSet<CVector2Int>();

    #region Unity Events
    protected virtual void Awake() { }
    protected virtual void Start() { }
    protected virtual void OnEnable() { }
    protected virtual void OnDisable()
    {
        if (BodyCollider != null)
        {
            Managers.Collision?.UnregisterCollider(BodyCollider);
            Managers.Collision?.RemoveFromCells(occupiedCells, BodyCollider);            
        }
    }
    protected virtual void Update() { }
    protected virtual void FixedUpdate()
    {
        BodyCollider.UpdatePosition(TargetMovePosition.ToCVector2());
        Managers.Collision.UpdateOccupiedCells(BodyCollider, occupiedCells);
    }
    #endregion

    public abstract void InitPos(PositionInfo posInfo);
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
