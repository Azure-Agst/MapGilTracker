using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using MapGilTracker.Models;
using System.Linq;
using MapGilTracker.Windows;
using static Lumina.Data.Parsing.Layer.LayerCommon;
using MapGilTracker.Tools;
using Lumina.Excel.GeneratedSheets;

namespace MapGilTracker
{
    public sealed class MapGilTracker : IDalamudPlugin
    {
        public string Name => "FATE/Map Gil Tracker";
        public string[] commandAliases = { "/giltracker", "/gt" };

        [PluginService]
        public static DalamudPluginInterface PluginInterface { get; set; } = null!;
        [PluginService]
        public static IPartyList PartyList { get; set; } = null!;
        [PluginService]
        public static IClientState ClientState { get; set; } = null!;
        [PluginService]
        public static IAddonLifecycle AddonLifecycle { get; set; } = null!;
        [PluginService]
        public static ICommandManager CommandManager { get; set; } = null!;
        [PluginService]
        public static IToastGui ToastGui { get; set; } = null!;
        [PluginService]
        public static IPluginLog PluginLog { get; set; } = null!;

        public RewardRecordKeeper rewardTracker { get; set; } = null!;


        private WindowSystem WindowSystem = new("MapGilTracker");
        private MainWindow MainWindow { get; set; }

        public bool isLoggedIn {
            get {
                return ClientState.LocalPlayer != null;
            }
        }
        public bool isTracking { get; set; } = false;


        public MapGilTracker()
        {
            // Initialize member vars
            rewardTracker = new RewardRecordKeeper();

            // Register Main Window
            MainWindow = new MainWindow(this);
            WindowSystem.AddWindow(MainWindow);

            // Add our draw function to imgui
            PluginInterface.UiBuilder.Draw += () => WindowSystem.Draw();
            PluginInterface.UiBuilder.OpenMainUi += () => MainWindow.Toggle();

            // Add command
            var commandInfo = new CommandInfo((_,_) => MainWindow.Toggle()) { HelpMessage = "Show the tracker menu!" };
            foreach(var alias in commandAliases)
                CommandManager.AddHandler(alias, commandInfo);

            // Register Fate Reward popup handler
            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "FateReward", OnFatePostSetup);

            // Add our login/logout handlers
            ClientState.Login += OnLogin;

            // If plugin is loaded while player is logged in, go ahead and run
            if (ClientState.LocalPlayer != null)
                OnLogin();
        }

        public void Dispose()
        {
            // Close windows
            WindowSystem.RemoveAllWindows();
            MainWindow.Dispose();

            // Deregister command
            foreach (var alias in commandAliases)
                CommandManager.RemoveHandler(alias);
        }

        private void OnLogin()
        {
#if DEBUG
            // If in debug mode, add some sample data
            new DummyData(this).Fill();
#endif
        }

        private unsafe void OnFatePostSetup(AddonEvent type, AddonArgs args)
        {
            // Log that we detected a FateReward popup
            PluginLog.Debug("Fate Popup Detected!");

            // If not tracking, just exit.
            if (!isTracking) return;

            // Work our way down to the text box that we need. Note that these hardcoded
            // node IDs were found via trial and error using Dalamud's built in addon explorer tool
            var fateRewardAddon = (AtkUnitBase*)args.Addon;
            var gil = Utils.GetIntFromFateReward(fateRewardAddon);

            // Print it for now
            PluginLog.Debug("Gil earned: {0}", gil);

            // Format player list
            // If solo, just the player. Else, the party.
            if (PartyList.Count == 0)
            {
                var playerName = ClientState.LocalPlayer!.Name.ToString();
                rewardTracker.AddRecord(gil, playerName);
            }
            else
            {
                foreach (var player in PartyList)
                    rewardTracker.AddRecord(gil, player.Name.ToString());
            }

            // Toast me
            ToastGui.ShowNormal($"GT: Recorded reward of {gil}g!");
        }
    }
}
