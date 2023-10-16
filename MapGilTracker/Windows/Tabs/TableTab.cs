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

namespace MapGilTracker.Windows.Tabs
{
    internal class TableTab: ITabItem
    {

        
        public string TabName => "Table View";
        public bool Enabled => true;

        private MapGilTracker plugin;
        private List<RewardRecord> rewardList;

        public TableTab(MainWindow mainWindow) {
            plugin = mainWindow.plugin;
            rewardList = mainWindow.plugin.rewardTracker.rewardList;
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
            if (rewardList.Count < 1)
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
                foreach (var entry in rewardList)
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

            // IsTracking Button
            if (plugin.isTracking)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x0000b300);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x0000b300);
            } 
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x000000b3);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x000000b3);
            }

            if (ImGui.Button(plugin.isTracking ? "Enabled" : "Disabled"))
            {
                plugin.isTracking = !plugin.isTracking;
            }

            ImGui.PopStyleColor(2);



        }
    }
}
