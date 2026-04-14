using UnityEngine;
using HarmonyLib;
using FS_FovChanger.Mechanics;
using System.Collections.Generic;

namespace FS_FovChanger.UI
{
    public static class Hud
    {
        private static Texture2D _barTex;
        
        // Caching references
        private static FPSController _player;
        private static DualWieldGun _dualGun;
        private static ProjectileGun _singleGun;
        
        private static float _nextSearchTime;
        // Search specifically for the 'Flag' script from Assembly-CSharp
        private static readonly List<Flag> _flagCache = new List<Flag>();
        
        private static GUIStyle _cardinalStyle;
        private static GUIStyle _degreeStyle;
        private static GUIStyle _flagStyle;

        public static void Draw()
        {
            if (!Config.ShowHudExtras.Value) return;

            if (_player == null)
            {
                _player = Object.FindAnyObjectByType<FPSController>();
                if (_player == null) return;
            }

            if (_barTex == null) _barTex = MakeTex(2, 2, Color.white);
            InitStyles();

            // Refresh cache every 2 seconds - searching for 'Flag' is extremely fast
            if (Time.time > _nextSearchTime)
            {
                RefreshCache();
                _nextSearchTime = Time.time + 2f;
            }

            if (Config.ShowCompass.Value) DrawCompass(_player);

            if (Config.ShowAmmoCircles.Value)
            {
                if (_dualGun != null && _dualGun.gameObject.activeInHierarchy) 
                    DrawDualAmmo(_dualGun);
                else if (_singleGun != null && _singleGun.gameObject.activeInHierarchy) 
                    DrawSingleAmmo(_singleGun);
            }

            if (Config.ShowMovementBars.Value)
            {
                DrawHorizontalCooldowns(_player);
                if (Config.EnableDoubleJump.Value) DrawDoubleJumpIndicator();
                if (Config.EnableWallRun.Value)
                {
                    DrawWallRunIndicator();
                    // Draw wallrun charges if actively wallrunning or if charges are not full
                    if (AdvancedMovementPatch.IsWallRunning || AdvancedMovementPatch.WallRunsRemaining < 2) 
                    {
                        DrawWallRunCharges();
                    }
                }
            }
        }

