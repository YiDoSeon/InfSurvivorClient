using Shared.Packet;
using UnityEngine;

namespace InfSurvivor.Runtime.Utils
{
    public static class Extensions
    {
        #region GridPos
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
        #endregion

        #region Unity Vector <-> CVector            
        public static Vector3 ToUnityVector3(this CVector2 vector)
        {
            return new Vector3(vector.x, vector.y, 0f);
        }
        public static Vector2 ToUnityVector2(this CVector2 vector)
        {
            return new Vector2(vector.x, vector.y);
        }
        public static CVector2 ToCVector2(this Vector3 vector)
        {
            return new CVector2(vector.x, vector.y);
        }

        public static CVector2 ToCVector2(this Vector2 vector)
        {
            return new CVector2(vector.x, vector.y);
        }
        #endregion
    }
}
