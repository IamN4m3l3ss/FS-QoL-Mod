using UnityEngine;
using HarmonyLib;
using FS_FovChanger.Mechanics;

namespace FS_FovChanger.UI
{
    public static class Hud
    {
        private static Texture2D _barTex;
        private static Texture2D _speedBgTex;
        
        public static FPSController Player;
        private static DualWieldGun _dualGun;
        private static ProjectileGun _singleGun;
        
        private static float _nextSearchTime;
        private static GUIStyle _speedometerStyle;
        private static GUIStyle _speedLabelStyle;
        private static Vector3 _lastPosition;
        private static float _currentSpeed;
        private static bool _wasBhopEnabled;

        public static void Draw()
        {
            if (!Config.ShowHudExtras.Value) return;

            if (Player == null)
            {
                Player = Object.FindAnyObjectByType<FPSController>();
                if (Player == null) return;
                _lastPosition = Player.transform.position;
            }

            if (_barTex == null) _barTex = MakeTex(2, 2, Color.white);
            if (_speedBgTex == null) _speedBgTex = MakeTex(2, 2, new Color(0.12f, 0.12f, 0.16f, 0.4f)); // Catppuccin Base (transparent)

            if (_speedometerStyle == null)
            {
                _speedometerStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 24,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.80f, 0.82f, 0.88f, 1f) } // Catppuccin Text
                };
            }
            if (_speedLabelStyle == null)
            {
                _speedLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.80f, 0.82f, 0.88f, 1.0f) }
                };
            }

            if (Time.time > _nextSearchTime)
            {
                RefreshCache();
                _nextSearchTime = Time.time + 2f;
            }

            if (Config.ShowAmmoCircles.Value)
            {
                DrawAmmoBars();
            }

            if (Config.ShowMovementBars.Value)
            {
                DrawHorizontalCooldowns(Player);
                if (Config.EnableDoubleJump.Value) DrawDoubleJumpIndicator();
                if (Config.EnableWallRun.Value)
                {
                    DrawWallRunIndicator();
                    if (AdvancedMovementPatch.IsWallRunning || AdvancedMovementPatch.WallRunsRemaining < Config.WallrunCharges.Value) 
                    {
                        DrawWallRunCharges();
                    }
                }
            }
            
            // Reset position tracking when toggling bhop so we don't get wild numbers
            if (Config.EnableBhop.Value != _wasBhopEnabled)
            {
                _lastPosition = Player.transform.position;
                _currentSpeed = 0f;
                _wasBhopEnabled = Config.EnableBhop.Value;
            }

            CalculateSpeedometer();
            if (Config.ShowSpeedometer.Value)
            {
                DrawSpeedometerUI();
            }
        }
        
        private static void RefreshCache()
        {
            _dualGun = Object.FindAnyObjectByType<DualWieldGun>();
            _singleGun = Object.FindAnyObjectByType<ProjectileGun>();
        }
        
        private static void CalculateSpeedometer()
        {
            if (Player == null) return;

            Vector3 currentPosition = Player.transform.position;
            
            if (Time.deltaTime > 0)
            {
                Vector3 delta = currentPosition - _lastPosition;
                delta.y = 0; // Horizontal speed only
                float speed = delta.magnitude / Time.deltaTime;
                
                // Prevent negative or absurdly high numbers on lag spikes/teleports
                if (speed < 0f || speed > 1000f) speed = 0f;
                
                _currentSpeed = Mathf.Lerp(_currentSpeed, speed, Time.deltaTime * 8f);
            }
            _lastPosition = currentPosition;
        }

        private static void DrawSpeedometerUI()
        {
            float centerX = Screen.width / 2f;
            float yPos = 40f; // Position it near the top

            float width = 100f;
            float height = 40f;
            Rect bgRect = new Rect(centerX - (width / 2f), yPos, width, height);
            
            // Draw background
            GUI.DrawTexture(bgRect, _speedBgTex);
            
            int displaySpeed = Mathf.RoundToInt(_currentSpeed);
            
            // Change color based on speed milestones
            Color speedColor = new Color(0.80f, 0.82f, 0.88f, 1f); // Default Text
            if (displaySpeed > 10) speedColor = new Color(0.95f, 0.82f, 0.61f, 1f); // Yellow
            if (displaySpeed > 20) speedColor = new Color(0.95f, 0.55f, 0.58f, 1f); // Red
            
            GUI.color = speedColor;
            GUI.Label(new Rect(centerX - (width / 2f), yPos, width, 25f), displaySpeed.ToString(), _speedometerStyle);
            GUI.color = Color.white;
            
            GUI.Label(new Rect(centerX - (width / 2f), yPos + 22f, width, 15f), "UNITS / SEC", _speedLabelStyle);
        }

        private static void DrawHorizontalCooldowns(FPSController player)
        {
            var tr = Traverse.Create(player);
            float dashCd = tr.Field<float>("dashCooldownRemaining").Value;

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            float barWidth = 60f;

            if (dashCd > 0)
            {
                float fill = Mathf.Clamp01(dashCd / player.dashCooldown);
                GUI.color = new Color(0.54f, 0.71f, 0.93f, 0.8f); // Catppuccin Blue
                GUI.DrawTexture(new Rect(centerX - (barWidth / 2f), centerY + 55f, barWidth * fill, 4f), _barTex);
            }

            if (!Config.EnableDoubleJump.Value)
            {
                float wallCd = tr.Field<float>("wallJumpCooldownRemaining").Value;
                if (wallCd > 0)
                {
                    float fill = Mathf.Clamp01(wallCd / player.wallJumpCooldown);
                    GUI.color = new Color(0.95f, 0.82f, 0.61f, 0.8f); // Catppuccin Yellow
                    GUI.DrawTexture(new Rect(centerX - (barWidth / 2f), centerY + 63f, barWidth * fill, 4f), _barTex);
                }
            }
            
            GUI.color = Color.white;
        }

        private static void DrawDoubleJumpIndicator()
        {
            int maxCharges = Config.DoubleJumpCharges.Value;
            if (AdvancedMovementPatch.isGrounded || AdvancedMovementPatch._jumpsPerformedInAir == 0) return;

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            float totalWidth = 60f;
            float barHeight = 3f;
            float spacing = 4f;
            float yPos = centerY + 63f;

            if (maxCharges == 1)
            {
                GUI.color = new Color(0.95f, 0.55f, 0.58f, 0.8f); // Catppuccin Red
                GUI.DrawTexture(new Rect(centerX - (totalWidth / 2f), yPos, totalWidth, barHeight), _barTex);
                GUI.color = Color.white;
                return;
            }

            float segmentWidth = (totalWidth - (spacing * (maxCharges - 1))) / maxCharges;

            for (int i = 0; i < maxCharges; i++)
            {
                float xPos = centerX - (totalWidth / 2f) + (i * (segmentWidth + spacing));

                GUI.color = i < AdvancedMovementPatch._jumpsPerformedInAir
                    ? new Color(0.95f, 0.55f, 0.58f, 0.8f) // Catppuccin Red
                    : new Color(0.24f, 0.24f, 0.33f, 0.6f); // Catppuccin Surface1
                
                GUI.DrawTexture(new Rect(xPos, yPos, segmentWidth, barHeight), _barTex);
            }
            GUI.color = Color.white;
        }
        
        private static void DrawWallRunIndicator()
        {
            if (!AdvancedMovementPatch.isGrounded && AdvancedMovementPatch.wallRunTimer < AdvancedMovementPatch.MaxWallRunTime)
            {
                float centerX = Screen.width / 2f;
                float centerY = Screen.height / 2f;
                float fill = Mathf.Clamp01(AdvancedMovementPatch.wallRunTimer / AdvancedMovementPatch.MaxWallRunTime);
                GUI.color = new Color(0.71f, 0.72f, 0.91f, 0.8f); // Catppuccin Lavender
                GUI.DrawTexture(new Rect(centerX - 30f, centerY + 71f, 60f * fill, 3f), _barTex);
            }
        }

        private static void DrawWallRunCharges()
        {
            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            float totalWidth = 60f;
            float barHeight = 3f;
            float spacing = 4f;
            float yPos = centerY + 79f;

            int maxCharges = Config.WallrunCharges.Value;
            float segmentWidth = (totalWidth - (spacing * (maxCharges - 1))) / maxCharges;

            for (int i = 0; i < maxCharges; i++)
            {
                float xPos = centerX - (totalWidth / 2f) + (i * (segmentWidth + spacing));

                GUI.color = i < AdvancedMovementPatch.WallRunsRemaining 
                    ? new Color(0.62f, 0.88f, 0.59f, 0.8f) // Catppuccin Green
                    : new Color(0.24f, 0.24f, 0.33f, 0.6f); // Catppuccin Surface1
                
                GUI.DrawTexture(new Rect(xPos, yPos, segmentWidth, barHeight), _barTex);
            }
            GUI.color = Color.white;
        }

        private static void DrawAmmoBars()
        {
            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            float radius = 30f;
            int segments = 40;
            
            Color blue = new Color(0.54f, 0.71f, 0.93f, 1f);
            Color red = new Color(0.95f, 0.55f, 0.58f, 1f);
            Color baseColor = new Color(0.12f, 0.12f, 0.16f, 0.8f);

            if (_dualGun != null && _dualGun.gameObject.activeInHierarchy)
            {
                var tr = Traverse.Create(_dualGun);
                float rTime = tr.Field<float>("reloadTime").Value;
                int magSize = tr.Field<int>("magazineSize").Value;

                float reload1 = Time.time - tr.Field<float>("reloadStartTimeGun1").Value;
                bool isReloading1 = reload1 < rTime;
                float fill1 = isReloading1 ? (reload1 / rTime) : (tr.Field<int>("bulletsLeftGun1").Value / (float)magSize);
                DrawCurvedBar(centerX, centerY, radius, segments, fill1, isReloading1 ? red : blue, baseColor, true);

                float reload2 = Time.time - tr.Field<float>("reloadStartTimeGun2").Value;
                bool isReloading2 = reload2 < rTime;
                float fill2 = isReloading2 ? (reload2 / rTime) : (tr.Field<int>("bulletsLeftGun2").Value / (float)magSize);
                DrawCurvedBar(centerX, centerY, radius, segments, fill2, isReloading2 ? red : blue, baseColor, false);
            }
            else if (_singleGun != null && _singleGun.gameObject.activeInHierarchy)
            {
                var tr = Traverse.Create(_singleGun);
                float rTime = tr.Field<float>("reloadTime").Value;
                float reload = Time.time - tr.Field<float>("reloadStartTime").Value;
                int magSize = tr.Field<int>("magazineSize").Value;

                bool isReloading = reload < rTime;
                float fill = isReloading ? (reload / rTime) : (tr.Field<int>("bulletsLeft").Value / (float)magSize);
                
                DrawCurvedBar(centerX, centerY, radius, segments, fill, isReloading ? red : blue, baseColor, true);
                DrawCurvedBar(centerX, centerY, radius, segments, fill, isReloading ? red : blue, baseColor, false);
            }
        }

        private static void DrawCurvedBar(float x, float y, float radius, int segments, float fill, Color color, Color bgColor, bool isLeft)
        {
            float startAngle = isLeft ? 240f : 120f;
            float endAngle = isLeft ? 300f : 60f;
            float angleRange = endAngle - startAngle;

            GUI.color = bgColor;
            for (int i = 0; i < segments; i++)
            {
                float angle = startAngle + (angleRange * i / segments);
                float rad = angle * Mathf.Deg2Rad;
                GUI.DrawTexture(new Rect(x + Mathf.Sin(rad) * radius, y - Mathf.Cos(rad) * radius, 4f, 4f), _barTex);
            }

            GUI.color = color;
            int fillSegments = Mathf.RoundToInt(segments * Mathf.Clamp01(fill));
            for (int i = 0; i < fillSegments; i++)
            {
                float angle = startAngle + (angleRange * i / segments);
                float rad = angle * Mathf.Deg2Rad;
                GUI.DrawTexture(new Rect(x + Mathf.Sin(rad) * radius, y - Mathf.Cos(rad) * radius, 4f, 4f), _barTex);
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