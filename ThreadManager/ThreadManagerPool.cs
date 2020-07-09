using System;
using System.Collections.Generic;
using System.Text;

namespace ThreadManagerEngine
{
    public static class ThreadManagerPool
    {
        private static ThreadManager _manager;

        public static int CountThread { get; set; }
        public static int CountActiveThread { get => _manager.CountActiveThreads; }

        static ThreadManagerPool()
        {
            _manager = new ThreadManager(CountThread);
            _manager.Start();
        }

        public static void AddTask(WorkAction action)
        {
            _manager.AddTask(action, false);
        }
    }
}
