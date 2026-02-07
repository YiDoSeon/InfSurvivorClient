using System;
using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using UnityEngine;

namespace InfSurvivor.Runtime.GameSystem
{
    public class GridSystem : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1.0f; // 한 칸의 크기
        public float CellSize => cellSize;
        public Color gridColor = Color.yellow;
        private readonly Dictionary<Vector2Int, HashSet<GameObject>> gridData = new Dictionary<Vector2Int, HashSet<GameObject>>();
        public Dictionary<Vector2Int, HashSet<GameObject>> GridData => gridData;

        private void Awake()
        {
            Managers.Collision.SetGridSystem(this);
        }

        private void Start()
        {
        }

        public void AddToCells(HashSet<Vector2Int> coords, GameObject go)
        {
            if (go == null)
            {
                return;
            }
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
            if (go == null)
            {
                return;
            }
            foreach (Vector2Int pos in coords)
            {
                if (gridData.TryGetValue(pos, out HashSet<GameObject> set))
                {
                    gridData[pos].Remove(go);

                    if (set.Count == 0)
                    {
                        gridData.Remove(pos);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            // 시작점 계산 (오브젝트 위치 기준)
            // Vector2 origin = transform.position + this.origin;

            // foreach (var kvp in gridData)
            // {
            //     Vector2Int coord = kvp.Key;
            //     HashSet<GameObject> gameObjects = kvp.Value;

            //     Vector2 worldPos = new Vector2(coord.x * cellSize, coord.y * cellSize);
            //     worldPos += Vector2.one * 0.5f * cellSize;
            //     worldPos += origin;

            //     foreach (GameObject gameObject in gameObjects)
            //     {
            //         if (gameObject.name == "Player")
            //         {
            //             Gizmos.color = Color.yellow;
            //         }
            //         else
            //         {
            //             Gizmos.color = Color.darkGreen;
            //         }
            //         Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, cellSize, 0f));
            //     }
            // }
        }
    }
}
