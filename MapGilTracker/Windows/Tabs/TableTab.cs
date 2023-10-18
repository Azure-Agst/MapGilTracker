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
        public string TabName => "Record View";
        public bool Enabled => true;

        private MapGilTracker plugin;
        private RewardRecordKeeper recordKeeper;
        private bool copyTimestamps = false;

        public TableTab(MainWindow mainWindow) {
            plugin = mainWindow.plugin;
            recordKeeper = mainWindow.plugin.rewardTracker;
        }

        public void Draw()
        {
            // Get whole region size
            var windowSize = ImGui.GetContentRegionAvail();

            // Start Main Table Wrapper
            if (!ImGui.BeginTable("##TableTabContainer", 2))
                return;

            // Set Left Column Flags
            ImGui.TableSetupColumn("##TableTabLeftColumn", ImGuiTableColumnFlags.WidthFixed, windowSize.X - 240f);
            ImGui.TableNextColumn();

            // Create child for lefthand side
            var leftSize = ImGui.GetContentRegionAvail();
            if (ImGui.BeginChild("###TableTabLeftColumnChild", leftSize, false, ImGuiWindowFlags.NoDecoration))
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
            // Create second child for lefthand side
            var regionAvail = ImGui.GetContentRegionAvail();
            var leftSize = regionAvail with { Y = regionAvail.Y - 25 };
            if (ImGui.BeginChild("###TableTabLeftColumnChildChild", leftSize, false, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                // Start Table
                var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg;
                if (!ImGui.BeginTable("RecordTable", 3, tableFlags))
                    return;

                // Set Headers
                ImGui.TableSetupColumn("Timestamp", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Participant", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                // Populate Table 
                if (recordKeeper.rewardList.Count < 1)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("---");
                    ImGui.TableNextColumn();
                    ImGui.Text("---");
                    ImGui.TableNextColumn();
                    ImGui.Text("---");
                }
                else
                {
                    // We have to append a hidden index to each button, otherwise the
                    // UI only responds to clicks on the first instance of a str
                    var index = 0;

                    foreach (var entry in recordKeeper.rewardList)
                    {
                        // Set up vars
                        var tsStr = entry.timestamp.ToString();
                        var gilStr = $"{entry.value}g";

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"{tsStr}##{index}"))
                            Utils.CopyToClipboard(tsStr);

                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"{entry.player}##{index}"))
                            Utils.CopyToClipboard(entry.player ?? "");

                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"{gilStr}##{index}"))
                            Utils.CopyToClipboard(gilStr);

                        index++;
                    }
                }
                ImGui.EndTable();
                ImGui.EndChild();
            }

            // Extra Hint
            ImGui.Separator();
            ImGui.TextDisabled("Clicking on any value will copy it to your clipboard!");
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
                plugin.config.ToggleIsTracking();
            ImGui.PopStyleColor(2);
            ImGui.Separator();

            // Export for spreadsheet
            ImGui.Text("Copy Distinct Reward List");
            ImGui.TextDisabled("For easy pasting into Excel/Sheets");

            ImGui.Checkbox("Include timestamps?", ref copyTimestamps);
            if (ImGui.Button("Copy Spreadsheet Data"))
                CopyDistinctRewards();
            ImGui.Separator();

            // Clear button
            if (ImGui.Button("Clear"))
                ImGui.OpenPopup("Clear##TableTab");

#if DEBUG
            // If indev, add a sample data button
            ImGui.SameLine();
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
                    plugin.config.Save();
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

        private void CopyDistinctRewards()
        {
            // Get a list of unique events
            var distinctEvents = plugin.rewardTracker.rewardList
                .DistinctBy(e => e.timestamp)
                .ToList();

            // Format our string
            string valueStr = "";
            foreach (var record in distinctEvents)
            {
                if (copyTimestamps)
                    valueStr += record.timestamp.ToString() + "\t";
                valueStr += record.value.ToString() + "\n";
            }

            // Copy to clipboard
            ImGui.SetClipboardText(valueStr);

            // Log in chat
            var devCnt = distinctEvents.Count;
            Services.Chat.Print($"Copied list of {devCnt} entries to the clipboard!");
        }
    }
}
