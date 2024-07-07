using System;
using System.Collections.Generic;
namespace Kurisu.Framework.AI
{
    public interface IAIProxy
    {
        /// <summary>
        /// Get current plan to append new task or traverse the sequence
        /// </summary>
        /// <returns></returns>
        SequenceTask GetPlan();
        /// <summary>
        /// Abort plan
        /// </summary>
        void Abort();
    }
    public interface IAIProxy<T> : IAIProxy where T : IAIContext
    {
        /// <summary>
        /// Bind host
        /// </summary>
        /// <value></value>
        AIController<T> Host { get; }
        /// <summary>
        /// Start a proxy plan
        /// </summary>
        /// <param name="host"></param>
        /// <param name="tasks"></param>
        /// <param name="callBack"></param>
        void StartProxy(AIController<T> host, IReadOnlyList<ITask> tasks, Action callBack);
    }
}
