namespace Breadcrumbs.Utils {
    public class CircularBuffer<T> where T : struct {
        private readonly T[] _buffer;
        private int _head; // Write position
        private int _tail; // Read position
        private int _count;
        private readonly int _capacity;

        public CircularBuffer(int capacity) {
            this._capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        public bool Enqueue(T item) {
            if (_count >= _capacity) return false; // Buffer full
            _buffer[_head] = item;
            _head = (_head + 1) % _capacity;
            _count++;
            return true;
        }

        public bool TryDequeue(out T item) {
            if (_count == 0) {
                item = default;
                return false;
            }

            item = _buffer[_tail];
            _tail = (_tail + 1) % _capacity;
            _count--;
            return true;
        }

        public int Count => _count;
    }
}
