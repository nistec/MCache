using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Caching
{
    public interface ISyncronizer
    {
        void Refresh(string name);
    }
}