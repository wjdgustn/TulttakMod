using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;

namespace TulttakMod {
    #if DEBUG
    [EnableReloading]
    #endif

    internal static class Main {
        internal static UnityModManager.ModEntry Mod;
        private static Harmony _harmony;
        internal static bool IsEnabled { get; private set; }
        internal static MainSettings Settings { get; private set; }

        public static bool EnableEasterEgg() {
            return new long[] {
                348009971037765633L, // 정현수#1234
                800236737182957608L, // 새제비#1201
                396515657560096780L, // Pharah#5252
                550696785518133299L, // 한비#0370
            }.Contains(DiscordController.currentUserID);
        }

        public static bool ShowSajabe() {
            return new long[] {
                348009971037765633L, // 정현수#1234
            }.Contains(DiscordController.currentUserID);
        }

        private static void Load(UnityModManager.ModEntry modEntry) {
            Mod = modEntry;
            Mod.OnToggle = OnToggle;
            Settings = UnityModManager.ModSettings.Load<MainSettings>(modEntry);
            Mod.OnGUI = Settings.OnGUI;
            Mod.OnSaveGUI = Settings.OnSaveGUI;
            
            #if DEBUG
            Mod.OnUnload = Stop;
            #endif
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            IsEnabled = value;

            if (value) Start();
            else Stop(modEntry);

            return true;
        }
        
        private static FileSystemWatcher watcher;
        private static void OnDllChanged(object source, FileSystemEventArgs e) {
            Mod.Logger.Log($"{Mod.Info.Id} mod change detected! Reloading...");
            watcher.Changed -= OnDllChanged;
            watcher.Dispose();
            watcher = null;
            Mod.GetType().GetMethod("Reload", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(Mod, null);
        }


        private static void Start() {
            _harmony = new Harmony(Mod.Info.Id);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            
#if DEBUG
            watcher = new FileSystemWatcher(Mod.Path);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.dll";

            watcher.Changed += OnDllChanged;

            watcher.EnableRaisingEvents = true;
#endif
        }

        private static bool Stop(UnityModManager.ModEntry modEntry) {
            _harmony.UnpatchAll(Mod.Info.Id);
            #if RELEASE
            _harmony = null;
            #endif

            return true;
        }
    }
}