using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Breadcrumbs.Common {
    public class AutoDeath : MonoBehaviour {
        [SerializeField] private float interval = 1f;
        private void Start() {
            DoAutoDeath().Forget();
        }

        private async UniTaskVoid DoAutoDeath() {
            await UniTask.WaitForSeconds(interval);
            
            // :(
            var unit = GetComponentInParent<Unit>();
            if (unit != null) {
                unit.OnUnitDiedInvoke();
            }
            
            Destroy(gameObject);
        }
    }
}