        private static void InitStyles()
        {
            if (_cardinalStyle != null) return;
            _cardinalStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.9f, 0.9f, 0.9f) } };
            _degreeStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } };
            _flagStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Color.red } };
        }

        private static void RefreshCache()
        {
            _dualGun = Object.FindAnyObjectByType<DualWieldGun>();
            _singleGun = Object.FindAnyObjectByType<ProjectileGun>();

            _flagCache.Clear();
            // This searches for the COMPONENT, not the GameObject name. Way faster!
            Flag[] flagsInScene = Object.FindObjectsByType<Flag>(FindObjectsSortMode.None);
            if (flagsInScene != null) _flagCache.AddRange(flagsInScene);
        }

        private static void DrawCompass(FPSController player)
        {
            float cx = Screen.width / 2f;
            float cy = 30f; 
            float width = 400f; 
            float halfW = width / 2f;

            GUI.color = new Color(0, 0, 0, 0.4f);
            GUI.DrawTexture(new Rect(cx - halfW, cy - 10f, width, 30f), _barTex);
            GUI.color = Color.white;

            float playerAngle = player.transform.eulerAngles.y;

            // Degree Markers
            for (int i = 0; i < 360; i += 15)
            {
                float diff = Mathf.DeltaAngle(playerAngle, i);
                if (Mathf.Abs(diff) <= 90f) 
                {
                    float pos = cx + (diff / 90f) * halfW;
                    string label = i.ToString();
                    GUIStyle style = _degreeStyle;
                    if (i == 0) { label = "N"; style = _cardinalStyle; }
                    else if (i == 90) { label = "E"; style = _cardinalStyle; }
                    else if (i == 180) { label = "S"; style = _cardinalStyle; }
                    else if (i == 270) { label = "W"; style = _cardinalStyle; }

                    GUI.Label(new Rect(pos - 15f, cy - 10f, 30f, 20f), label, style);
                }
            }

            // Flag Markers
            for (int i = _flagCache.Count - 1; i >= 0; i--)
            {
                Flag f = _flagCache[i];
                
                // If the flag component is destroyed or disabled, remove and skip
                if (f == null || !f.enabled || !f.gameObject.activeInHierarchy)
                {
                    _flagCache.RemoveAt(i);
                    continue;
                }

                Vector3 dirToFlag = (f.transform.position - player.transform.position).normalized;
                float diff = Vector3.SignedAngle(player.transform.forward, dirToFlag, Vector3.up);

                if (Mathf.Abs(diff) <= 90f)
                {
                    float pos = cx + (diff / 90f) * halfW;
                    GUI.Label(new Rect(pos - 10f, cy + 5f, 20f, 20f), "▼", _flagStyle);
                }
            }
        }

        // -- AMMO AND COOLDOWNS (Fixed math and caching) --

        private static void DrawHorizontalCooldowns(FPSController player)
        {
            var tr = Traverse.Create(player);
            float dashCd = (float)tr.Field("dashCooldownRemaining").GetValue();
            float wallCd = (float)tr.Field("wallJumpCooldownRemaining").GetValue();

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            float barWidth = 60f;

            if (dashCd > 0)
            {
                float fill = Mathf.Clamp01(dashCd / player.dashCooldown);
                GUI.color = new Color(0, 1, 1, 0.8f);
                GUI.DrawTexture(new Rect(centerX - (barWidth / 2f), centerY + 55f, barWidth * fill, 4f), _barTex);
            }

            if (wallCd > 0)
            {
                float fill = Mathf.Clamp01(wallCd / player.wallJumpCooldown);
                GUI.color = new Color(1, 0.9f, 0, 0.8f);
                GUI.DrawTexture(new Rect(centerX - (barWidth / 2f), centerY + 63f, barWidth * fill, 4f), _barTex);
            }
            GUI.color = Color.white;
        }

        private static void DrawDoubleJumpIndicator()
        {
            if (!AdvancedMovementPatch.isGrounded && !AdvancedMovementPatch.canDoubleJump)
            {
                float centerX = Screen.width / 2f;
                float centerY = Screen.height / 2f;
                GUI.color = new Color(0.8f, 0.2f, 0.2f, 0.6f); 
                GUI.DrawTexture(new Rect(centerX - 30f, centerY + 71f, 60f, 3f), _barTex);
                GUI.color = Color.white;
            }
        }
        
        private static void DrawWallRunIndicator()
        {
            if (!AdvancedMovementPatch.isGrounded && AdvancedMovementPatch.wallRunTimer < AdvancedMovementPatch.MaxWallRunTime)
            {
                float centerX = Screen.width / 2f;
                float centerY = Screen.height / 2f;
                float fill = Mathf.Clamp01(AdvancedMovementPatch.wallRunTimer / AdvancedMovementPatch.MaxWallRunTime);
                GUI.color = new Color(0.7f, 0.2f, 0.9f, 0.8f); 
                GUI.DrawTexture(new Rect(centerX - 30f, centerY + 79f, 60f * fill, 3f), _barTex);
                GUI.color = Color.white;
            }
        }

        private static void DrawWallRunCharges()
        {
            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            float barWidth = 60f;
            float barHeight = 3f;
            float spacing = 5f;

            int maxCharges = 2; // Assuming 2 is the max wallrun charges
            for (int i = 0; i < maxCharges; i++)
            {
                float xPos = centerX - (barWidth / 2f) + (i * (barWidth / maxCharges + spacing));
                float yPos = centerY + 87f; // Adjust Y position as needed

                if (i < AdvancedMovementPatch.WallRunsRemaining)
                {
                    GUI.color = new Color(0.2f, 0.9f, 0.7f, 0.8f); // Color for remaining charges
                }
                else
                {
                    GUI.color = new Color(0.4f, 0.4f, 0.4f, 0.6f); // Color for used charges
                }
                GUI.DrawTexture(new Rect(xPos, yPos, barWidth / maxCharges, barHeight), _barTex);
            }
            GUI.color = Color.white;
        }

        private static void DrawDualAmmo(DualWieldGun gun)
        {
            var tr = Traverse.Create(gun);
            float rTime = (float)tr.Field("reloadTime").GetValue();
            int magSize = (int)tr.Field("magazineSize").GetValue();
            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;

            float reload1 = Time.time - (float)tr.Field("reloadStartTimeGun1").GetValue();
            DrawArc(cx, cy, 30f, (reload1 < rTime) ? (reload1 / rTime) : ((int)tr.Field("bulletsLeftGun1").GetValue() / (float)magSize), (reload1 < rTime) ? Color.red : Color.white, true);

            float reload2 = Time.time - (float)tr.Field("reloadStartTimeGun2").GetValue();
            DrawArc(cx, cy, 30f, (reload2 < rTime) ? (reload2 / rTime) : ((int)tr.Field("bulletsLeftGun2").GetValue() / (float)magSize), (reload2 < rTime) ? Color.red : Color.white, false);
        }

        private static void DrawSingleAmmo(ProjectileGun gun)
        {
            var tr = Traverse.Create(gun);
            float rTime = (float)tr.Field("reloadTime").GetValue();
            float reload = Time.time - (float)tr.Field("reloadStartTime").GetValue();
            int magSize = (int)tr.Field("magazineSize").GetValue();

            if (reload < rTime) DrawFullArc(Screen.width / 2f, Screen.height / 2f, 30f, reload / rTime, Color.red);
            else DrawFullArc(Screen.width / 2f, Screen.height / 2f, 30f, (int)tr.Field("bulletsLeft").GetValue() / (float)magSize, Color.white);
        }

        private static void DrawFullArc(float x, float y, float radius, float fill, Color col)
        {
            GUI.color = col;
            int segments = Mathf.RoundToInt(160 * Mathf.Clamp01(fill));
            for (int i = 160 - segments; i < 160; i++)
            {
                float rad = i * 2.25f * Mathf.Deg2Rad;
                GUI.DrawTexture(new Rect(x + Mathf.Sin(rad) * radius - 2f, y - Mathf.Cos(rad) * radius - 2f, 4f, 4f), _barTex);
            }
            GUI.color = Color.white;
        }

        private static void DrawArc(float x, float y, float radius, float fill, Color col, bool isLeft)
        {
            GUI.color = col;
            int segments = Mathf.RoundToInt(80 * Mathf.Clamp01(fill));
            for (int i = 80 - segments; i < 80; i++)
            {
                float angle = isLeft ? (360f - (15f + i * 1.875f)) : (15f + i * 1.875f);
                float rad = angle * Mathf.Deg2Rad;
                GUI.DrawTexture(new Rect(x + Mathf.Sin(rad) * radius - 2f, y - Mathf.Cos(rad) * radius - 2f, 4f, 4f), _barTex);
            }
            GUI.color = Color.white;
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Texture2D result = new Texture2D(width, height);
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}