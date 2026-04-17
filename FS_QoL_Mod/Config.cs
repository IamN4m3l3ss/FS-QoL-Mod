using MelonLoader;

namespace FS_FovChanger
{
    public static class Config
    {
        public static MelonPreferences_Category ModCategory;
        
        // Visuals
        public static MelonPreferences_Entry<bool> HideWeapons;
        public static MelonPreferences_Entry<bool> HideTurret;
        public static MelonPreferences_Entry<bool> ShowHudExtras;
        
        // HUD
        public static MelonPreferences_Entry<bool> ShowAmmoCircles;
        public static MelonPreferences_Entry<bool> ShowMovementBars;
        public static MelonPreferences_Entry<bool> ShowSpeedometer;
        
        // Mechanics
        public static MelonPreferences_Entry<bool> EnableBhop;
        public static MelonPreferences_Entry<bool> EnableDoubleJump;
        public static MelonPreferences_Entry<int> DoubleJumpCharges;
        public static MelonPreferences_Entry<bool> EnableWallRun;
        public static MelonPreferences_Entry<int> WallrunCharges;
        public static MelonPreferences_Entry<bool> DisableStamina;

        // Crosshair
        public static MelonPreferences_Entry<bool> CrosshairEnabled;
        public static MelonPreferences_Entry<float> CrosshairSize;
        public static MelonPreferences_Entry<float> CrosshairThickness;
        public static MelonPreferences_Entry<float> CrosshairGap;
        public static MelonPreferences_Entry<string> CrosshairColor;
        public static MelonPreferences_Entry<float> CrosshairRotation;
        public static MelonPreferences_Entry<bool> CrosshairBarTop;
        public static MelonPreferences_Entry<bool> CrosshairBarBottom;
        public static MelonPreferences_Entry<bool> CrosshairBarLeft;
        public static MelonPreferences_Entry<bool> CrosshairBarRight;

        public static bool showMenu = false;

        public static void Initialize()
        {
            ModCategory = MelonPreferences.CreateCategory("FS_QoL_Mod");
            
            HideWeapons = ModCategory.CreateEntry("HideWeapons", false);
            HideTurret = ModCategory.CreateEntry("HideTurret", false);
            ShowHudExtras = ModCategory.CreateEntry("ShowHudExtras", true);
            
            ShowAmmoCircles = ModCategory.CreateEntry("ShowAmmoCircles", true);
            ShowMovementBars = ModCategory.CreateEntry("ShowMovementBars", true);
            ShowSpeedometer = ModCategory.CreateEntry("ShowSpeedometer", true);
            
            EnableBhop = ModCategory.CreateEntry("EnableBhop", false);
            EnableDoubleJump = ModCategory.CreateEntry("EnableDoubleJump", false);
            DoubleJumpCharges = ModCategory.CreateEntry("DoubleJumpCharges", 1);
            EnableWallRun = ModCategory.CreateEntry("EnableWallRun", false);
            WallrunCharges = ModCategory.CreateEntry("WallrunCharges", 2);
            DisableStamina = ModCategory.CreateEntry("DisableStamina", false);

            CrosshairEnabled = ModCategory.CreateEntry("CrosshairEnabled", false);
            CrosshairSize = ModCategory.CreateEntry("CrosshairSize", 20f);
            CrosshairThickness = ModCategory.CreateEntry("CrosshairThickness", 2f);
            CrosshairGap = ModCategory.CreateEntry("CrosshairGap", 8f);
            CrosshairColor = ModCategory.CreateEntry("CrosshairColor", "#FFFFFFFF");
            CrosshairRotation = ModCategory.CreateEntry("CrosshairRotation", 0f);
            CrosshairBarTop = ModCategory.CreateEntry("CrosshairBarTop", true);
            CrosshairBarBottom = ModCategory.CreateEntry("CrosshairBarBottom", true);
            CrosshairBarLeft = ModCategory.CreateEntry("CrosshairBarLeft", true);
            CrosshairBarRight = ModCategory.CreateEntry("CrosshairBarRight", true);
        }
    }
}