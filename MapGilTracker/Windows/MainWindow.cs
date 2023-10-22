
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using MapGilTracker.Interfaces;
using MapGilTracker.Models;
using MapGilTracker.Windows.Tabs;

namespace MapGilTracker.Windows
{
    public class MainWindow : Window, IDisposable
    {
        public MapGilTracker plugin;
        public List<ITabItem> tabs;

        public MainWindow(MapGilTracker plugin) : base(
            "Gil Tracker Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.plugin = plugin;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(800, 350),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            tabs = new List<ITabItem>()
            {
                new TableTab(this),
                new ReportTab(this),
                new InfoTab(this)
            };
        }

        public void Dispose() { }

        public override void Draw()
        {
            // If not loaded, show error
            if (!plugin.isLoggedIn)
            {
                ImGui.Text("Not logged in! ClientState inaccessible.");
                return; 
            }

            // Else, render our main tab bar
            if (ImGui.BeginTabBar("##MainTabBar"))
            {
                foreach (var tab in tabs)
                {
                    if (ImGui.BeginTabItem(tab.TabName))
                    {
                        tab.Draw();
                        ImGui.EndTabItem();
                    }
                }
                ImGui.EndTabBar();
            }
        }
    }
}
