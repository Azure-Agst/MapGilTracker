using Dalamud.Configuration;
using Dalamud.Plugin.Services;
using ImGuiNET;
using MapGilTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGilTracker.Tools
{
    public class DummyData
    {
        private MapGilTracker plugin;

        public DummyData(MapGilTracker plugin) {
            this.plugin = plugin;
        }

        public void Fill()
        {
            // Ensure we're logged in
            if (!plugin.isLoggedIn)
            {
                MapGilTracker.PluginLog.Error("Not Logged in!");
                return;
            }

            // Get current player name
            var playerName = MapGilTracker.ClientState.LocalPlayer!.Name.ToString();

            // Start filling data
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
            plugin.rewardTracker.AddRecord(1000, playerName);
        }
    }
}
