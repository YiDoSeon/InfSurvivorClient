using System.Collections.Generic;
using InfSurvivor.Runtime.GameSystem;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using Shared.Packet.Struct;
using Shared.Physics.Collider;
using Shared.Utils;

namespace InfSurvivor.Runtime.Manager
{
    public class CollisionManager
    {
        private GridSystem gridSystem;
        public float CellSize => gridSystem.CellSize;
        private List<ColliderBase> allColliders = new List<ColliderBase>();
        private Dictionary<IColliderTrigger, ColliderBase> colliderMap = new Dictionary<IColliderTrigger, ColliderBase>();
        private HashSet<IColliderTrigger> currentTickOverlaps = new HashSet<IColliderTrigger>();
        private List<IColliderTrigger> removalCache = new List<IColliderTrigger>();
        private Dictionary<CollisionLayer, int> collisionMatrix = new Dictionary<CollisionLayer, int>();

        public void InitMatrix()
        {
            SetLayerCollision(CollisionLayer.Player, CollisionLayer.Monster, true);
        }

        private void SetLayerCollision(CollisionLayer layerA, CollisionLayer layerB, bool canCollide)
        {
            if (collisionMatrix.ContainsKey(layerA) == false)
            {
                collisionMatrix[layerA] = 0;
            }
            if (collisionMatrix.ContainsKey(layerB) == false)
            {
                collisionMatrix[layerB] = 0;
            }

            if (canCollide)
            {
                collisionMatrix[layerA] |= (1 << (int)layerB);
                collisionMatrix[layerB] |= (1 << (int)layerA);
            }
            else
            {
                collisionMatrix[layerA] &= ~(1 << (int)layerB);
                collisionMatrix[layerB] &= ~(1 << (int)layerA);
            }
        }

        public bool ShouldCollide(CollisionLayer a, CollisionLayer b)
        {
            if (collisionMatrix.TryGetValue(a, out int mask))
            {
                return (mask & (1 << (int)b)) != 0;
            }
            return false;
        }

        public void SetGridSystem(GridSystem gridVisualizer)
        {
            this.gridSystem = gridVisualizer;
        }

        public void AddToCells(HashSet<CVector2Int> occupiedCells, ColliderBase col)
        {
            gridSystem?.AddToCells(occupiedCells, col);
        }

        public void RemoveFromCells(HashSet<CVector2Int> occupiedCells, ColliderBase col)
        {
            gridSystem?.RemoveFromCells(occupiedCells, col);
        }

        public void RegisterCollider(ColliderBase collider)
        {
            if (collider.Owner == null)
            {
                return;
            }
            allColliders.Add(collider);
            colliderMap[collider.Owner] = collider;
        }

        public void UnregisterCollider(ColliderBase collider)
        {
            if (collider.Owner == null)
            {
                return;
            }
            allColliders.Remove(collider);
            colliderMap.Remove(collider.Owner);
        }

        public void UpdateOccupiedCells(ColliderBase col, HashSet<CVector2Int> occupiedCells)
        {
            CVector2 worldPos = col.Center;
            CVector2 halfSize = col.HalfSize; 

            CVector2 minWorldPos = new CVector2(worldPos.x - halfSize.x, worldPos.y - halfSize.y);
            CVector2 maxWorldPos = new CVector2(worldPos.x + halfSize.x, worldPos.y + halfSize.y);

            CVector2Int minGridPos = minWorldPos.ToGridPos(CellSize);
            CVector2Int maxGridPos = maxWorldPos.ToGridPos(CellSize);

            RemoveFromCells(occupiedCells, col);
            occupiedCells.Clear();

            for (int x = minGridPos.x; x <= maxGridPos.x; x++)
            {
                for (int y = minGridPos.y; y <= maxGridPos.y; y++)
                {
                    CVector2Int coord = new CVector2Int(x, y);
                    occupiedCells.Add(coord);
                }
            }

            AddToCells(occupiedCells, col);
        }

