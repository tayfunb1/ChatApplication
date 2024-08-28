using System.Collections.Concurrent;

namespace ChatApplication.DataAccess.Utility;

public class LimitedConcurrentQueue<T>
{
    private readonly ConcurrentQueue<T> _queue = new();

    public bool TryEnqueue(T item, int maxLength)
    {
        if (_queue.Count >= maxLength)
        {
            return false;
        }

        _queue.Enqueue(item);
        return true;
    }
    
    public bool DequeueOnCondition(out T result, Func<T, bool> condition)
    {
        if (_queue.TryDequeue(out result))
        {
            if (condition(result))
            {
                return true;
            }

            _queue.Enqueue(result);
            result = default;
            return false;
        }

        result = default;
        return false;
    }

    public IReadOnlyCollection<T> GetQueueData(Func<T, bool> predicate = null)
    {
        return predicate == null ? _queue.ToList() : _queue.Where(predicate).ToList();
    }
}