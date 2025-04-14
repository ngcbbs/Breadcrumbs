using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    public interface IDamageable {
        void OnDamage(int damage, Vector3 hitDirection);
    }
}