        public IEnumerable<ColliderBase> GetColliderInRange(ColliderBase searcher, bool alsoCheckObjectBox = true)
        {
            float targetX = searcher.Center.x;
            float targetY = searcher.Center.y;
            float halfSizeX = searcher.HalfSize.x;
            float halfSizeY = searcher.HalfSize.y;

            float sqrRange = float.MaxValue;
            if (searcher is CCircleCollider circle)
            {
                sqrRange = circle.Radius * circle.Radius;
            }

            int minX = CMath.FloorToInt((targetX - halfSizeX) / CellSize);
            int maxX = CMath.FloorToInt((targetX + halfSizeX) / CellSize);
            int minY = CMath.FloorToInt((targetY - halfSizeY) / CellSize);
            int maxY = CMath.FloorToInt((targetY + halfSizeY) / CellSize);

            for (int x = minX; x <= maxX; x++)
            {
                float dSqrX = GetMinSqrDistanceToCell(x, targetX, CellSize);

                for (int y = minY; y <= maxY; y++)
                {
                    // 범위 내 grid pos
                    CVector2Int key = new CVector2Int(x, y);

                    if (gridSystem.GridData.TryGetValue(key, out HashSet<ColliderBase> set) == false)
                    {
                        // 아무것도 없음
                        continue;
                    }

                    float dSqrY = GetMinSqrDistanceToCell(y, targetY, CellSize);

                    // sqrRange가 MaxValue가 아닌경우 즉, CircleCollider일 때만 원의 범위안에 Grid가 포함되어있는지 체크
                    if (dSqrX + dSqrY > sqrRange)
                    {
                        // 범위 밖에 있음
                        continue;
                    }

                    foreach (ColliderBase col in set)
                    {
                        if (col == searcher)
                        {
                            continue;
                        }

                        // 레이어상 서로 충돌하는지 체크
                        if (ShouldCollide(searcher.Layer, col.Layer) == false)
                        {
                            continue;
                        }

                        if (alsoCheckObjectBox)
                        {
                            // 콜라이더 비교
                            if (searcher.CheckCollision(col) == false)
                            {
                                continue;
                            }
                        }
                        yield return col;
                    }
                }
            }
        }

        public ColliderBase FindColliderByOwner(IColliderTrigger owner)
        {
            return colliderMap.GetValueOrDefault(owner);
        }

        public void OnTick()
        {
            foreach (ColliderBase colA in allColliders)
            {
                // TODO: Static 콜라이더추가 및 가만히 있어 스스로 탐색하지 않도록 한다.
                IColliderTrigger ownerA = colA.Owner;
                if (ownerA == null)
                {
                    continue;
                }

                IEnumerable<ColliderBase> candidates = GetColliderInRange(colA, false);
                currentTickOverlaps.Clear();

                foreach (ColliderBase colB in candidates)
                {
                    if (colA.CheckCollision(colB))
                    {
                        IColliderTrigger ownerB = colB.Owner;
                        if (ownerB == null)
                        {
                            continue;
                        }

                        currentTickOverlaps.Add(colB.Owner);

                        if (colA.OverlappingIds.Add(colB.Owner))
                        {
                            colA.Owner?.OnCustomTriggerEnter(colB);
                        }
                        else
                        {
                            colA.Owner?.OnCustomTriggerStay(colB);
                        }
                    }
                }

                removalCache.Clear();

                foreach (IColliderTrigger oldOwner in colA.OverlappingIds)
                {
                    if (currentTickOverlaps.Contains(oldOwner) == false)
                    {
                        removalCache.Add(oldOwner);
                    }
                }

                foreach (IColliderTrigger toRemove in removalCache)
                {
                    colA.OverlappingIds.Remove(toRemove);
                    ColliderBase other = FindColliderByOwner(toRemove);
                    if (other != null)
                    {
                        ownerA.OnCustomTriggerExit(other);
                    }
                }
            }
        }

        private float GetMinSqrDistanceToCell(float gridIndex, float targetPos, float cellSize)
        {
            // 그리드 축의 최소/최대 월드 좌표 계산
            float min = gridIndex * cellSize;
            float max = min + cellSize;

            // 그리드 축 상에서 타겟과 가장 가까운 지점 찾기
            float closest = CMath.Clamp(targetPos, min, max);

            // 타겟과 가장 가까운 지점 사이의 거리 차이
            float delta = closest - targetPos;
            return delta * delta;
        }
    }
}
