using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AsyncQueue<T>
{
    private readonly Queue<T> _queue = new Queue<T>();
    private readonly Queue<TaskCompletionSource<T>> _waitingTasks = new Queue<TaskCompletionSource<T>>();

    // Метод для додавання елемента в чергу
    public void Enqueue(T item)
    {
        // Якщо є очікуючі задачі, видаємо елемент через TaskCompletionSource
        if (_waitingTasks.Count > 0)
        {
            var waitingTask = _waitingTasks.Dequeue();
            waitingTask.SetResult(item);
        }
        else
        {
            // Інакше додаємо елемент у чергу
            _queue.Enqueue(item);
        }
    }

    // Асинхронний метод для вилучення елемента з черги
    public async Task<T> DequeueAsync()
    {
        // Якщо в черзі є елементи, одразу видаємо
        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }

        // Якщо черга порожня, створюємо TaskCompletionSource і чекаємо, поки з'явиться елемент
        var tcs = new TaskCompletionSource<T>();
        _waitingTasks.Enqueue(tcs);
        return await tcs.Task;
    }
}

class Program
{
    static async Task Producer(AsyncQueue<int> queue)
    {
        // Імітація додавання елементів з певною затримкою
        for (int i = 1; i <= 5; i++)
        {
            await Task.Delay(1000); // Затримка 1 секунда
            queue.Enqueue(i);
            Console.WriteLine($"Producer: Додав елемент {i}");
        }
    }

    static async Task Consumer(AsyncQueue<int> queue)
    {
        // Імітація споживання елементів
        for (int i = 1; i <= 5; i++)
        {
            var item = await queue.DequeueAsync();
            Console.WriteLine($"Consumer: Витягнув елемент {item}");
        }
    }

    static async Task Main(string[] args)
    {
        var queue = new AsyncQueue<int>();

        // Запускаємо виробника і споживача одночасно
        var producerTask = Producer(queue);
        var consumerTask = Consumer(queue);

        // Чекаємо завершення обох задач
        await Task.WhenAll(producerTask, consumerTask);

        Console.WriteLine("Всі елементи оброблені.");
    }
}
