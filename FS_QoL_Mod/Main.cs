using MelonLoader;
using UnityEngine;
using FS_FovChanger.UI;
using System.Collections;
using HarmonyLib;
using FS_FovChanger.Patches;

[assembly: MelonInfo(typeof(FS_FovChanger.Main), "FS QoL Mod", "2.0.0", "IamN4m3l3ss")]

namespace FS_FovChanger 
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Config.Initialize();
            HarmonyInstance.PatchAll(typeof(CameraPatches).Assembly);
            HarmonyInstance.PatchAll(typeof(PauseManagerPatch).Assembly);
            LoggerInstance.Msg("FS QoL Mod Initialized! Preferences Loaded.");
        }

        public override void OnUpdate()
        {
            // The PauseManager now handles showing the menu.
            // We just need to handle closing it.
            if (Config.showMenu && Input.GetKeyDown(KeyCode.Escape))
            {
                Config.showMenu = false;
                // The game's PauseManager will handle resuming.
            }
            
            CustomCrosshair.Update();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            MelonCoroutines.Start(DelayedApply());
        }

        private IEnumerator DelayedApply()
        {
            yield return new WaitForSeconds(1f);
            ModGUI.ApplyVisibility();
        }

        public override void OnGUI()
        {
            if (Config.showMenu)
            {
                ModGUI.Draw();
            }

            Hud.Draw(); 
        }
    }
    
    [HarmonyPatch(typeof(PauseManager), "Pause")]
    public static class PauseManagerPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Config.showMenu = true;
        }
    }
}