using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskPlugin
{
   public interface IOperation
    {
       DateTime LatestCompletingTime { get; set; }
       TimeSpan SlidingExpirationTime { get; set; }
       OperationState OperationState { get; set; }
       long ExecutionTime { get; set; }
       int ResultCount { get; set; }
       void Execute();
       bool CheckUpdateTime();
       void Abort();

    }
}
