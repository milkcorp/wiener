using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Threading;

namespace uConstruct.Core.Threading
{

    /// <summary>
    /// This class handles the multi-threading mechanics of uConstruct.
    /// </summary>
    public static class ThreadManager
    {
        /// <summary>
        /// Will the system multi-thread calculations in order to remove overload from main thread?
        /// </summary>
        static bool _enabled = true;
        public static bool enabled
        {
            get { return _enabled && UCSettings.instance.UCThreadingEnabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// Queued thread actions
        /// </summary>
        static List<IThreadTask> UConstructThreadActions = new List<IThreadTask>();
        /// <summary>
        /// List of all queued unity thread actions
        /// </summary>
        static List<IThreadTask> UnityThreadQueuedActions = new List<IThreadTask>();

        #if !UNITY_WEBGL
        /// <summary>
        /// Our thread instance.
        /// </summary>
        static Thread thread = new Thread(RunThread);
        #endif

        /// <summary>
        /// should the thread run
        /// </summary>
        static bool isRunning = false;

        /// <summary>
        /// should the thread update
        /// </summary>
        static bool isUpdate;

        /// <summary>
        /// Run Thread
        /// </summary>
        static void RunThread()
        {
            while (isRunning) // run our thread
            {
                if (isUpdate)
                {
                    isUpdate = false;

                    IThreadTask queuedData;

                    for (int i = UConstructThreadActions.Count - 1; i >= 0; i--) // run queue
                    {
                        queuedData = UConstructThreadActions[i];

                        queuedData.Invoke();

                        UConstructThreadActions.RemoveAt(i);
                    }
                }

                Thread.Sleep(60);
            }
        }
        /// <summary>
        /// Start our thread
        /// </summary>
        public static void StartThread()
        {
            #if !UNITY_WEBGL

            if(!thread.IsAlive)
            {
                isRunning = true;
                thread.Start();
            }

            #endif
        }
        /// <summary>
        /// Stop the thread
        /// </summary>
        public static void StopThread()
        {
            #if !UNITY_WEBGL

            isRunning = false;
            thread.Abort();

            #endif
        }
        /// <summary>
        /// Update thread
        /// </summary>
        public static void UpdateThread()
        {
            isUpdate = true;
        }

        /// <summary>
        /// Access to unity thread
        /// </summary>
        public static void UnityThread()
        {
            UpdateThread();

            IThreadTask queuedData;

            for (int i = 0; i < UnityThreadQueuedActions.Count; )// run queue
            {
                queuedData = UnityThreadQueuedActions[i];

                if (queuedData != null)
                {
                    queuedData.Invoke();
                }

                UnityThreadQueuedActions.RemoveAt(i);
            }
        }

        /// <summary>
        /// Add an action to the unity thread
        /// </summary>
        /// <param name="action">the action</param>
        public static void RunOnUnityThread(IThreadTask action)
        {
            UnityThreadQueuedActions.Add(action);
        }

        /// <summary>
        /// Add an action to the uConstruct thread
        /// </summary>
        /// <param name="action">the action</param>
        public static void RunOnUConstructThread(IThreadTask action)
        {
            if (enabled)
                UConstructThreadActions.Add(action);
            else
                RunOnUnityThread(action);
        }
    }

    #region Tasks
    /// <summary>
    /// A thread task that takes no parameters.
    /// </summary>
    public class ThreadTask : IThreadTask
    {
        System.Action action;

        public ThreadTask(System.Action _action)
        {
            action = _action;
        }

        public void Invoke()
        {
            action();
        }
    }
    /// <summary>
    /// A thread task that takes 1 parameter.
    /// <typeparam name="T">Type 1</typeparam>
    /// </summary>
    public class ThreadTask<T> : IThreadTask
    {
        System.Action<T> action;
        T data;

        public ThreadTask(System.Action<T> _action, T _data)
        {
            action = _action;
            data = _data;
        }

        public void Invoke()
        {
            action(data);
        }
    }
    /// <summary>
    /// A thread task that takes 2 parameters.
    /// <typeparam name="T">Type 1</typeparam>
    /// <typeparam name="T1">Type 2</typeparam>
    /// </summary>
    public class ThreadTask<T, T1> : IThreadTask
    {
        System.Action<T, T1> action;
        T data1;
        T1 data2;

        public ThreadTask(System.Action<T, T1> _action, T _data1, T1 _data2)
        {
            action = _action;
            data1 = _data1;
            data2 = _data2;
        }

        public void Invoke()
        {
            action(data1, data2);
        }
    }
    /// <summary>
    /// A thread task that takes 3 parameters.
    /// <typeparam name="T">Type 1</typeparam>
    /// <typeparam name="T1">Type 2</typeparam>
    /// <typeparam name="T2">Type 3</typeparam>
    /// </summary>
    public class ThreadTask<T, T1, T2> : IThreadTask
    {
        System.Action<T, T1, T2> action;
        T data1;
        T1 data2;
        T2 data3;

        public ThreadTask(System.Action<T, T1, T2> _action, T _data1, T1 _data2, T2 _data3)
        {
            action = _action;
            data1 = _data1;
            data2 = _data2;
            data3 = _data3;
        }

        public void Invoke()
        {
            action(data1, data2, data3);
        }
    }
    /// <summary>
    /// A thread task interface.
    /// Implement on any customely created thread task.
    /// </summary>
    public interface IThreadTask
    {
        void Invoke();
    }
    #endregion

}