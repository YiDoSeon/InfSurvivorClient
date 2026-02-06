using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using UnityEngine;

public class DetectableObject : MonoBehaviour
{
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Managers.Collision.UpdateOccupiedCells(gameObject, occupiedCells, Vector2.zero, Vector2.one);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
        {
            return;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector2.one);

        Gizmos.color = Color.darkGreen;
        float cellSize = Managers.Collision.CellSize;

        foreach (Vector2Int cell in occupiedCells)
        {
            Vector2 worldPos = new Vector2(cell.x * cellSize, cell.y * cellSize);
            worldPos += Vector2.one * 0.5f * cellSize;

            Gizmos.DrawWireCube(worldPos , Vector2.one * cellSize);
        }
    }
}
