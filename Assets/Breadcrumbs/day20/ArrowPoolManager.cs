using Breadcrumbs.Common;

namespace Breadcrumbs.day20 {
    public class ArrowPoolManager : CommonObjectPool<Arrow> {
        private static ArrowPoolManager _instance;
        public static ArrowPoolManager Instance => _instance;

        protected override void Awake() {
            base.Awake();
            _instance = this;
        }

        private void OnDestroy() {
            Clear();
        }
    }
}
