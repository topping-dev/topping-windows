using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    public static class TaskExtension
    {
        public static T Synchronize<T>(this Task<T> task)
        {
            var awaiter = task.GetAwaiter();
            while (!awaiter.IsCompleted) Task.Delay(1);
            if (task.IsFaulted)
                return default(T);
            return task.Result;
        }

        public static void Synchronize(this Task task)
        {
            var awaiter = task.GetAwaiter();
            while (!awaiter.IsCompleted) Task.Delay(1);
        }
    }
}
