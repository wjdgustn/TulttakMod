using System;
using System.Collections.Generic;
using System.Reflection;
using ADOFAI;
using HarmonyLib;
using UnityEngine;

namespace TulttakMod.MainPatch {
    [HarmonyPatch(typeof(scnEditor), "ShowNextPage")]

    internal static class PreventGoFirstPage {
        private static bool Prefix(scnEditor __instance) {
            var values = Enum.GetValues(typeof(LevelEventCategory));
            var num = Array.IndexOf(values, __instance.currentCategory);

            return !(num + 1 == values.Length && Main.Settings.EventNoMoveToFirst);
        }
    }
    
    [HarmonyPatch(typeof(scnEditor), "ShowPrevPage")]
    
    internal static class PreventGoLastPage {
        private static bool Prefix(scnEditor __instance) {
            var values = Enum.GetValues(typeof(LevelEventCategory));
            var num = Array.IndexOf(values, __instance.currentCategory);

            return !(num - 1 < 0 && Main.Settings.EventNoMoveToFirst);
        }
    }

    [HarmonyPatch(typeof(scnEditor), "OnSelectedFloorChange")]

    internal static class ShowBeats {
        private static scrFloor beforeTextFloor;
        
        private static void Postfix(scnEditor __instance) {
            if (__instance.showFloorNums) return;
            
            if (beforeTextFloor != null) {
                beforeTextFloor.editorNumText.gameObject.SetActive(false);
                beforeTextFloor = null;
            }
            
            if (!Main.Settings.ShowBeats) return;

            if (__instance.SelectionIsSingle() || __instance.selectedFloors.Count == 0) return;

            var editorLastSelectedFloor = (scrFloor) AccessTools.Field(typeof(scnEditor), "lastSelectedFloor").GetValue(__instance);
            var clickedFloorIsFirst = __instance.selectedFloors[0] != editorLastSelectedFloor;
            var firstFloor = __instance.selectedFloors[0];
            var lastFloor = __instance.selectedFloors[__instance.selectedFloors.Count - 1];
            var clickedFloor = clickedFloorIsFirst ? firstFloor : lastFloor;

            var beats = 0.0f;

            for (var i = 0; i < __instance.selectedFloors.Count - 1; i++) {
                ADOBase.lm.CalculateFloorAngleLengths();
                var currentFloor = __instance.selectedFloors[i];
                var result = (Mathf.Round((float) currentFloor.angleLength * 57.29578f) / 180);
                beats += result * (firstFloor.speed / currentFloor.speed);
            }

            clickedFloor.editorNumText.gameObject.SetActive(true);
            clickedFloor.editorNumText.letterText.text = beats.ToString();
            
            beforeTextFloor = clickedFloor;
        }
    }

    [HarmonyPatch(typeof(scnEditor), "OnSelectedFloorChange")]

    public static class ShowAngle {
        public static scrFloor beforeTextFloor;

        public static void Postfix(scnEditor __instance) {
            if (beforeTextFloor != null) {
                beforeTextFloor.editorNumText.letterText.text = $"{beforeTextFloor.seqID}";
                if (!__instance.showFloorNums) beforeTextFloor.editorNumText.gameObject.SetActive(false);
                beforeTextFloor = null;
            }
            
            if (__instance.showFloorNums) return;
            
            if (!Main.Settings.ShowAngle) return;
            
            if (!__instance.SelectionIsSingle()) return;

            var clickedFloor = __instance.selectedFloors[0];
            
            ADOBase.lm.CalculateFloorAngleLengths();
            
            clickedFloor.editorNumText.gameObject.SetActive(true);
            clickedFloor.editorNumText.letterText.text = $"{(float) clickedFloor.angleLength * Mathf.Rad2Deg:#.###}";

            beforeTextFloor = clickedFloor;
        }
    }

    [HarmonyPatch(typeof(scnEditor), "RemakePath")]

