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
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace MapGilTracker
{
    public sealed class MapGilTracker : IDalamudPlugin
    {
        public string Name => "FATE/Map Gil Tracker";
        public string[] mainCmdAliases = { "/giltracker", "/gt" };
        public string[] toggleCmdAliases = { "/gttoggle" };

        public static string rgxPattern = @"You obtain ([\d,]+) gil\.";
        public Regex gilRegex = new Regex(rgxPattern);

        public bool isLoggedIn {
            get {
                return Services.ClientState.LocalPlayer != null;
            }
        }

        public TimedList<int> chatGilKeeper = new TimedList<int>(10);
        public TimedList<int> fateGilKeeper = new TimedList<int>(10);

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
            Services.Chat.ChatMessage += OnChatMsg;

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

        private void OnChatMsg(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            // If we get a message in either of the Loot Notices channels...
            // NOTE: FATE gil is logged in normal 2110 loot channel. Chests are logged in 62, for some reason.
            if (message.ToString().Contains("gil"))
                Services.Log.Debug($"Chat: [{type}] {message}");
            if (!((int)type == 2110 || (int)type == 62)) return;

            // ... See if it's gil and if so, stash the value
            var r = gilRegex.Match(message.ToString());
            if (!r.Success) return;

            // Get gil value
            var gilStr = gilRegex.Match(message.ToString()).Groups[1].ToString();
            var gilValue = int.Parse(gilStr, NumberStyles.AllowThousands);

            // See if we have a value in the other list.
            // If so, remove from other list and process
            // If not, add gil value to this list
            var i = fateGilKeeper.Contains(gilValue);
            if (i == null)
            {
                Services.Log.Debug($"Chat: No Match Found, Adding {gilValue} to TimedList!");
                chatGilKeeper.Add(gilValue);
            }
            else 
            {
                Services.Log.Debug($"Chat: Found match for {gilValue}, processing!");
                fateGilKeeper.Remove(i);
                AddGilRecord(gilValue);
            }
        }

        private unsafe void OnFatePostSetup(AddonEvent type, AddonArgs args)
        {
            // Log that we detected a FateReward popup
            Services.Log.Debug($"FATE: [{type}] Popup Detected!");

            // If not tracking, just exit.
            if (!config.isTracking) return;

            // Work our way down to the text box that we need. Note that these hardcoded
            // node IDs were found via trial and error using Dalamud's built in addon explorer tool
            var fateRewardAddon = (AtkUnitBase*)args.Addon;
            var gilValue = Utils.GetIntFromFateReward(fateRewardAddon);

            // See if we have a value in the other list.
            // If so, remove from other list and process
            // If not, add gil value to this list
            var i = chatGilKeeper.Contains(gilValue);
            if (i == null)
            {
                Services.Log.Debug($"FATE: No Match Found, Adding {gilValue} to TimedList!");
                fateGilKeeper.Add(gilValue);
            }
            else
            {
                Services.Log.Debug($"FATE: Found match for {gilValue}, processing!");
                chatGilKeeper.Remove(i);
                AddGilRecord(gilValue);
            }
        }

        private void AddGilRecord(int gilAmt)
        {
            // Get current time
            var curTime = DateTime.Now;

            // Format player list
            // If solo, just the player. Else, the party.
            if (Services.PartyList.Count == 0)
            {
                var playerName = Services.ClientState.LocalPlayer!.Name.ToString();
                rewardTracker.AddRecord(gilAmt, playerName, curTime);
            }
            else
            {
                foreach (var player in Services.PartyList)
                    rewardTracker.AddRecord(gilAmt, player.Name.ToString(), curTime);
            }

            // Save config
            config.Save();

            // Toast me
            Services.Chat.Print($"[GT] Recorded {gilAmt}g!");
        }
    }
}
