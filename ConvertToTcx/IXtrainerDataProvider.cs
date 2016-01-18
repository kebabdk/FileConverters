using System;
using System.Collections.Generic;

namespace ConvertToTcx
{
    public interface IXtrainerDataProvider
    {
        DateTime StartTime { get; }
        IEnumerable<XtrainerDataLine> DataLines { get; }
    }
}
