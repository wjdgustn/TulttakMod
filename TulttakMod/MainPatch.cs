using System;
using System.CodeDom;
using System.Configuration;
using ADOFAI;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace TulttakMod.MainPatch {
    // public class Text : MonoBehaviour {
    //     public static string Content = "Wa sans!";
    //     
    //     void OnGUI() {
    //         GUIStyle style = new GUIStyle();
    //         style.fontSize = (int) 50.0f;
    //         style.font = RDString.GetFontDataForLanguage(RDString.language).font;
    //         style.normal.textColor = Color.white;
    //
    //         GUI.Label(new Rect(10, -10, Screen.width, Screen.height), Content, style);
    //     }
    // }

    // [HarmonyPatch(typeof(scrCountdown), "ShowGetReady")]
    //
    // internal static class ResetGame {
    //     private static void Prefix(ADOBase __instance) {
    //         Main.Mod.Logger.Log("wa reset");
    //         Main._called = false;
    //     }
    // }
    //
    // [HarmonyPatch(typeof(scrCountdown), "Update")]
    //
    // internal static class StartGame {
    //     private static void Prefix(ADOBase __instance) {
    //         if (Main._called || !scrController.instance.goShown)
    //             return;
    //         Main._called = true;
    //         Main.Mod.Logger.Log("started game");
    //         __instance.conductor.song.pitch = (float) 0.5;
    //     }
    // }
    //
    // [HarmonyPatch(typeof(ADOBase), "isMobile", MethodType.Getter)]
    //
    // internal static class ForceMobile {
    //     private static bool Prefix(ref bool __result) {
    //         __result = true;
    //         return false;
    //     }
    // }
    //
    // [HarmonyPatch(typeof(PauseMenu), "ChangeLevel")]
    //
    // internal static class MiniSpeed {
    //     private static void Prefix(bool next) {
    //         if (!GCS.speedTrialMode) return;
    //         if (!Input.GetKey(KeyCode.LeftShift)) return;
    //         GCS.nextSpeedRun += next ? -0.09f : 0.09f;
    //     }
    // }
    //
    // [HarmonyPatch(typeof(scrController), "Update")]
    //
    // internal static class ChangeTitle {
    //     private static void Prefix(scrController __instance) {
    //         var str = RDString.Get("levelSelect.multiplier").Replace("[multiplier]", GCS.currentSpeedRun.ToString("0.00"));
    //         __instance.txtCaption.text = __instance.caption + " (" + str + ")";
    //     }
    // }
    //
    // [HarmonyPatch(typeof(PauseMenu), "UpdateLevelDescriptionAndReload")]
    //
    // internal static class ChangePauseMenuTitle {
    //     private static void Postfix(PauseMenu __instance) {
    //         if (!GCS.speedTrialMode) return;
    //
    //         var str1 = scrController.instance.caption;
    //         var str2 = RDString.Get("levelSelect.multiplier").Replace("[multiplier]", GCS.nextSpeedRun.ToString("0.00"));
    //         str1 = str1 + " (" + str2 + ")";
    //
    //         __instance.subtitle.text = str1;
    //     }
    // }

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
                __instance.controller.lm.CalculateFloorAngleLengths();
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
        private static scrFloor beforeTextFloor;

        private static void Postfix(scnEditor __instance) {
            if (__instance.showFloorNums) return;
            
            if (beforeTextFloor != null) {
                beforeTextFloor.editorNumText.gameObject.SetActive(false);
                beforeTextFloor = null;
            }
            
            if (!Main.Settings.ShowAngle) return;
            
            if (!__instance.SelectionIsSingle()) return;

            var clickedFloor = __instance.selectedFloors[0];
            
            __instance.controller.lm.CalculateFloorAngleLengths();
            
            clickedFloor.editorNumText.gameObject.SetActive(true);
            clickedFloor.editorNumText.letterText.text = Mathf.Round((float) clickedFloor.angleLength * 57.29578f).ToString();

            beforeTextFloor = clickedFloor;
        }
    }
}