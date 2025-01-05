using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable 1591
namespace Nistec.Caching
{
    public interface ISyncronizer
    {
        void Refresh(string name);
    }
}