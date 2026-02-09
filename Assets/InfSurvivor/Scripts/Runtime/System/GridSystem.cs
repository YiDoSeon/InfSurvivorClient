using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using UnityEngine;

namespace InfSurvivor.Runtime.GameSystem
{
    public class GridSystem : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1.0f; // 한 칸의 크기
        public float CellSize => cellSize;
        public Color gridColor = Color.yellow;
        private readonly Dictionary<CVector2Int, HashSet<ColliderBase>> gridData = new Dictionary<CVector2Int, HashSet<ColliderBase>>();
        public Dictionary<CVector2Int, HashSet<ColliderBase>> GridData => gridData;

        private void Awake()
        {
            Managers.Collision.SetGridSystem(this);
        }

        private void Start()
        {
        }

        public void AddToCells(HashSet<CVector2Int> coords, ColliderBase go)
        {
            if (go == null)
            {
                return;
            }
            foreach (CVector2Int pos in coords)
            {
                if (gridData.TryGetValue(pos, out HashSet<ColliderBase> set) == false)
                {
                    set = new HashSet<ColliderBase>();
                    gridData[pos] = set;
                }
                set.Add(go);
            }
        }

        public void RemoveFromCells(HashSet<CVector2Int> coords, ColliderBase go)
        {
            if (go == null)
            {
                return;
            }
            foreach (CVector2Int pos in coords)
            {
                if (gridData.TryGetValue(pos, out HashSet<ColliderBase> set))
                {
                    gridData[pos].Remove(go);

                    if (set.Count == 0)
                    {
                        gridData.Remove(pos);
                    }
                }
            }
        }
    }
}
