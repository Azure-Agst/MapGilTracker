using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using MapGilTracker.Interfaces;
using MapGilTracker.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MapGilTracker.Windows.Tabs
{
    public class ReportTab: ITabItem
    {
        public string TabName => "Summary";
        public bool Enabled => true;

        private const string SelectPlayer = "Select Player";

        private MapGilTracker plugin;
        private RewardRecordKeeper recordKeeper;
        private string? curName = null;
        private int taxRate = 50;

        public ReportTab(MainWindow mainWindow) {
            plugin = mainWindow.plugin;
            recordKeeper = plugin.rewardTracker;
        }

        public void Draw()
        {
            // Get dimensions
            var windowSize = ImGui.GetContentRegionAvail();

            // Label
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Player:");
            ImGui.SameLine();

            // Draw combobox
            ImGui.PushItemWidth(300);
            if (ImGui.BeginCombo("##NameCombo", curName ?? SelectPlayer))
            {
                // If no player selected...
                if (ImGui.Selectable(SelectPlayer, curName == null))
                    curName = null;

                // Add each player
                foreach (string player in recordKeeper.userTable.Keys)
                {
                    if (ImGui.Selectable(player, curName == player))
                        curName = player;
                }

                ImGui.EndCombo();
            }

            // Tax Rate
            ImGui.SameLine(windowSize.X - 150);
            ImGui.PushItemWidth(100);
            ImGui.InputInt("Tax Rate", ref this.taxRate);

            // Separator
            ImGui.Separator();

            // If no user selected, display text
            if (curName == null)
            {
                ImGui.Text("Select a user above to see their report!");
                return;
            }

            // Aggregate all rewards
            var playerRewards = plugin.rewardTracker.rewardList
                .Where(e => e.player == curName);
            ImGui.Text($"Total Reward count: {playerRewards.Count()}");

            // Sum up value
            var totalGil = playerRewards
                .Select(e => e.value)
                .Sum();
            ImGui.Text($"Total gil: {totalGil}g");

            // Spacer
            ImGui.Text($"---");

            // Print Tax Rate
            ImGui.Text($"Tax Rate: {taxRate}%%");


            // Determine Taxes
            double taxPct = taxRate / 100d;
            
            var taxAmt = (int)Math.Floor(totalGil * taxPct);
            ImGui.Text($"Amount owed: {taxAmt}g");
            ImGui.Text($"Takehome: {totalGil - taxAmt}g");
        }
    }
}
