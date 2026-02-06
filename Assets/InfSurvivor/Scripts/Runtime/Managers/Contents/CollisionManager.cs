using System.Collections.Generic;
using InfSurvivor.Runtime.System;
using InfSurvivor.Runtime.Utils;
using UnityEngine;

namespace InfSurvivor.Runtime.Manager
{
    public class CollisionManager : BehaviourManagerBase
    {
        private GridSystem gridVisualizer;
        public float CellSize => gridVisualizer.CellSize;

        public void SetGridVisualizer(GridSystem gridVisualizer)
        {
            this.gridVisualizer = gridVisualizer;
        }

        public void AddToCells(HashSet<Vector2Int> coords, GameObject go)
        {
            gridVisualizer.AddToCells(coords, go);
        }

        public void RemoveFromCells(HashSet<Vector2Int> coords, GameObject go)
        {
            gridVisualizer.RemoveFromCells(coords, go);
        }
        
        public void UpdateOccupiedCells(GameObject go, HashSet<Vector2Int> occupiedCells, Vector2 offset, Vector2 size)
        {
            Vector2 worldPos = offset + new Vector2(go.transform.position.x, go.transform.position.y);
            Vector2 halfSize = size * 0.5f;
            Vector2 minWorldPos = new Vector2(worldPos.x - halfSize.x, worldPos.y - halfSize.y);
            Vector2 maxWorldPos = new Vector2(worldPos.x + halfSize.x, worldPos.y + halfSize.y);

            Vector2Int minGridPos = minWorldPos.ToGridPos(CellSize);
            Vector2Int maxGridPos = maxWorldPos.ToGridPos(CellSize);

            RemoveFromCells(occupiedCells, go);
            occupiedCells.Clear();

            for (int x = minGridPos.x; x <= maxGridPos.x; x++)
            {
                for (int y = minGridPos.y; y <= maxGridPos.y; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    occupiedCells.Add(coord);
                }
            }

            AddToCells(occupiedCells, go);
        }

        public void UpdatePosition(Vector2Int oldPos, Vector2Int newPos, GameObject go)
        {
            gridVisualizer.UpdatePosition(oldPos, newPos, go);
        }
    }
}
