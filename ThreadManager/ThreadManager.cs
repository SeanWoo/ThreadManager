using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadManagerEngine
{
    public class ThreadManager
    {
        private int _sleep;
        private Thread _listen;
        private List<WorkUnit> _actions = new List<WorkUnit>();
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private int _countThreads = 100;

        public int Frequency { get; set; } = 1;
        public int CountThreads
        {
            get => _countThreads;
            set
            {
                _countThreads = value;
                ThreadPool.SetMinThreads(_countThreads, _countThreads);
                ThreadPool.SetMaxThreads(_countThreads, _countThreads);
            }
        }
        public bool Completed { get; private set; }
        public int CountActiveThreads
        {
            get
            {
                return -1;
            }
        }

        /// <summary>
        /// Base constructor. Sets the variables CountThread, Frequency to 100, 1 respectively;
        /// </summary>
        public ThreadManager()
        {
            CountThreads = 100;
        }
        /// <summary>
        /// Base constructor. Sets the Frequency variable to 1;
        /// </summary>
        /// <param name="countThreads">Sets the CountThreads variable.</param>
        public ThreadManager(int countThreads)
        {
            CountThreads = countThreads;
        }
        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="countThreads">Sets the CountThreads variable.</param>
        /// <param name="frequency">Sets the Frequency variable.</param>
        public ThreadManager(int countThreads, int frequency)
        {
            CountThreads = countThreads;
            Frequency = frequency;
        }

        private void Listen()
        {
            Completed = false;
            cancellationToken = new CancellationTokenSource();
            _listen = new Thread(() =>
            {
                while (!cancellationToken.IsCancellationRequested && _actions.Count > 0)
                {
                    DateTime time = DateTime.Now;
                    int countCompeleted = 0;
                    for (int i = 0; i < _actions.Count; i++)
                    {
                        var action = _actions[i];
                        if (action.Completed) countCompeleted++;

                        if (((time >= action.EndTime && action.Loop) || (!action.Completed && !action.Loop)) && !action.IsWork && CountThreads > CountActiveThreads)
                        {
                            action.IsWork = true;
                            ThreadPool.QueueUserWorkItem(o =>
                            {
                                WorkUnit unit = (WorkUnit)o;
                                unit.Action.Invoke(unit);
                                unit.IsWork = false;
                                unit.Completed = !unit.Loop;
                            }, action);
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
                    Completed = countCompeleted == _actions.Count;
                    Thread.Sleep(_sleep);
                    _sleep = int.MaxValue;
                    Thread.Sleep(1000 / Frequency);
                }
            });
            _listen.IsBackground = true;
            _listen.Start();
        }

        /// <summary>
        /// Clear all tasks.
        /// </summary>
        public void Clear() => _actions.Clear();
        /// <summary>
        /// Start Thread Manager. 
        /// </summary>
        public async void Start() => await Task.Run(() => Listen());
        /// <summary>
        /// Stop Thread Manager. 
        /// </summary>
        public void Stop()
        {
            Completed = true;
            if (cancellationToken != null && !cancellationToken.IsCancellationRequested)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
            }
            if (_listen != null)
            {
                _listen.Abort();
            }
        }
        /// <summary>
        /// Waiting for work to finish
        /// </summary>
        public void Wait()
        {
            while (!Completed) { Thread.Sleep(100); }
        }

        /// <summary>
        /// Add a new Task.
        /// </summary>
        /// <param name="action">Delegate. Task.</param>
        /// <param name="loop">Repeat the task.</param>
        public void AddTask(WorkAction action, bool loop, object arg = null)
        {
            WorkUnit unit = new WorkUnit();
            unit.Action = action;
            unit.Loop = loop;
            unit.Completed = false;
            unit.Argument = arg;
            _actions.Add(unit);
        }
    }
}
