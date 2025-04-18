using System.Collections.Generic;

namespace Breadcrumbs.Player {
    public class InputBuffer {
        private readonly Queue<InputData> _inputQueue = new Queue<InputData>();

        public void EnqueueInput(InputData input) {
            _inputQueue.Enqueue(input);
        }

        public InputData? DequeueInput() {
            if (_inputQueue.Count > 0) {
                return _inputQueue.Dequeue();
            }

            return null;
        }

        public InputData? PeekInput() {
            if (_inputQueue.Count > 0) {
                return _inputQueue.Peek();
            }

            return null;
        }

        public bool IsEmpty => _inputQueue.Count == 0;
    }
}