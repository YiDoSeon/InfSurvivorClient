using UnityEngine;

namespace InfSurvivor.Runtime.Utils
{
    public static class Extensions
    {
        public static Vector2Int ToGridPos(this Vector3 worldPos, float cellSize)
        {
            return new Vector2Int
            (
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.y / cellSize)
            );
        }
        public static Vector2Int ToGridPos(this Vector2 worldPos, float cellSize)
        {
            return new Vector2Int
            (
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.y / cellSize)
            );
        }
    }
}
