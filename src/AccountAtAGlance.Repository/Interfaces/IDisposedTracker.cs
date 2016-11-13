using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AccountAtAGlance.Repository.Interfaces
{
    public interface IDisposedTracker
    {
        bool IsDisposed { get; set; }
    }
}
