using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using MapGilTracker.Interfaces;
using MapGilTracker.Models;
using MapGilTracker.Tools;
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
            // Get whole region size
            var windowSize = ImGui.GetContentRegionAvail();

            // Start Main Table Wrapper
            if (!ImGui.BeginTable("##ReportTabContainer", 2))
                return;

            // Set Left Column Flags
            ImGui.TableSetupColumn("##ReportTabLeftColumn", ImGuiTableColumnFlags.WidthFixed, windowSize.X - 240f);
            ImGui.TableNextColumn();

            // Create child for lefthand side
            var leftSize = ImGui.GetContentRegionAvail();
            if (ImGui.BeginChild("###ReportTabLeftColumnChild", leftSize, false, ImGuiWindowFlags.NoDecoration))
            {
                DrawStatementTable();
                ImGui.EndChild();
            }

            // Set Right Column Flags
            ImGui.TableSetupColumn("##ReportTabRightColumn", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextColumn();

            // Create child for righthand side
            var rightSize = ImGui.GetContentRegionAvail();
            if (ImGui.BeginChild("###ReportTabRightColumnChild", rightSize, false, ImGuiWindowFlags.NoDecoration))
            {

                // Tax Rate Selector
                ImGui.InputInt("Tax %", ref taxRate, 5);
                if (taxRate < 0) taxRate = 0; 
                if (taxRate > 100) taxRate = 100; 
                ImGui.Separator();

                // Total number of tracked players
                ImGui.Text($"# of Tracked Participants: {recordKeeper.userTable.Count}");
                int totalEarnings = recordKeeper.rewardList.Select(e => e.value).Sum();

                // Total amount of gil earned by all players
                ImGui.Text($"Total gil earned: {totalEarnings:n0}g");

                // Average Gross Income
                int grossPlAvg = recordKeeper.userTable.Count > 0 ?
                    (int)Math.Floor(totalEarnings / (double)recordKeeper.userTable.Count) : 0;
                ImGui.Text($"Gross avg. earned: {grossPlAvg:n0}g");
                ImGui.SameLine(); ImGui.TextDisabled("(?)");
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Average amount earned per\nplayer, before expenses are\ncalculated.");

                // Average Net Income
                int netPlAvg = (int)(grossPlAvg * ((100-taxRate) / 100d));
                ImGui.Text($"Net avg. earned: {netPlAvg:n0}g");
                ImGui.SameLine(); ImGui.TextDisabled("(?)");
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Average amount earned per\nplayer, after expenses are\ncalculated.");

                ImGui.EndChild();
            }

            // End table
            ImGui.EndTable();
        }

        public void DrawStatementTable()
        {
            // Create second child for lefthand side
            var regionAvail = ImGui.GetContentRegionAvail();
            var leftSize = regionAvail with { Y = regionAvail.Y - 25 };
            if (ImGui.BeginChild("###ReportTabLeftColumnChildChild", leftSize, false, ImGuiWindowFlags.NoDecoration))
            {
                // Start Table
                var colCount = 5;
                var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg;
                if (!ImGui.BeginTable("StatementTable", colCount, tableFlags))
                    return;

                // Set Headers
                ImGui.TableSetupColumn("Player", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Cnt.", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Total", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Keeps", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Owes", ImGuiTableColumnFlags.WidthFixed);
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
                    // We have to append a hidden index to each button, otherwise the
                    // UI only responds to clicks on the first instance of a str
                    var index = 0;

                    // Running Totals
                    int ptyRwdCnt = 0; int ptyTotalGil = 0;
                    int ptyTaxAmt = 0; int ptyTakeAmt = 0;

                    foreach (var player in playerList)
                    {
                        // Calculations first...
                        var playerRewards = plugin.rewardTracker.rewardList
                            .Where(e => e.player == player);
                        var rewardCountStr = $"{playerRewards.Count():n0}";
                        var totalGil = playerRewards.Select(e => e.value).Sum();
                        var totalGilStr = $"{totalGil:n0}g";
                        var taxAmt = (int)Math.Floor(totalGil * (taxRate / 100d));
                        var taxAmtStr = $"{taxAmt:n0}g";
                        var takeAmt = totalGil - taxAmt;
                        var takeAmtStr = $"{takeAmt:n0}g";

                        // Increase running totals
                        ptyRwdCnt += playerRewards.Count(); ptyTotalGil += totalGil;
                        ptyTaxAmt += taxAmt; ptyTakeAmt += takeAmt;

                        // Print row
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"{player}##Nme{index}"))
                            Utils.CopyToClipboard(player);

                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"{rewardCountStr}##Rwd{index}"))
                            Utils.CopyToClipboard(rewardCountStr);

                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"{totalGilStr}##Ttl{index}"))
                            Utils.CopyToClipboard(totalGilStr);

                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"{takeAmtStr}##Tke{index}"))
                            Utils.CopyToClipboard(takeAmtStr);

                        ImGui.TableNextColumn();
                        if (ImGui.Selectable($"{taxAmtStr}##Tax{index}"))
                            Utils.CopyToClipboard(taxAmtStr);

                        index++;
                    }

                    // Spacer
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("---");

                    // Convert running totals to strings
                    var ptyRwdCntStr = $"{ptyRwdCnt:n0}";
                    var ptyTotalGilStr = $"{ptyTotalGil:n0}";
                    var ptyTaxAmtStr = $"{ptyTaxAmt:n0}";
                    var ptyTakeAmtStr = $"{ptyTakeAmt:n0}";

                    // Print Totals Row
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    if (ImGui.Selectable($"Total##Nme{index}"))
                        Utils.CopyToClipboard("Total");

                    ImGui.TableNextColumn();
                    if (ImGui.Selectable($"{ptyRwdCntStr}##Rwd{index}"))
                        Utils.CopyToClipboard($"{ptyRwdCntStr}");

                    ImGui.TableNextColumn();
                    if (ImGui.Selectable($"{ptyTotalGilStr}g##Ttl{index}"))
                        Utils.CopyToClipboard($"{ptyTotalGilStr}g");

                    ImGui.TableNextColumn();
                    if (ImGui.Selectable($"{ptyTakeAmtStr}g##Tke{index}"))
                        Utils.CopyToClipboard($"{ptyTakeAmtStr}g");

                    ImGui.TableNextColumn();
                    if (ImGui.Selectable($"{ptyTaxAmtStr}g##Tax{index}"))
                        Utils.CopyToClipboard($"{ptyTaxAmtStr}g");

                }

                ImGui.EndTable();
                ImGui.EndChild();
            }

            // Extra Hint
            ImGui.Separator();
            ImGui.TextDisabled("Clicking on any value will copy it to your clipboard!");
        }
    }
}
