using DV.Hovering;
using DV.HUD.Signs;
using DV.Signs;
using DV.UI.LocoHUD;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace Signals.Game
{
    internal class SignalHover : SignHover
    {
        private static Dictionary<Sprite, GameObject> s_sprites = new Dictionary<Sprite, GameObject>();
        private static ContentSizeFitter? s_sizeFitter;
        private static SignDisplayElement? s_template;
        private static SignDisplayElement Template
        {
            get
            {
                if (s_template == null)
                {
                    s_template = Sign.Config.GetSignReference(SignType.RectRed).uiDisplayElement;
                }

                if (s_sizeFitter == null && HUDManager.Instance != null)
                {
                    s_sizeFitter = HUDManager.Instance.SignDisplay.contentRoot.GetComponent<ContentSizeFitter>();
                    s_sizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
                    s_sizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
                }

                return s_template;
            }
        }

        public void Initialise(Sprite? offSprite)
        {
            signTypes = new List<SignDisplay.SignDisplayInstance>();

            if (offSprite == null)
            {
                return;
            }

            signTypes.Add(new SignDisplay.SignDisplayInstance()
            {
                prefab = GetPrefabFromSprite(offSprite)
            });
        }

        public void UpdateStateDisplay(Sprite? sprite)
        {
            signTypes.Clear();

            if (sprite != null)
            {
                signTypes.Add(new SignDisplay.SignDisplayInstance()
                {
                    prefab = GetPrefabFromSprite(sprite),
                    text = " "
                });
            }

            var (type, obj) = NonVRHoverManager.Instance.CurrentlyHovered;
            if (type == NonVRHoverManager.HoverType.Sign && ((SignHover)obj) == this)
            {
                Unhovered();
                Hovered();
            }
        }

        public GameObject GetPrefabFromSprite(Sprite sprite)
        {
            bool found = s_sprites.TryGetValue(sprite, out var go);

            if (!found || go == null)
            {
                var display = Instantiate(Template, SignalManager.Holder);
                var rect = display.GetComponentInChildren<RectTransform>();
                rect.sizeDelta = new Vector2(120, 120);
                var img = display.GetComponentInChildren<Image>();
                img.sprite = sprite;

                go = display.gameObject;

                // Sprite existed in dictionary but GO was deleted somehow, so just reassign with the new one.
                if (found)
                {
                    s_sprites[sprite] = go;
                }
                else
                {
                    s_sprites.Add(sprite, go);
                }
            }

            return go;
        }
    }
}
