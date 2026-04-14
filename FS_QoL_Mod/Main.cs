using MelonLoader;
using UnityEngine;
using FS_FovChanger.UI;        
using System.Collections; // Required for Coroutines

[assembly: MelonInfo(typeof(FS_FovChanger.Main), "FS QoL Mod", "1.1.0", "IamN4m3l3ss")]

namespace FS_FovChanger 
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Config.Initialize(); 
            LoggerInstance.Msg("FS QoL Mod Initialized! Preferences Loaded.");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                Config.showMenu = !Config.showMenu;
                if (!Config.showMenu) Config.ModCategory.SaveToFile(false);
            }
        }

        // Fired exactly when the level is finished initializing
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            MelonCoroutines.Start(DelayedApply());
        }

        // Waits 1 second for the player to actually spawn, then applies the mod
        private IEnumerator DelayedApply()
        {
            yield return new WaitForSeconds(1f);
            ModGUI.ApplyVisibility();
            if (Config.ShowPoiBeam.Value) ModGUI.UpdatePoiBeams();
        }

        public override void OnGUI()
        {
            ModGUI.Draw(); 
            Hud.Draw(); 
        }
    }
}
