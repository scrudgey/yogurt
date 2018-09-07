using System.Collections.Generic;

public class FixedSizedQueue<T>: Queue<T> {
    private object lockObject = new object();
    public int Limit;
    public void Add(T obj) {
        Enqueue(obj);
        lock (lockObject) {
            while (Count > Limit) {
                Dequeue();
            }
        }
    }
}