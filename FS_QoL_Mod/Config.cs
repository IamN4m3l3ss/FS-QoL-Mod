using MelonLoader;

namespace FS_FovChanger
{
    public static class Config
    {
        public static MelonPreferences_Category ModCategory;
        
        // Settings
        public static MelonPreferences_Entry<float> SavedFov;
        public static MelonPreferences_Entry<float> SavedSens;
        
        // Visuals
        public static MelonPreferences_Entry<bool> HideWeapons;
        public static MelonPreferences_Entry<bool> HideTurret;
        public static MelonPreferences_Entry<bool> ShowPoiBeam;
        public static MelonPreferences_Entry<bool> ShowHudExtras;
        
        // NEW: Independent HUD Toggles
        public static MelonPreferences_Entry<bool> ShowCompass;
        public static MelonPreferences_Entry<bool> ShowAmmoCircles;
        public static MelonPreferences_Entry<bool> ShowMovementBars;
        
        // Mechanics
        public static MelonPreferences_Entry<bool> EnableDoubleJump;
        public static MelonPreferences_Entry<bool> EnableWallRun;

        public static bool showMenu = false;

        public static void Initialize()
        {
            ModCategory = MelonPreferences.CreateCategory("FS_QoL_Mod");
            
            SavedFov = ModCategory.CreateEntry("FOV", 60f);
            SavedSens = ModCategory.CreateEntry("Sensitivity", 2.0f);
            
            HideWeapons = ModCategory.CreateEntry("HideWeapons", false);
            HideTurret = ModCategory.CreateEntry("HideTurret", false);
            ShowPoiBeam = ModCategory.CreateEntry("ShowPoiBeam", true);
            ShowHudExtras = ModCategory.CreateEntry("ShowHudExtras", true);

            // Corrected MelonLoader Entries
            ShowCompass = ModCategory.CreateEntry("ShowCompass", true);
            ShowAmmoCircles = ModCategory.CreateEntry("ShowAmmoCircles", true);
            ShowMovementBars = ModCategory.CreateEntry("ShowMovementBars", true);
            
            EnableDoubleJump = ModCategory.CreateEntry("EnableDoubleJump", false);
            EnableWallRun = ModCategory.CreateEntry("EnableWallRun", false);
        }
    }
}