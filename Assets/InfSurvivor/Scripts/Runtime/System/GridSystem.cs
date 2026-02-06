using System;
using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using UnityEngine;

namespace InfSurvivor.Runtime.System
{
    public class GridSystem : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1.0f; // 한 칸의 크기
        [SerializeField] private Vector3 origin;
        public Color gridColor = Color.yellow;

        public float CellSize => cellSize;
        private Dictionary<Vector2Int, HashSet<GameObject>> gridData = new Dictionary<Vector2Int, HashSet<GameObject>>();
        public static readonly Vector2Int[] Direction9 =
        {
            new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1),
            new Vector2Int(-1, 0),  new Vector2Int(0, 0),  new Vector2Int(1, 0),
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        };

        private void Awake()
        {
            Managers.Collision.SetGridVisualizer(this);
        }

        private void Start()
        {
        }
        
        public void AddToCells(HashSet<Vector2Int> coords, GameObject go)
        {
            foreach (Vector2Int pos in coords)
            {
                if (gridData.TryGetValue(pos, out HashSet<GameObject> set) == false)
                {
                    set = new HashSet<GameObject>();
                    gridData[pos] = set;
                }
                set.Add(go);
            }
        }

        public void RemoveFromCells(HashSet<Vector2Int> coords, GameObject go)
        {
            foreach (Vector2Int pos in coords)
            {
                if (gridData.TryGetValue(pos, out HashSet<GameObject> set))
                {
                    gridData[pos].Remove(go);
                }

                if (set.Count == 0)
                {
                    gridData.Remove(pos);
                }
            }
        }

        public void UpdatePosition(Vector2Int oldPos, Vector2Int newPos, GameObject go)
        {
            if (gridData.TryGetValue(oldPos, out HashSet<GameObject> oldSet))
            {
                oldSet.Remove(go);
                if (oldSet.Count == 0)
                {
                    gridData.Remove(oldPos);
                }
            }

            if (gridData.TryGetValue(newPos, out HashSet<GameObject> newSet) == false)
            {
                newSet = new HashSet<GameObject>();
                gridData[newPos] = newSet;
            }
            newSet.Add(go);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            // 시작점 계산 (오브젝트 위치 기준)
            Vector3 origin = transform.position + this.origin;

            Gizmos.color = Color.blue;
            foreach (var kvp in gridData)
            {
                Vector2Int coord = kvp.Key;
                HashSet<GameObject> gameObjects = kvp.Value;

                Vector3 worldPos = new Vector3(coord.x * cellSize, coord.y * cellSize);
                worldPos += Vector3.one * 0.5f * cellSize;
                worldPos += origin;
                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, cellSize, 0f));

                foreach (GameObject gameObject in gameObjects)
                {
                    if (gameObject.name == "Player")
                    {
                        Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, cellSize, 0f));
                    }
                }
            }
        }
    }
}
