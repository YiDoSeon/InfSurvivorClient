using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

public class DetectableObject : MonoBehaviour, IColliderTrigger
{
    private HashSet<CVector2Int> occupiedCells = new HashSet<CVector2Int>();
    private CCircleCollider circleCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        circleCollider = new CCircleCollider(
            this,
            CVector2.zero,
            transform.position.ToCVector2(),
            0.5f);
        circleCollider.Layer = CollisionLayer.Monster;
        Managers.Collision.RegisterCollider(circleCollider);
    }

    private void OnDisable()
    {
        Managers.Collision?.UnregisterCollider(circleCollider);
        Managers.Collision?.RemoveFromCells(occupiedCells, circleCollider);
    }

    private void FixedUpdate()
    {
        circleCollider.UpdatePosition(transform.position.ToCVector2());
        Managers.Collision.UpdateOccupiedCells(circleCollider, occupiedCells);
    }

    public void OnCustomTriggerEnter(ColliderBase other)
    {
    }

    public void OnCustomTriggerStay(ColliderBase other)
    {
    }

    public void OnCustomTriggerExit(ColliderBase other)
    {
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

            Gizmos.DrawWireCube(worldPos , Vector2.one * cellSize);
        }
    }
}
