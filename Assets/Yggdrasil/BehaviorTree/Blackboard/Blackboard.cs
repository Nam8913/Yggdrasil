using System;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class Blackboard
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
        private readonly Dictionary<string, List<Delegate>> _observers = new Dictionary<string, List<Delegate>>();
        private readonly object _lock = new object();

        public IEnumerable<string> GetAllKeys()
        {
            lock (_lock)
            {
                return new List<string>(_data.Keys);
            }
        }

        public T Get<T>(BBKey<T> key)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key.Name, out var value))
                    return (T)value;
            }

            throw new KeyNotFoundException($"Blackboard key '{key.Name}' not found.");
        }

        public bool TryGet<T>(BBKey<T> key, out T value)
        {
            lock (_lock)
            {
                if (_data.TryGetValue(key.Name, out var obj))
                {
                    try
                    {
                        value = (T)obj;
                    }catch (InvalidCastException)
                    {
                        value = default;
                        UnityEngine.Debug.LogError($"Blackboard key '{key.Name}' has a value of type '{obj.GetType().Name}', which cannot be cast to '{typeof(T).Name}'.");
                        return false;
                    }
                    
                    return true;
                }
            }

            value = default;
            return false;
        }

        public void Set<T>(BBKey<T> key, T value)
        {
            List<Delegate> snapshot;
            lock (_lock)
            {
                _data[key.Name] = value;
                if (!_observers.TryGetValue(key.Name, out var list))
                    snapshot = null;
                else
                    snapshot = new List<Delegate>(list);
            }

            // Notify outside lock to avoid deadlocks
            if (snapshot != null)
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if (snapshot[i] is Action<T> callback)
                        callback.Invoke(value);
                }
            }
        }

        public void Remove<T>(BBKey<T> key)
        {
            lock (_lock)
            {
                _data.Remove(key.Name);
            }
        }

        public bool Has<T>(BBKey<T> key)
        {
            lock (_lock)
            {
                return _data.ContainsKey(key.Name);
            }
        }

        public void Subscribe<T>(BBKey<T> key, Action<T> callback)
        {
            lock (_lock)
            {
                if (!_observers.ContainsKey(key.Name))
                    _observers[key.Name] = new List<Delegate>();

                _observers[key.Name].Add(callback);
            }
        }

        public void Unsubscribe<T>(BBKey<T> key, Action<T> callback)
        {
            lock (_lock)
            {
                if (_observers.TryGetValue(key.Name, out var list))
                    list.Remove(callback);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _data.Clear();
            }
        }
    }
}
