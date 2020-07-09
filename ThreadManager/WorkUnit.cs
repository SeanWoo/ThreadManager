using System;

namespace ThreadManagerEngine
{
    /// <summary>
    /// Represents an instance of a task.
    /// </summary>
    public class WorkUnit
    {
        public bool IsWork { get; set; } = false;
        public DateTime EndTime { get; set; } = DateTime.Now;
        public WorkAction Action { get; set; }
        public bool Loop { get; set; }
        public bool Completed { get; set; }

        public void Wait(int time)
        {
            EndTime = DateTime.Now.AddMilliseconds(time);
            IsWork = false;
        }
    }
}