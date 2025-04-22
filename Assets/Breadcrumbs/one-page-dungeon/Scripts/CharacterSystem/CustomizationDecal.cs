using System;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class CustomizationDecal {
        public enum DecalType {
            Tattoo,
            Scar,
            Makeup,
            Warpaint
        }

        public DecalType type;
        public int decalId;
        public Color decalColor = Color.black;
        public Vector2 position;
        public float scale = 1f;
        public float rotation = 0f;
        public float opacity = 1f;
    }
}