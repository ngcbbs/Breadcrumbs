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
            
            var unit = GetComponent<Unit>();
            if (unit != null) {
                // auto release..
                ObjectPoolManager.Instance.Release(unit);
            }
        }
    }
}