    internal static class RecoverFloorNum
    {
        private static void Postfix(scnEditor __instance)
        {
            if (__instance.showFloorNums)
            {
                if (ShowAngle.beforeTextFloor == null) return;
                
                ShowAngle.beforeTextFloor.editorNumText.letterText.text = $"{ShowAngle.beforeTextFloor.seqID}";
                ShowAngle.beforeTextFloor = null;
            }
            else
            {
                ShowAngle.Postfix(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(scnEditor), "Update")]

    internal static class EditorKeymap {
        private static void Postfix(scnEditor __instance) {
            var ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (ctrl && shift && Input.GetKeyDown(KeyCode.B)) {
                var isMesh = !scnGame.instance.levelData.isOldLevel;
                if (isMesh) {
                    var data = Util.ToLegacy(scnGame.instance.levelData.angleData, out var unsuccessful);
                    if (unsuccessful.HasValue) {
                        scnEditor.instance.ShowNotification($"{(Main.EnableEasterEgg() ? "제비탈모" : "변환 실패!")}\n타일 #{unsuccessful.Value.Item1}: {unsuccessful.Value.Item2}");
                        return;
                    }
                    scnGame.instance.levelData.pathData = data;
                }
                else {
                    scnGame.instance.levelData.angleData = Util.ToMesh(scnGame.instance.levelData.pathData);
                    scnGame.instance.RemakePath();
                }

                scnGame.instance.levelData.isOldLevel = isMesh;
                scnGame.instance.RemakePath();
            }

            if (alt && Input.GetKeyDown(KeyCode.C))
            {
                if (!__instance.SelectionIsSingle()) return;
                
                var selectedFloor = __instance.selectedFloors[0];
                var selectedFloorTime = selectedFloor.entryTime * 1000d;
                var firstFloor = __instance.floors[1];
                var firstFloorTime = firstFloor.entryTime * 1000d;
                
                var offset = (double)__instance.levelData.offset;
                
                var songPos = selectedFloorTime - firstFloorTime + offset;
                if (selectedFloor.seqID == 0) songPos = 0;
                
                GUIUtility.systemCopyBuffer = $"{songPos}";
            }
        }
    }

    [HarmonyPatch(typeof(DiscordController), "CheckForBirthday")]

    internal static class EastereggModname {
        private static void Postfix() {
            if (Main.EnableEasterEgg())
                Main.Mod.Info.DisplayName = $"{(Main.ShowSajabe() ? "새제비" : DiscordController.currentUsername)}모드";
        }
    }

    [HarmonyPatch]
    
    internal static class AutoTwirl
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(scnEditor), "CreateFloor", new[] { typeof(char), typeof(bool), typeof(bool) });
            yield return AccessTools.Method(typeof(scnEditor), "CreateFloor", new[] { typeof(float), typeof(bool), typeof(bool) });
        }
        
        private static void Postfix(scnEditor __instance)
        {
            if (!Main.Settings.AutoTwirl) return;

            if (!__instance.SelectionIsSingle()) return;
            
            var floor = __instance.selectedFloors[0].prevfloor ?? __instance.floors[__instance.floors.Count - 2];
            var prevFloor = floor?.prevfloor;

            if (floor == null) return;

            var angle = Mathf.Floor((float)floor.angleLength * Mathf.Rad2Deg * 100f) / 100f;
            var prevAngle = prevFloor == null
                ? 0
                : Mathf.Floor((float)prevFloor.angleLength * Mathf.Rad2Deg * 100f) / 100f;

            if (prevFloor != null
                && Main.Settings.ExcludePseudoSecondFloor
                && prevAngle <= Main.Settings.PseudoMaxAngle) return;

            if (angle < Main.Settings.TwirlMinAngle
                || angle > Main.Settings.TwirlMaxAngle) return;
            
            var addEvent = AccessTools.Method(typeof(scnEditor), "AddEvent");
            addEvent.Invoke(__instance, new object[] { floor.seqID, LevelEventType.Twirl });

            __instance.ApplyEventsToFloors();
            __instance.SelectFloor(__instance.floors[floor.seqID + 1]);
        }
    }
}