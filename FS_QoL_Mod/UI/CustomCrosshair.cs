using UnityEngine;
using UnityEngine.UI;

namespace FS_FovChanger.UI
{
    public static class CustomCrosshair
    {
        private static GameObject _crosshairRoot;
        private static Image[] _lines = new Image[4];
        private static Image _defaultCrosshairImage;
        private static FPSController _lastPlayer;

        private static void Initialize()
        {
            if (Hud.Player == null) return;

            // Find the correct canvas/UI transform, trying "PlayerUI" first, then "Canvas".
            var uiRootTransform = Hud.Player.transform.Find("Main Camera/PlayerUI");
            if (uiRootTransform == null)
            {
                uiRootTransform = Hud.Player.transform.Find("Main Camera/Canvas");
            }
            
            if (uiRootTransform == null) return; // Abort if no UI root is found

            _crosshairRoot = new GameObject("CustomCrosshairRoot");
            _crosshairRoot.transform.SetParent(uiRootTransform, false);
            var rootRect = _crosshairRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;

            for (int i = 0; i < 4; i++)
            {
                var lineObj = new GameObject("CustomLine" + i);
                lineObj.transform.SetParent(_crosshairRoot.transform, false);
                _lines[i] = lineObj.AddComponent<Image>();
                _lines[i].sprite = GetWhiteSprite();
            }
        }

        public static void OnSceneLoad()
        {
            _defaultCrosshairImage = null;
            _lastPlayer = null;
        }

        public static void Update()
        {
            if (Hud.Player == null)
            {
                if (_crosshairRoot != null && _crosshairRoot.activeSelf)
                {
                    _crosshairRoot.SetActive(false);
                }
                return;
            }

            if (_lastPlayer != Hud.Player)
            {
                _lastPlayer = Hud.Player;
                _defaultCrosshairImage = null;
                // Also reset the crosshair root to force re-initialization for the new player model
                _crosshairRoot = null; 
            }
            
            if (_defaultCrosshairImage == null)
            {
                // Try to find the crosshair panel using both known paths.
                var panelTransform = Hud.Player.transform.Find("Main Camera/PlayerUI/Panel (4)");
                if (panelTransform == null)
                {
                    panelTransform = Hud.Player.transform.Find("Main Camera/Canvas/Panel (4)");
                }

                if (panelTransform != null)
                {
                    _defaultCrosshairImage = panelTransform.GetComponent<Image>();
                }
            }

            if (Config.CrosshairEnabled.Value)
            {
                if (_defaultCrosshairImage != null && _defaultCrosshairImage.enabled)
                {
                    _defaultCrosshairImage.enabled = false;
                }

                if (_crosshairRoot == null)
                {
                    Initialize();
                    if (_crosshairRoot == null) return;
                }
                
                if (!_crosshairRoot.activeSelf) _crosshairRoot.SetActive(true);

                if (ColorUtility.TryParseHtmlString(Config.CrosshairColor.Value, out var color))
                {
                    foreach (var line in _lines)
                    {
                        if (line != null) line.color = color;
                    }
                }

                bool[] barEnabled = {
                    Config.CrosshairBarTop.Value, Config.CrosshairBarRight.Value,
                    Config.CrosshairBarBottom.Value, Config.CrosshairBarLeft.Value
                };

                for (int i = 0; i < 4; i++)
                {
                    if (_lines[i] == null) continue;
                    
                    _lines[i].enabled = barEnabled[i];
                    if (!barEnabled[i]) continue;

                    var lineRect = _lines[i].GetComponent<RectTransform>();
                    
                    float baseAngle = 90f * i;
                    float finalAngle = baseAngle + Config.CrosshairRotation.Value;
                    float rad = finalAngle * Mathf.Deg2Rad;

                    lineRect.sizeDelta = new Vector2(Config.CrosshairThickness.Value, Config.CrosshairSize.Value);
                    lineRect.localRotation = Quaternion.Euler(0, 0, -finalAngle);
                    
                    float effectiveGap = Config.CrosshairGap.Value + (Config.CrosshairSize.Value / 2f);
                    lineRect.anchoredPosition = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * effectiveGap;
                }
            }
            else
            {
                if (_crosshairRoot != null && _crosshairRoot.activeSelf)
                {
                    _crosshairRoot.SetActive(false);
                }

                if (_defaultCrosshairImage != null && !_defaultCrosshairImage.enabled)
                {
                    _defaultCrosshairImage.enabled = true;
                }
            }
        }

        private static Sprite GetWhiteSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        }
    }
}