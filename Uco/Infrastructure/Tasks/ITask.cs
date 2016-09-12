using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uco.Infrastructure.Tasks
{
    public interface ITask
    {
        void Execute();
        string Title { get; }
        int StartSeconds { get; }
        int IntervalSecondsFrom { get; }
        int IntervalSecondsTo { get; }
    }
}