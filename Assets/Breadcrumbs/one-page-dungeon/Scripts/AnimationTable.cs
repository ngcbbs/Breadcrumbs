using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    [CreateAssetMenu(fileName = "AnimationTable", menuName = "Breadcrumbs/Tools/Create AnimationTable")]
    public class AnimationTable : ScriptableObject {
        public string enemyType;
        public AnimationClip idleClip;
        public AnimationClip walkClip;
        public AnimationClip runClip;
        public AnimationClip attackClip;
        public AnimationClip deathClip;
    }
}
