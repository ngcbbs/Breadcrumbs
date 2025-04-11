using System;
using UnityEngine;

namespace Breadcrumbs.one_page_dungeon {
    [Serializable]
    public struct Edge {
        public Vector2Int pos;
        public Wall wall;
    }
}