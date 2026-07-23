using System.Collections.Generic;

namespace BehaviorTreePro.Performance
{
    using BehaviorTree;
    public class NodePool<T> where T : NodeBT, new()
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly int _maxSize;
        private readonly object _lock = new object();

        public NodePool(int maxSize = 32)
        {
            _maxSize = maxSize;
        }

        public T Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var node = _pool.Pop();
                    node.Reset();
                    return node;
                }
            }

            return new T();
        }

        public void Return(T node)
        {
            if (node == null)
                return;

            node.Reset();

            lock (_lock)
            {
                if (_pool.Count < _maxSize)
                {
                    _pool.Push(node);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }
    }
}
