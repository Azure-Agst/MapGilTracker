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
            DrawStatementTable();
        }

        public void DrawStatementTable()
        {
            // Start Table
            var colCount = 4;
            var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable;
            if (!ImGui.BeginTable("StatementTable", colCount, tableFlags))
                return;

            // Set Headers
            ImGui.TableSetupColumn("Player", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Cnt.", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Total", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Taxes", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            // Prep vars
            var playerList = new List<string>();
            foreach (var key in recordKeeper.userTable.Keys)
                playerList.Add((string)key);
            playerList = playerList.OrderBy(x => x).ToList();

            // Populate Table 
            if (recordKeeper.rewardList.Count < 1)
            {
                ImGui.TableNextRow();
                for (var i = 0; i < colCount; i++)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text("---");
                }
            }
            else
            {
                foreach (var player in playerList)
                {
                    // Calculations first...
                    var playerRewards = plugin.rewardTracker.rewardList
                        .Where(e => e.player == player);
                    var totalGil = playerRewards.Select(e => e.value).Sum();
                    var taxAmt = (int)Math.Floor(totalGil * (taxRate / 100d));

                    // ...Display later!
                    ImGui.PushStyleColor(ImGuiCol.Button, 0x00000000);

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    if (ImGui.SmallButton(player))
                        ImGui.SetClipboardText(player);

                    ImGui.TableNextColumn();
                    if (ImGui.SmallButton($"{playerRewards.Count()}"))
                        ImGui.SetClipboardText($"{playerRewards.Count()}");

                    ImGui.TableNextColumn();
                    if (ImGui.SmallButton($"{totalGil}g"))
                        ImGui.SetClipboardText($"{totalGil}g");

                    ImGui.TableNextColumn();
                    if (ImGui.SmallButton($"{taxAmt}g"))
                        ImGui.SetClipboardText($"{taxAmt}g");

                    ImGui.PopStyleColor();
                }
            }
            ImGui.EndTable();
        }

        public void _OldDraw()
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
