using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGilTracker.Models
{
    public class RewardRecord
    {
        public DateTime timestamp { get; set; }
        public int value { get; set; }
        public string? player { get; set; }
    }
}
