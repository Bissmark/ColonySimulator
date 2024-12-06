using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class ThreadedDataRequester : MonoBehaviour {
    static ThreadedDataRequester instance;
    Queue<ThreadInfo> dataThread = new();

    void Awake() {
        instance = FindFirstObjectByType<ThreadedDataRequester>();
    }

    public static void RequestData(Func<object> generateData, Action<object> callback) {
        ThreadStart threadStart = delegate {
            instance.DataThread(generateData, callback);
        };

        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback) {
        object data = generateData();
        lock (dataThread) {
            dataThread.Enqueue(new ThreadInfo(callback, data));
        }
    }

    void Update() {
        if (dataThread.Count > 0) {
            for (int i = 0; i < dataThread.Count; i++) {
                ThreadInfo threadInfo = dataThread.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    struct ThreadInfo {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}