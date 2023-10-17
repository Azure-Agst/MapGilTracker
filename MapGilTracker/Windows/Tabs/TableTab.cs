using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Components;
using ImGuiNET;
using MapGilTracker.Interfaces;
using MapGilTracker.Models;
using MapGilTracker.Tools;

namespace MapGilTracker.Windows.Tabs
{
    internal class TableTab: ITabItem
    {

        
        public string TabName => "Table View";
        public bool Enabled => true;

        private MapGilTracker plugin;
        private RewardRecordKeeper recordKeeper;

        public TableTab(MainWindow mainWindow) {
            plugin = mainWindow.plugin;
            recordKeeper = mainWindow.plugin.rewardTracker;
        }

        public void Draw()
        {
            // Get whole region size
            var windowSize = ImGui.GetContentRegionAvail();

            // Start Main Table Wrapper
            var tableTabMainFlags = ImGuiTableFlags.Resizable;
            if (!ImGui.BeginTable("##TableTabContainer", 2, tableTabMainFlags))
                return;

            // Set Left Column Flags
            ImGui.TableSetupColumn("##TableTabLeftColumn", ImGuiTableColumnFlags.WidthStretch, windowSize.X * 0.7f);
            ImGui.TableNextColumn();

            // Create child for lefthand side
            var leftSize = ImGui.GetContentRegionAvail();
            if (ImGui.BeginChild("###TableTabLeftColumnChild", leftSize, false, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                DrawRecordsTable();
                ImGui.EndChild();
            }

            // Set Right Column Flags
            ImGui.TableSetupColumn("##TableTabRightColumn", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextColumn();

            // Create child for righthand side
            var rightSize = ImGui.GetContentRegionAvail();
            if (ImGui.BeginChild("###TableTabRightColumnChild", rightSize, false, ImGuiWindowFlags.NoDecoration))
            {
                DrawInfoMenu();
                ImGui.EndChild();
            }   

            // End table
            ImGui.EndTable();
        }

        private void DrawRecordsTable()
        {
            // Start Table
            var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable;
            if (!ImGui.BeginTable("RecordTable", 3, tableFlags))
                return;

            // Set Headers
            ImGui.TableSetupColumn("Timestamp", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Participation", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            // Populate Table 
            if (recordKeeper.rewardList.Count < 1)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("---");
                ImGui.TableNextColumn();
                ImGui.Text("No records!");
                ImGui.TableNextColumn();
                ImGui.Text("---");
            }
            else
            {
                foreach (var entry in recordKeeper.rewardList)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(entry.timestamp.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.value}g");
                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(entry.player);
                }
            }
            ImGui.EndTable();
        }

        public void DrawInfoMenu()
        {
            // Label
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Tracking Status:");
            ImGui.SameLine();

            // IsTracking Button Style
            if (plugin.config.isTracking)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x0000b300);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x0000b300);
            } 
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x000000b3);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x000000b3);
            }

            // IsTracking Button Implementation
            if (ImGui.Button(plugin.config.isTracking ? "Enabled" : "Disabled"))
            {
                plugin.config.isTracking = !plugin.config.isTracking;
                plugin.config.Save();
            }
            ImGui.PopStyleColor(2);

            // Other stats
            ImGui.Text($"# of Tracked Participants: {recordKeeper.userTable.Count}");
            int totalEarnings = recordKeeper.rewardList.Select(e => e.value).Sum();
            ImGui.Text($"Total amount earned: {totalEarnings}");
            int avgEarned = (int)Math.Floor(totalEarnings / (double)recordKeeper.userTable.Count);
            ImGui.Text($"Avg amount earned: {avgEarned}");

            ImGui.Separator();

            // Clear button
            if (ImGui.Button("Clear"))
                ImGui.OpenPopup("Clear##TableTab");

#if DEBUG
            // If indev, add a sample data button
            if (ImGui.Button("Add Dummy Data"))
                new DummyData(plugin).Fill();
#endif

            /*
             * Modal Definitions
             */

            // Clear Modal
            bool clearModalPOpen = true;
            if (ImGui.BeginPopupModal("Clear##TableTab", ref clearModalPOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                // Prompt
                ImGui.Text("Are you sure you want to clear the queue?");
                ImGui.Separator();

                // Buttons
                if (ImGui.Button("Confirm")) {
                    plugin.rewardTracker.Clear();
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

        }
    }
}
