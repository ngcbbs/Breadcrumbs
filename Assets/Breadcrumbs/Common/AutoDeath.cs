using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Breadcrumbs.Common {
    public class AutoDeath : MonoBehaviour {
        [SerializeField] private float interval = 1f;
        private CancellationTokenSource _cts;

        public void Initialize() {
            _cts = new CancellationTokenSource();
            DoAutoDeath(_cts.Token).Forget();
        }

        private void OnDisable() {
            _cts?.Cancel();
        }

        private async UniTaskVoid DoAutoDeath(CancellationToken token) {
            await UniTask.WaitForSeconds(interval, true, 0f, token);
            
            var unit = GetComponent<Unit>();
            if (unit != null) {
                ObjectPoolManager.Instance.Release(unit);
            }
        }
    }
}
