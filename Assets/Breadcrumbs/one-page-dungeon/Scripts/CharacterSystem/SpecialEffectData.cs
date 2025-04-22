using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [System.Serializable]
    public class SpecialEffectData {
        public string effectName;
        public string effectDescription;
        public float activationChance;
        public GameObject visualEffect;
        public AudioClip soundEffect;
    }
}