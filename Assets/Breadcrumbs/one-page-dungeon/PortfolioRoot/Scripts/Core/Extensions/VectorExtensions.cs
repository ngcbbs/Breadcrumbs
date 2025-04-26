using UnityEngine;

namespace GamePortfolio.Core.Extensions
{
    /// <summary>
    /// Extension methods for Vector2, Vector3, and other vector types
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a Vector3 to a Vector2 by discarding the Z component
        /// </summary>
        public static Vector2 ToVector2XY(this Vector3 v) => new Vector2(v.x, v.y);

        /// <summary>
        /// Converts a Vector3 to a Vector2 using X and Z components (for top-down games)
        /// </summary>
        public static Vector2 ToVector2XZ(this Vector3 v) => new Vector2(v.x, v.z);

        /// <summary>
        /// Converts a Vector2 to a Vector3 with a specified Y component
        /// </summary>
        public static Vector3 ToVector3XZ(this Vector2 v, float y = 0) => new Vector3(v.x, y, v.y);

        /// <summary>
        /// Converts a Vector2Int to a Vector2
        /// </summary>
        public static Vector2 ToVector2(this Vector2Int v) => new Vector2(v.x, v.y);

        /// <summary>
        /// Converts a Vector3Int to a Vector3
        /// </summary>
        public static Vector3 ToVector3(this Vector3Int v) => new Vector3(v.x, v.y, v.z);

        /// <summary>
        /// Sets the X component of a Vector3
        /// </summary>
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);

        /// <summary>
        /// Sets the Y component of a Vector3
        /// </summary>
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);

        /// <summary>
        /// Sets the Z component of a Vector3
        /// </summary>
        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

        /// <summary>
        /// Calculates the flat distance (ignoring Y axis) between two points
        /// </summary>
        public static float FlatDistance(this Vector3 from, Vector3 to)
        {
            Vector2 from2D = new Vector2(from.x, from.z);
            Vector2 to2D = new Vector2(to.x, to.z);
            return Vector2.Distance(from2D, to2D);
        }

        /// <summary>
        /// Returns a random point inside a circle with the given radius
        /// </summary>
        public static Vector2 RandomPointInCircle(this Vector2 center, float radius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0f, radius);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        /// <summary>
        /// Returns a random point inside a circle with the given radius (XZ plane)
        /// </summary>
        public static Vector3 RandomPointInCircleXZ(this Vector3 center, float radius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0f, radius);
            return center + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
        }

        /// <summary>
        /// Check if a point is inside a rectangle
        /// </summary>
        public static bool IsInside(this Vector2 point, Rect rect)
        {
            return point.x >= rect.xMin && point.x <= rect.xMax &&
                   point.y >= rect.yMin && point.y <= rect.yMax;
        }

        /// <summary>
        /// Calculates the direction from one point to another
        /// </summary>
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }

        /// <summary>
        /// Calculates the direction from one point to another on a 2D plane
        /// </summary>
        public static Vector2 DirectionTo(this Vector2 from, Vector2 to)
        {
            return (to - from).normalized;
        }
    }
}
