using UnityEngine;
using MelonLoader;

namespace FS_FovChanger.UI
{
    public static class ModGUI 
    {
        private static bool _isUIInitialized;
        private static int _selectedTab;
        private static readonly string[] Tabs = { "Visuals", "HUD", "Movement", "Crosshair" };

        private static Rect _windowRect = new Rect(20, 20, 450, 520); 
        
        private static Color _crosshairColor;
        private static string _crosshairHex;

        private static GUIStyle _windowStyle, _toggleStyle, _sliderStyle, _sliderThumbStyle, 
                                _textFieldStyle, _toolbarStyle, _labelStyle, _headerLabelStyle;

        public static void Draw()
        {
            if (!Config.showMenu) return;

            if (!_isUIInitialized || _windowStyle == null || _windowStyle.normal.background == null)
            {
                Initialize();
                _isUIInitialized = true;
            }

            GUI.skin.window = _windowStyle;
            _windowRect = GUI.Window(0, _windowRect, WindowFunction, "<b>FS QoL Mod</b>");
        }
        
        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            Object.DontDestroyOnLoad(result);
            return result;
        }

        private static void Initialize()
        {
            if (ColorUtility.TryParseHtmlString(Config.CrosshairColor.Value, out _crosshairColor))
                _crosshairHex = Config.CrosshairColor.Value;
            else {
                _crosshairColor = Color.white;
                _crosshairHex = "#FFFFFFFF";
            }

            Color baseColor = new Color(0.12f, 0.12f, 0.16f, 0.95f);
            Color mantleColor = new Color(0.11f, 0.11f, 0.15f, 1f);
            Color crustColor = new Color(0.10f, 0.10f, 0.13f, 1f);
            Color textColor = new Color(0.80f, 0.82f, 0.88f, 1f);
            Color subtext0Color = new Color(0.65f, 0.67f, 0.73f, 1f);
            Color blueColor = new Color(0.54f, 0.71f, 0.93f, 1f);
            Color lavenderColor = new Color(0.71f, 0.72f, 0.91f, 1f);

            _windowStyle = new GUIStyle(GUI.skin.window) {
                normal = { background = MakeTex(2, 2, baseColor), textColor = textColor },
                onNormal = { background = MakeTex(2, 2, baseColor), textColor = textColor },
                padding = new RectOffset(10, 10, 25, 10), border = new RectOffset(2, 2, 2, 2), fontStyle = FontStyle.Bold
            };
            _toggleStyle = new GUIStyle(GUI.skin.label) {
                normal = { textColor = textColor }, hover = { textColor = lavenderColor },
                padding = new RectOffset(20, 0, 0, 0), alignment = TextAnchor.MiddleLeft, fixedHeight = 18
            };
            _sliderStyle = new GUIStyle(GUI.skin.horizontalSlider) { normal = { background = MakeTex(2, 2, crustColor) }, fixedHeight = 8 };
            _sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb) {
                normal = { background = MakeTex(2, 2, blueColor) }, hover = { background = MakeTex(2, 2, lavenderColor) },
                active = { background = MakeTex(2, 2, lavenderColor) }, fixedHeight = 14, fixedWidth = 14
            };
            _textFieldStyle = new GUIStyle(GUI.skin.textField) {
                normal = { background = MakeTex(2, 2, mantleColor), textColor = textColor },
                focused = { background = MakeTex(2, 2, crustColor), textColor = lavenderColor }, padding = new RectOffset(4, 4, 4, 4)
            };
            _toolbarStyle = new GUIStyle(GUI.skin.button) {
                normal = { background = MakeTex(2, 2, baseColor), textColor = subtext0Color },
                onNormal = { background = MakeTex(2, 2, mantleColor), textColor = blueColor },
                hover = { background = MakeTex(2, 2, mantleColor), textColor = lavenderColor },
                onHover = { background = MakeTex(2, 2, mantleColor), textColor = lavenderColor },
                active = { background = MakeTex(2, 2, mantleColor), textColor = blueColor },
                onActive = { background = MakeTex(2, 2, mantleColor), textColor = blueColor },
                padding = new RectOffset(5, 5, 5, 5), fontStyle = FontStyle.Bold, fontSize = 12
            };
            _labelStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = textColor }, richText = true };
            _headerLabelStyle = new GUIStyle(_labelStyle) { fontStyle = FontStyle.Bold };
        }

        private static void WindowFunction(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            GUILayout.Space(5);
            _selectedTab = GUILayout.Toolbar(_selectedTab, Tabs, _toolbarStyle, GUILayout.Height(25));
            GUILayout.Space(15);
            DrawTabContent();
        }

        private static void DrawTabContent()
        {
            switch (_selectedTab) {
                case 0: DrawVisualsTab(); break;
                case 1: DrawHudTab(); break;
                case 2: DrawMovementTab(); break;
                case 3: DrawCrosshairTab(); break;
            }
        }
        
        private static bool CustomToggle(Rect r, bool value, string text)
        {
            bool wasClicked = GUI.Button(r, text, _toggleStyle);
            Rect boxRect = new Rect(r.x, r.y + 3, 12, 12);
            GUI.color = value ? new Color(0.54f, 0.71f, 0.93f, 1f) : new Color(0.11f, 0.11f, 0.15f, 1f);
            GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            return wasClicked ? !value : value;
        }

        private static void DrawVisualsTab()
        {
            var r1 = GUILayoutUtility.GetRect(new GUIContent(" Hide Weapon Models"), _toggleStyle);
            bool hideWeapons = CustomToggle(r1, Config.HideWeapons.Value, " Hide Weapon Models");
            if (hideWeapons != Config.HideWeapons.Value) {
                Config.HideWeapons.Value = hideWeapons;
                ApplyVisibility();
            }
            
            var r2 = GUILayoutUtility.GetRect(new GUIContent(" Hide Flying Turret"), _toggleStyle);
            bool hideTurret = CustomToggle(r2, Config.HideTurret.Value, " Hide Flying Turret");
            if (hideTurret != Config.HideTurret.Value) {
                Config.HideTurret.Value = hideTurret;
                ApplyVisibility();
            }
        }

        private static void DrawHudTab()
        {
            var r1 = GUILayoutUtility.GetRect(new GUIContent(" Master HUD Switch"), _toggleStyle);
            Config.ShowHudExtras.Value = CustomToggle(r1, Config.ShowHudExtras.Value, " Master HUD Switch");
            
            if (Config.ShowHudExtras.Value) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                
                var r2 = GUILayoutUtility.GetRect(new GUIContent(" Ammo Arcs (Crosshair)"), _toggleStyle);
                Config.ShowAmmoCircles.Value = CustomToggle(r2, Config.ShowAmmoCircles.Value, " Ammo Arcs (Crosshair)");
                
                var r3 = GUILayoutUtility.GetRect(new GUIContent(" Movement Cooldowns"), _toggleStyle);
                Config.ShowMovementBars.Value = CustomToggle(r3, Config.ShowMovementBars.Value, " Movement Cooldowns");
                
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        private static void DrawMovementTab()
        {
            var r1 = GUILayoutUtility.GetRect(new GUIContent(" Enable Double Jump"), _toggleStyle);
            Config.EnableDoubleJump.Value = CustomToggle(r1, Config.EnableDoubleJump.Value, " Enable Double Jump");
            
            if (Config.EnableDoubleJump.Value) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                var r2 = GUILayoutUtility.GetRect(new GUIContent(" 2 Charges"), _toggleStyle);
                Config.DoubleJumpCharges.Value = CustomToggle(r2, Config.DoubleJumpCharges.Value == 2, " 2 Charges") ? 2 : 1;
                GUILayout.EndHorizontal();
            }

            var r3 = GUILayoutUtility.GetRect(new GUIContent(" Enable Wall Running"), _toggleStyle);
            Config.EnableWallRun.Value = CustomToggle(r3, Config.EnableWallRun.Value, " Enable Wall Running");
            
            if (Config.EnableWallRun.Value) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                var r4 = GUILayoutUtility.GetRect(new GUIContent(" 3 Charges"), _toggleStyle);
                Config.WallrunCharges.Value = CustomToggle(r4, Config.WallrunCharges.Value == 3, " 3 Charges") ? 3 : 2;
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(10);
            var r5 = GUILayoutUtility.GetRect(new GUIContent(" Disable Stamina"), _toggleStyle);
            Config.DisableStamina.Value = CustomToggle(r5, Config.DisableStamina.Value, " Disable Stamina");
            
            GUILayout.Space(15);
            GUILayout.Label("<color=#6C7086><size=11>Note: Movement mechanics may cause jitter\non some complex forest terrains.</size></color>", _labelStyle);
        }

        private static void DrawCrosshairTab()
        {
            var r1 = GUILayoutUtility.GetRect(new GUIContent(" Enable Custom Crosshair"), _toggleStyle);
            Config.CrosshairEnabled.Value = CustomToggle(r1, Config.CrosshairEnabled.Value, " Enable Custom Crosshair");
            
            if (Config.CrosshairEnabled.Value) {
                Config.CrosshairSize.Value = DrawSliderRow("Size", Config.CrosshairSize.Value, 1f, 100f);
                Config.CrosshairThickness.Value = DrawSliderRow("Thickness", Config.CrosshairThickness.Value, 1f, 50f);
                Config.CrosshairGap.Value = DrawSliderRow("Gap", Config.CrosshairGap.Value, 0f, 100f);
                Config.CrosshairRotation.Value = DrawSliderRow("Rotation", Config.CrosshairRotation.Value, 0f, 360f);
                GUILayout.Space(15);
                GUILayout.Label("Color Picker", _headerLabelStyle);
                _crosshairColor.r = DrawSliderRow("R", _crosshairColor.r, 0f, 1f, "F2");
                _crosshairColor.g = DrawSliderRow("G", _crosshairColor.g, 0f, 1f, "F2");
                _crosshairColor.b = DrawSliderRow("B", _crosshairColor.b, 0f, 1f, "F2");
                _crosshairColor.a = DrawSliderRow("A", _crosshairColor.a, 0f, 1f, "F2");
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Hex: ", _labelStyle, GUILayout.Width(40));
                Rect previewRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                GUI.color = _crosshairColor;
                GUI.DrawTexture(previewRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUILayout.Space(10);
                string newHex = GUILayout.TextField(_crosshairHex, _textFieldStyle, GUILayout.Width(100));
                if (newHex != _crosshairHex) {
                    _crosshairHex = newHex;
                    if (ColorUtility.TryParseHtmlString(_crosshairHex, out var newColor)) _crosshairColor = newColor;
                }
                string updatedHex = "#" + ColorUtility.ToHtmlStringRGBA(_crosshairColor);
                if (updatedHex != _crosshairHex) {
                    _crosshairHex = updatedHex;
                    Config.CrosshairColor.Value = _crosshairHex;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(15);
                GUILayout.Label("Visible Bars", _headerLabelStyle);

                // --- FIX: Use precise Rects for toggle layout ---
                Rect row1 = GUILayoutUtility.GetRect(1, 20);
                Config.CrosshairBarTop.Value = CustomToggle(new Rect(row1.x, row1.y, 100, 20), Config.CrosshairBarTop.Value, " Top");
                Config.CrosshairBarBottom.Value = CustomToggle(new Rect(row1.x + 110, row1.y, 100, 20), Config.CrosshairBarBottom.Value, " Bottom");
                
                Rect row2 = GUILayoutUtility.GetRect(1, 20);
                Config.CrosshairBarLeft.Value = CustomToggle(new Rect(row2.x, row2.y, 100, 20), Config.CrosshairBarLeft.Value, " Left");
                Config.CrosshairBarRight.Value = CustomToggle(new Rect(row2.x + 110, row2.y, 100, 20), Config.CrosshairBarRight.Value, " Right");
            }
        }

        private static float DrawSliderRow(string label, float value, float min, float max, string format = "F1")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _labelStyle, GUILayout.Width(70));
            GUILayout.BeginVertical();
            GUILayout.Space(4);
            float sliderVal = GUILayout.HorizontalSlider(value, min, max, _sliderStyle, _sliderThumbStyle, GUILayout.Width(200));
            GUILayout.EndVertical();
            GUILayout.Space(10);
            string textVal = GUILayout.TextField(sliderVal.ToString(format), _textFieldStyle, GUILayout.Width(60));
            if (float.TryParse(textVal, out float parsed)) sliderVal = Mathf.Clamp(parsed, min, max);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            return sliderVal;
        }

        public static void ApplyVisibility()
        {
            var camera = Camera.main;
            if (camera == null) return;
            for (int i = 0; i < camera.transform.childCount; i++) {
                Transform child = camera.transform.GetChild(i);
                if (child.name.Contains("UI") || child.name.Contains("Canvas")) continue;
                var renderers = child.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers) r.enabled = !Config.HideWeapons.Value;
            }
            var rootPlayer = camera.transform.root;
            var turret = FindRecursive(rootPlayer, "Flyingturret(Clone)");
            if (turret != null) {
                var renderers = turret.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers) r.enabled = !Config.HideTurret.Value;
            }
        }

        private static Transform FindRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent) {
                var result = FindRecursive(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}