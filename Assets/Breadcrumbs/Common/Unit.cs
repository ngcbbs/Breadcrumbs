using UnityEngine;

namespace Breadcrumbs.Common {
    public class Unit : MonoBehaviour {
        public delegate void UnitDiedHandler(Unit unit);

        public event UnitDiedHandler OnUnitDied;

        public void OnUnitDiedInvoke() {
            OnUnitDied?.Invoke(this);
        }
    }
}
