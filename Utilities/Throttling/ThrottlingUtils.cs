using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utilities.Throttling
{
    public static class ThrottlingUtils
    {
        private static readonly object GLOBAL_LOCK = new object();

        private static readonly Dictionary<object, object> THROTTLE_GROUP_LOCKS = new Dictionary<object, object>();
        private static readonly Dictionary<object, Task> EXECUTING_TASK_GROUPS = new Dictionary<object, Task>();

        public static async Task Throttle(ThrottleGroup throttleGroup, Func<Task> action, Task postProcess = null)
        {
            object groupLock;
            lock (GLOBAL_LOCK)
            {
                if (!THROTTLE_GROUP_LOCKS.ContainsKey(throttleGroup))
                    THROTTLE_GROUP_LOCKS[throttleGroup] = new object();
                groupLock = THROTTLE_GROUP_LOCKS[throttleGroup];
            }

            Task nextTask;
            lock (groupLock)
            {
                if (!EXECUTING_TASK_GROUPS.ContainsKey(throttleGroup))
                    EXECUTING_TASK_GROUPS[throttleGroup] = Task.CompletedTask;

                var executingTask = EXECUTING_TASK_GROUPS[throttleGroup];
                if (executingTask.IsFaulted)
                    executingTask = Task.CompletedTask;

                nextTask = ProcessInternal(executingTask, action);
                EXECUTING_TASK_GROUPS[throttleGroup] = PostProcess(nextTask, postProcess ?? Task.CompletedTask);
            }

            await nextTask;
        }

        public static async Task<T> Throttle<T>(ThrottleGroup throttleGroup, Func<Task<T>> action, Func<T, Task> postProcess = null)
        {
            object groupLock;
            lock (GLOBAL_LOCK)
            {
                if (!THROTTLE_GROUP_LOCKS.ContainsKey(throttleGroup))
                    THROTTLE_GROUP_LOCKS[throttleGroup] = new object();
                groupLock = THROTTLE_GROUP_LOCKS[throttleGroup];
            }

            Task<T> nextTask;
            lock (groupLock)
            {
                if (!EXECUTING_TASK_GROUPS.ContainsKey(throttleGroup))
                    EXECUTING_TASK_GROUPS[throttleGroup] = Task.CompletedTask;

                var executingTask = EXECUTING_TASK_GROUPS[throttleGroup];
                if (executingTask.IsFaulted)
                    executingTask = Task.CompletedTask;

                nextTask = ProcessInternal(executingTask, action);
                EXECUTING_TASK_GROUPS[throttleGroup] = PostProcess(nextTask, postProcess ?? (_ => Task.CompletedTask));
            }

            return await nextTask;
        }

        private static async Task<T> ProcessInternal<T>(Task previousTask, Func<Task<T>> action)
        {
            await previousTask;
            return await action();
        }

        private static async Task ProcessInternal(Task previousTask, Func<Task> action)
        {
            await previousTask;
            await action();
        }

        private static async Task PostProcess(Task executingTask, Task postProcess)
        {
            await executingTask;
            await postProcess;
        }

        private static async Task PostProcess<T>(Task<T> executingTask, Func<T, Task> postProcess)
        {
            var result = await executingTask;
            await postProcess(result);
        }
    }

    public class ThrottleGroup
    {
        private readonly string _name;

        public ThrottleGroup(string name)
        {
            _name = name;
        }
        
        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ThrottleGroup otherThrottle))
                return false;

            return _name.Equals(otherThrottle._name);
        }
    }
}
