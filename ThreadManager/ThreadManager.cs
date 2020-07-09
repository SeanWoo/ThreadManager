using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadManagerEngine
{
    public class ThreadManager
    {
        private int _sleep;
        private Thread _listen;
        private Thread _delete;
        private Thread[] _threads;
        private List<WorkUnit> _actions = new List<WorkUnit>();
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private bool _isClearActions = false;

        public int CountThreads { get; }
        public bool Completed { get => CountActiveThreads == 0; }
        public int CountActiveThreads
        {
            get {
                int count = 0;
                for (int i = 0; i < _threads.Length; i++) 
                    if (_threads[i] != null) count++;
                return count;
            }
        }

        public ThreadManager()
        {
            CountThreads = 100;
            _threads = new Thread[CountThreads];
        }
        public ThreadManager(int countThreads)
        {
            CountThreads = countThreads;
            _threads = new Thread[CountThreads];
        }

        private void Listen()
        {
            _listen = new Thread(() =>
            {
                while (!cancellationToken.IsCancellationRequested && _actions.Count > 0)
                {
                    DateTime time = DateTime.Now;
                    for (int i = 0; i < _actions.Count; i++)
                    {
                        var action = _actions[i];
                        if (((time >= action.EndTime && action.Loop) || (!action.Completed && !action.Loop)) && !action.IsWork && CountThreads > CountActiveThreads)
                        {
                            action.IsWork = true;
                            Thread thread = new Thread((o) =>
                            {
                                WorkUnit unit = (WorkUnit)o;
                                unit.Action.Invoke(unit);
                                unit.IsWork = false;
                                unit.Completed = !unit.Loop;
                            });
                            thread.IsBackground = true;
                            thread.Start(action);

                            int threadLength = _threads.Length;
                            for (int j = 0; j < threadLength; j++)
                            {
                                if (_threads[j] == null) 
                                {
                                    _threads[j] = thread;
                                    break;
                                }
                            }
                        }
                        int subTime = (int)(action.EndTime - time).TotalMilliseconds;
                        if (subTime < _sleep)
                        {
                            if (subTime < 0)
                            {
                                _sleep = 0;
                            }
                            else
                            {
                                _sleep = subTime;
                            }
                        }
                    }
                    if (_isClearActions)
                    {
                        _actions.Clear();
                        _isClearActions = false;
                    }
                    Thread.Sleep(_sleep);
                    _sleep = int.MaxValue;
                }
            });
            _delete = new Thread(() =>
            {
                while (!cancellationToken.IsCancellationRequested && _actions.Count > 0)
                {
                    int threadLength = _threads.Length;
                    for (int i = 0; i < threadLength; i++)
                    {
                        var thread = _threads[i];
                        if (thread != null && !thread.IsAlive)
                        {
                            thread.Abort();
                            _threads[i] = null;
                            i--;
                        }
                    }
                    if (CountActiveThreads == 0)
                    {
                        Thread.Sleep(_sleep);
                    }
                }
            });
            _listen.IsBackground = true;
            _delete.IsBackground = true;
            _listen.Start();
            _delete.Start();
        }

        public void Clear() => _isClearActions = true;

        public async void Start() => await Task.Run(() => Listen() );

        public void Stop() => cancellationToken.Cancel();

        public void AddTask(WorkAction action, bool loop)
        {
            WorkUnit unit = new WorkUnit();
            unit.Action = action;
            unit.Loop = loop;
            unit.Completed = false;
            _actions.Add(unit);
        }
    }
}
