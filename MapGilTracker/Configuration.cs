using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using MapGilTracker.Models;

namespace MapGilTracker
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool isTracking { get; set; } = false;

        public List<RewardRecord> rewardList { get; set; } = new List<RewardRecord>();

        public void Save()
        {
            Services.Plugin!.SavePluginConfig(this);
        }

        public void ToggleIsTracking()
        {
            isTracking = !isTracking;
            var statusText = isTracking ? "Enabled" : "Disabled";
            Services.Chat.Print($"GilTracker Status is now: {statusText}");
            Save();
        }
    }
}
