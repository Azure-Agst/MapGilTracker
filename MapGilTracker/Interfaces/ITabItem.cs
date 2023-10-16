using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGilTracker.Interfaces
{
    public interface ITabItem
    {
        string TabName { get; }
        bool Enabled { get; }
        void Draw();
    }
}
