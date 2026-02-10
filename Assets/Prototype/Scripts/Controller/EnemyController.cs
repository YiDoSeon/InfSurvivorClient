using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

public class EnemyController : BaseController
{
    private CCircleCollider circleCollider;
    public override ColliderBase BodyCollider => circleCollider;

    public override void InitPos(PositionInfo posInfo)
    {
        transform.position =TargetMovePosition = posInfo.Pos.ToUnityVector2();
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
    }

    protected override void Update()
    {
        base.Update();
        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetMovePosition,
            MoveSpeed * Time.deltaTime
        );
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
