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
        private Random rand = new Random();

        public DummyData(MapGilTracker plugin) {
            this.plugin = plugin;
        }

        public void Fill()
        {
            // Ensure we're logged in
            if (!plugin.isLoggedIn)
            {
                Services.Log.Error("Not Logged in!");
                return;
            }

            // Get current player name
            var playerName = Services.ClientState.LocalPlayer!.Name.ToString();
            string[] players = { playerName, "TestPlayer1 FFXIV", "TestPlayer2 FFXIV", "TestPlayer3 FFXIV" }; 

            // Randomly generate 10 FATEs
            for (int f = 0; f < 10; f++)
            {
                int reward = rand.Next(2, 21) * 500;
                int playerCount = rand.Next(1, players.Length);
                var ts = DateTime.Now - TimeSpan.FromMinutes(10-f);
                for (int i = 0; i < playerCount; i++)
                {
                    plugin.rewardTracker.AddRecord(reward, players[i], ts);
                }
            }
        }
    }
}
