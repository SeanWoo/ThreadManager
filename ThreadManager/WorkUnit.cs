using System;

namespace ThreadManagerEngine
{
    /// <summary>
    /// Represents an instance of a task.
    /// </summary>
    public class WorkUnit
    {
        internal bool IsWork { get; set; } = false;
        internal DateTime EndTime { get; set; } = DateTime.Now;
        internal WorkAction Action { get; set; }
        internal bool Loop { get; set; }
        internal bool Completed { get; set; }
        internal object Argument { get; set; }

        public void Wait(int time)
        {
            EndTime = DateTime.Now.AddMilliseconds(time);
            IsWork = false;
        }

        public T GetArg<T>()
        {
            return (T)Argument;
        }
    }
}