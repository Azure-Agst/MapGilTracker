using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using MapGilTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGilTracker
{
    internal class Services
    {
        [PluginService] public static DalamudPluginInterface Plugin { get; set; } = null!;
        [PluginService] public static IPartyList PartyList { get; set; } = null!;
        [PluginService] public static IClientState ClientState { get; set; } = null!;
        [PluginService] public static IAddonLifecycle AddonLifecycle { get; set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; set; } = null!;
        [PluginService] public static IToastGui ToastGui { get; set; } = null!;
        [PluginService] public static IPluginLog Log { get; set; } = null!;

        internal static void Init(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Services>();
        }
    }
}
