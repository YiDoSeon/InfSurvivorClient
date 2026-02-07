using System.Collections.Generic;
using System.Linq;
using InfSurvivor.Runtime.GameSystem;
using InfSurvivor.Runtime.Utils;
using UnityEngine;

namespace InfSurvivor.Runtime.Manager
{
    public class CollisionManager : BehaviourManagerBase
    {
        private GridSystem gridSystem;
        public float CellSize => gridSystem.CellSize;

        public void SetGridSystem(GridSystem gridVisualizer)
        {
            this.gridSystem = gridVisualizer;
        }

        public void UpdateOccupiedCells(GameObject go, HashSet<Vector2Int> occupiedCells, Vector2 offset, Vector2 size)
        {
            Vector2 worldPos = offset + new Vector2(go.transform.position.x, go.transform.position.y);
            Vector2 halfSize = size * 0.5f;
            Vector2 minWorldPos = new Vector2(worldPos.x - halfSize.x, worldPos.y - halfSize.y);
            Vector2 maxWorldPos = new Vector2(worldPos.x + halfSize.x, worldPos.y + halfSize.y);

            Vector2Int minGridPos = minWorldPos.ToGridPos(CellSize);
            Vector2Int maxGridPos = maxWorldPos.ToGridPos(CellSize);

            gridSystem.RemoveFromCells(occupiedCells, go);
            occupiedCells.Clear();

            for (int x = minGridPos.x; x <= maxGridPos.x; x++)
            {
                for (int y = minGridPos.y; y <= maxGridPos.y; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    occupiedCells.Add(coord);
                }
            }

            gridSystem.AddToCells(occupiedCells, go);
        }

        public List<GameObject> GetObjectsInRange(Vector2 center, Vector2 offset, float range, bool alsoCheckObjectBox = true)
        {
            HashSet<GameObject> result = new HashSet<GameObject>();

            float targetX = center.x + offset.x;
            float targetY = center.y + offset.y;
            float sqrRange = range * range;

            int minX = Mathf.FloorToInt((targetX - range) / CellSize);
            int maxX = Mathf.FloorToInt((targetX + range) / CellSize);
            int minY = Mathf.FloorToInt((targetY - range) / CellSize);
            int maxY = Mathf.FloorToInt((targetY + range) / CellSize);

            // range(반지름) * 2의 사각형 영역 검색
            for (int x = minX; x <= maxX; x++)
            {
                float dSqrX = GetMinSqrDistanceToCell(x, targetX, CellSize); 

                for (int y = minY; y <= maxY; y++)
                {
                    // 범위 내 grid pos
                    Vector2Int key = new Vector2Int(x, y);

                    if (gridSystem.GridData.TryGetValue(key, out HashSet<GameObject> set) == false)
                    {
                        // 아무것도 없음
                        continue;
                    }

                    float dSqrY = GetMinSqrDistanceToCell(y, targetY, CellSize);

                    if (dSqrX + dSqrY > sqrRange)
                    {
                        // 범위 밖에 있음
                        continue;
                    }

                    foreach (GameObject obj in set)
                    {
                        if (alsoCheckObjectBox)
                        {
                            Vector2 pos = obj.transform.position;
                            Vector2 halfSize = 0.5f * Vector2.one;

                            // 유닛 박스의 범위
                            float oMinX = pos.x - halfSize.x;
                            float oMaxX = pos.x + halfSize.x;
                            float oMinY = pos.y - halfSize.y;
                            float oMaxY = pos.y + halfSize.y;

                            // 타겟 중심에서 유닛 박스까지의 가장 가까운 거리 계산 (아까 만든 로직과 동일!)
                            float closestX = Mathf.Clamp(targetX, oMinX, oMaxX);
                            float closestY = Mathf.Clamp(targetY, oMinY, oMaxY);

                            float dx = closestX - targetX;
                            float dy = closestY - targetY;

                            if ((dx * dx) + (dy * dy) > sqrRange)
                            {
                                // 범위 밖에 있음
                                continue;
                            }                            
                        }
                        result.Add(obj);
                    }
                }
            }

            return result.ToList();
        }
        
        private float GetMinSqrDistanceToCell(float gridIndex, float targetPos, float cellSize)
        {
            // 그리드 축의 최소/최대 월드 좌표 계산
            float min = gridIndex * cellSize;
            float max = min + cellSize;

            // 그리드 축 상에서 타겟과 가장 가까운 지점 찾기
            float closest = Mathf.Clamp(targetPos, min, max);

            // 타겟과 가장 가까운 지점 사이의 거리 차이
            float delta = closest - targetPos;
            return delta * delta;
        }
    }
}
