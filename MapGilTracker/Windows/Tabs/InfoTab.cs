using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;
using MapGilTracker.Interfaces;
using MapGilTracker.Models;

namespace MapGilTracker.Windows.Tabs
{
    public class InfoTab : ITabItem
    {
        public string TabName => "Info";

        public bool Enabled => true;

        private MapGilTracker plugin;
        private AssemblyName asm = Assembly.GetExecutingAssembly().GetName();

        public InfoTab(MainWindow mainWindow)
        {
            plugin = mainWindow.plugin;
        }

        public void Draw()
        {
            // Get window size
            var regionSize = ImGui.GetContentRegionAvail();
            var padding = (regionSize.Y - 150) * 0.5f;

            ImGui.Dummy(new Vector2(0.0f, padding));
            drawCenteredText($"MapGilTracker v{asm.Version}");
            drawCenteredText($"By: Andrew \"Azure\" Augustine");
            drawCenteredText("---");

            // Color Palletes (RGBA)
            var gh_default = new Vector4(0.1f, 0.12f, 0.14f, 1f);
            var gh_active = new Vector4(0.24f, 0.26f, 0.28f, 1f);
            var gh_hovered = new Vector4(0.19f, 0.21f, 0.23f, 1f);
            var kofi_default = new Vector4(1f, 0.357f, 0.369f, 1f);
            var kofi_active = new Vector4(1f, 0.62f, 0.612f, 1f);
            var kofi_hovered = new Vector4(1f, 0.494f, 0.482f, 1f);
            
            // Left Padding
            ImGui.Text("");
            ImGui.SameLine((regionSize.X - 225) * 0.5f);

            // GitHub Repo
            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Code, "GitHub", gh_default, gh_active, gh_hovered))
                Process.Start(new ProcessStartInfo { FileName = "https://github.com/Azure-Agst/MapGilTracker", UseShellExecute = true });
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Visit the GitHub Repo!");
            ImGui.SameLine();

            // GitHub Issues
            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.ExclamationTriangle, "Issues", gh_default, gh_active, gh_hovered))
                Process.Start(new ProcessStartInfo { FileName = "https://github.com/Azure-Agst/MapGilTracker/issues", UseShellExecute = true });
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Found a bug? Have an issue?\nReport it here!");
            ImGui.SameLine();

            // Ko-Fi
            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Coffee, "Ko-Fi", kofi_default, kofi_active, kofi_hovered))
                Process.Start(new ProcessStartInfo { FileName = "https://ko-fi.com/azureagst", UseShellExecute = true });
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Buy me a coffee!");
        }

        private void drawCenteredText(string str, bool disabled = true)
        {
            // Calculate Spacing
            var regionSize = ImGui.GetContentRegionAvail();
            var textWidth = ImGui.CalcTextSize(str);
            var indent = (regionSize.X - textWidth.X) * 0.5f;

            // Print
            ImGui.Text("");
            ImGui.SameLine(indent);
            if (disabled)
                ImGui.TextDisabled(str);
            else
                ImGui.Text(str);
        }
    }
}
