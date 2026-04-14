using UnityEngine;
using HarmonyLib;
using System.Globalization;
using System.Collections.Generic;

namespace FS_FovChanger.UI
{
    public static class ModGUI 
    {
        private static string _fovInputText = "";
        private static string _sensInputText = "";
        private static bool _isUIInitialized;

        private static int _selectedTab = 0;
        private static readonly string[] _tabs = { "Camera", "Visuals", "HUD", "Movement" };

        private static readonly Dictionary<int, Vector3> _originalPoiPositions = new Dictionary<int, Vector3>();

        public static void Draw()
        {
            if (!Config.showMenu) return;

            if (!_isUIInitialized)
            {
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f); 
                GUI.contentColor = Color.white; 
                _fovInputText = Mathf.Round(Config.SavedFov.Value).ToString(CultureInfo.InvariantCulture);
                _sensInputText = System.Math.Round(Config.SavedSens.Value, 2).ToString(CultureInfo.InvariantCulture);
                _isUIInitialized = true;
            }

            // Expanded height slightly to fit more toggles
            GUI.Box(new Rect(20, 20, 380, 280), "<b><size=16>FS QoL Mod Menu</size></b>");
            _selectedTab = GUI.Toolbar(new Rect(40, 55, 340, 30), _selectedTab, _tabs);
            DrawTabContent();
        }

        private static void DrawTabContent()
        {
            float startY = 105f; 

            switch (_selectedTab)
            {
                case 0: // CAMERA
                    GUI.Label(new Rect(40, startY, 100, 20), "FOV:");
                    float fovSlider = GUI.HorizontalSlider(new Rect(40, startY + 25f, 200, 20), Config.SavedFov.Value, 30f, 150f);
                    if (!Mathf.Approximately(fovSlider, Config.SavedFov.Value))
                    {
                        Config.SavedFov.Value = fovSlider;
                        _fovInputText = Mathf.Round(fovSlider).ToString(CultureInfo.InvariantCulture); 
                    }

                    _fovInputText = GUI.TextField(new Rect(250, startY + 20f, 50, 20), _fovInputText);
                    if (GUI.changed && float.TryParse(_fovInputText, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedFov))
                    {
                        Config.SavedFov.Value = Mathf.Clamp(parsedFov, 10f, 180f); 
                    }

                    GUI.Label(new Rect(40, startY + 60f, 100, 20), "Sensitivity:");
                    float sensSlider = GUI.HorizontalSlider(new Rect(40, startY + 85f, 200, 20), Config.SavedSens.Value, 0.1f, 20f);
                    if (!Mathf.Approximately(sensSlider, Config.SavedSens.Value))
                    {
                        Config.SavedSens.Value = sensSlider;
                        _sensInputText = System.Math.Round(sensSlider, 2).ToString(CultureInfo.InvariantCulture);
                    }

                    _sensInputText = GUI.TextField(new Rect(250, startY + 80f, 50, 20), _sensInputText);
                    if (GUI.changed && float.TryParse(_sensInputText, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedSens))
                    {
                        Config.SavedSens.Value = Mathf.Clamp(parsedSens, 0.01f, 100f);
                    }
                    break;

                case 1: // VISUALS
                    Config.HideWeapons.Value = GUI.Toggle(new Rect(40, startY, 250, 20), Config.HideWeapons.Value, " Hide Weapon Models");
                    Config.HideTurret.Value = GUI.Toggle(new Rect(40, startY + 30f, 250, 20), Config.HideTurret.Value, " Hide Flying Turret");
                    
                    bool beamToggle = GUI.Toggle(new Rect(40, startY + 60f, 250, 20), Config.ShowPoiBeam.Value, " Show Flag Sky-Beams");
                    if (beamToggle != Config.ShowPoiBeam.Value)
                    {
                        Config.ShowPoiBeam.Value = beamToggle;
                        UpdatePoiBeams();
                    }
                    
                    if (GUI.changed) ApplyVisibility();
                    break;

                case 2: // HUD (Independent Toggles)
                    Config.ShowHudExtras.Value = GUI.Toggle(new Rect(40, startY, 250, 20), Config.ShowHudExtras.Value, " Master HUD Switch");
                    
                    // Only show sub-options if the master switch is ON
                    if (Config.ShowHudExtras.Value)
                    {
                        Config.ShowCompass.Value = GUI.Toggle(new Rect(60, startY + 30f, 230, 20), Config.ShowCompass.Value, " Flag Compass (Top Bar)");
                        Config.ShowAmmoCircles.Value = GUI.Toggle(new Rect(60, startY + 60f, 230, 20), Config.ShowAmmoCircles.Value, " Ammo Arcs (Crosshair)");
                        Config.ShowMovementBars.Value = GUI.Toggle(new Rect(60, startY + 90f, 230, 20), Config.ShowMovementBars.Value, " Movement Cooldowns");
                    }
                    break;
                
                case 3: // MOVEMENT
                    Config.EnableDoubleJump.Value = GUI.Toggle(new Rect(40, startY, 250, 20), Config.EnableDoubleJump.Value, " Enable Double Jump");
                    Config.EnableWallRun.Value = GUI.Toggle(new Rect(40, startY + 30f, 250, 20), Config.EnableWallRun.Value, " Enable Wall Running");
                    
                    GUI.Label(new Rect(40, startY + 65f, 300, 40), "<color=grey><size=10>Note: Movement mechanics may cause jitter\non some complex forest terrains.</size></color>");
                    break;
            }
        }

        private static void ApplyFov()
        {
            var player = Object.FindAnyObjectByType<FPSController>();
            if (player != null) Traverse.Create(player).Field("originalFOV").SetValue(Config.SavedFov.Value);
        }

        public static void ApplyVisibility()
        {
            var camera = Camera.main;
            if (camera == null) return;

            for (int i = 0; i < camera.transform.childCount; i++)
            {
                Transform child = camera.transform.GetChild(i);
                if (child.name.Contains("UI") || child.name.Contains("Canvas")) continue;

                Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renderers) r.enabled = !Config.HideWeapons.Value; 
            }

            Transform rootPlayer = camera.transform.root; 
            Transform turret = FindRecursive(rootPlayer, "Flyingturret(Clone)");
            if (turret != null)
            {
                Renderer[] renderers = turret.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renderers) r.enabled = !Config.HideTurret.Value;
            }
        }

        public static void UpdatePoiBeams()
        {
            GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in objects)
            {
                if (obj.name == "POI_intrestpointindicator")
                {
                    int id = obj.GetInstanceID();
                    if (!_originalPoiPositions.ContainsKey(id)) _originalPoiPositions.Add(id, obj.transform.localPosition);

                    if (Config.ShowPoiBeam.Value)
                    {
                        obj.transform.localScale = new Vector3(0.5f, 500f, 0.5f);
                        obj.transform.localPosition = new Vector3(0, 250f, 0);
                    }
                    else
                    {
                        obj.transform.localScale = Vector3.one;
                        obj.transform.localPosition = _originalPoiPositions[id];
                    }
                }
            }
        }

        private static Transform FindRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform result = FindRecursive(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}