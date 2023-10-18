using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using MapGilTracker.Models;
using MapGilTracker.Windows;
using MapGilTracker.Tools;
using System;

namespace MapGilTracker
{
    public sealed class MapGilTracker : IDalamudPlugin
    {
        public string Name => "FATE/Map Gil Tracker";
        public string[] mainCmdAliases = { "/giltracker", "/gt" };
        public string[] toggleCmdAliases = { "/gttoggle" };
        public bool isLoggedIn {
            get {
                return Services.ClientState.LocalPlayer != null;
            }
        }

        public RewardRecordKeeper rewardTracker { get; set; }
        public Configuration config { get; set; }

        private WindowSystem windowSystem = new("MapGilTracker");
        private MainWindow mainWindow { get; set; }

        public MapGilTracker(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            // Initialize our static services
            Services.Init(pluginInterface);

            // Load our config, or make a new one if needed
            config = Services.Plugin.GetPluginConfig() as Configuration ?? new Configuration();

            // Initialize any member vars
            rewardTracker = new RewardRecordKeeper(config);

            // Leaving this here for other open sourcers looking at how to set dev icons
            // NOTE: You will need to restart the game for any icon chamges to take effect
            // NOTE: Production icons are fetched asynchronously from the plugin repo, not cached locally
            var iconPath = Path.Combine(Services.Plugin.AssemblyLocation.Directory!.FullName, "images", "icon.png");
            if (File.Exists(iconPath))
                Services.Log.Info("Dev icon found!");

            // Register Main Window
            mainWindow = new MainWindow(this);
            windowSystem.AddWindow(mainWindow);

            // Add our draw function to imgui
            Services.Plugin.UiBuilder.Draw += () => windowSystem.Draw();
            Services.Plugin.UiBuilder.OpenMainUi += () => mainWindow.Toggle();

            // Add main window commands
            var mainWindowCmdInfo = new CommandInfo((_,_) => mainWindow.Toggle()) { 
                HelpMessage = "Show the tracker menu!" 
            };
            foreach (var alias in mainCmdAliases)
                Services.CommandManager.AddHandler(alias, mainWindowCmdInfo);

            // Add toggle command
            var toggleCmdInfo = new CommandInfo((_, _) => config.ToggleIsTracking()) {
                HelpMessage = "Toggle the status of the tracker" 
            };
            foreach (var alias in toggleCmdAliases)
                Services.CommandManager.AddHandler(alias, toggleCmdInfo);

            // Register Fate Reward popup handler
            Services.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "FateReward", OnFatePostSetup);

            // Add our login/logout handlers
            Services.ClientState.Login += OnLogin;

            // If plugin is loaded while player is logged in, go ahead and run
            if (Services.ClientState.LocalPlayer != null)
                OnLogin();
        }

        public void Dispose()
        {
            // Close windows
            windowSystem.RemoveAllWindows();
            mainWindow.Dispose();

            // Deregister commands
            foreach (var alias in mainCmdAliases)
                Services.CommandManager.RemoveHandler(alias);
            foreach (var alias in toggleCmdAliases)
                Services.CommandManager.RemoveHandler(alias);
        }

        private void OnLogin()
        {
#if DEBUG
            // If in debug mode, add some sample data
            //rewardTracker.Clear();
            //new DummyData(this).Fill();
            config.Save();
#endif
        }

        private unsafe void OnFatePostSetup(AddonEvent type, AddonArgs args)
        {
            // Log that we detected a FateReward popup
            Services.Log.Debug("Fate Popup Detected!");

            // If not tracking, just exit.
            if (!config.isTracking) return;

            // Work our way down to the text box that we need. Note that these hardcoded
            // node IDs were found via trial and error using Dalamud's built in addon explorer tool
            var fateRewardAddon = (AtkUnitBase*)args.Addon;
            var gil = Utils.GetIntFromFateReward(fateRewardAddon);

            // Get current time
            var curTime = DateTime.Now;

            // Format player list
            // If solo, just the player. Else, the party.
            if (Services.PartyList.Count == 0)
            {
                var playerName = Services.ClientState.LocalPlayer!.Name.ToString();
                rewardTracker.AddRecord(gil, playerName, curTime);
            }
            else
            {
                foreach (var player in Services.PartyList)
                    rewardTracker.AddRecord(gil, player.Name.ToString(), curTime);
            }

            // Save config
            config.Save();

            // Toast me
            Services.ToastGui.ShowNormal($"GT: Recorded reward of {gil}g!");
        }
    }
}
