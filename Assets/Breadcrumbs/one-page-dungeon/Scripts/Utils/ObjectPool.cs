using System.Collections.Generic;

namespace Breadcrumbs.Utils {
    public class ObjectPool<T> where T : class, new() {
        private readonly Queue<T> _pool = new Queue<T>();

        public ObjectPool(int initialSize) {
            for (var i = 0; i < initialSize; i++) {
                _pool.Enqueue(new T());
            }
        }

        public T Get() => _pool.Count > 0 ? _pool.Dequeue() : new T();
        public void Return(T item) => _pool.Enqueue(item);
    }
}
