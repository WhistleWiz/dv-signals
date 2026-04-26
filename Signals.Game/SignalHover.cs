using DV.Common;
using DV.Hovering;
using DV.Signs;
using DV.UI.LocoHUD;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Signals.Game
{
    internal class SignalHover : SignHover
    {
        private static Vector2 s_size = new Vector2(120, 120);
        private static Dictionary<Sprite, GameObject> s_sprites = new Dictionary<Sprite, GameObject>();
        private static ContentSizeFitter? s_sizeFitter;
        private static ASignDisplayElement? s_template;
        private static ASignDisplayElement Template
        {
            get
            {
                if (s_template == null)
                {
                    s_template = Sign.Config.GetSignReference(SignType.RectWhite).uiDisplayElement;
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
            signTypes = new List<SignDisplayInstance>();

            if (offSprite == null)
            {
                return;
            }

            signTypes.Add(new SignDisplayInstance()
            {
                prefab = GetPrefabFromSprite(offSprite)
            });
        }

        public void UpdateStateDisplay(Signal signal)
        {
            signTypes.Clear();

            var hudElements = signal.GetAllHudElements().OrderBy(x => x.DisplayOrder);

            foreach (var element in hudElements)
            {
                if (element.Sprite == null || string.IsNullOrEmpty(element.DisplayText)) continue;

                var go = GetPrefabFromSprite(element.Sprite);
                var text = go.GetComponentInChildren<TMP_Text>();

                if (text != null)
                {
                    text.color = element.TextColour;
                }

                signTypes.Add(new SignDisplayInstance()
                {
                    prefab = go,
                    text = element.DisplayText
                });
            }

            // Refresh if the current hovered thing is this.
            var (type, obj) = NonVRHoverManager.Instance.CurrentlyHovered;
            if (type == NonVRHoverManager.HoverType.Sign && ((SignHover)obj) == this)
            {
                Unhovered();
                Hovered();
            }
        }

        public GameObject GetPrefabFromSprite(Sprite? sprite)
        {
            if (sprite == null) return null!;

            bool found = s_sprites.TryGetValue(sprite, out var go);

            if (!found || go == null)
            {
                var display = Instantiate(Template, SignalManager.Holder);
                display.name = sprite.name;
                var rect = display.GetComponentInChildren<RectTransform>();
                rect.sizeDelta = GetSize(sprite);
                var img = display.GetComponentInChildren<Image>();
                img.sprite = sprite;

                go = display.gameObject;
                s_sprites[sprite] = go;
            }

            return go;
        }

        private static Vector2 GetSize(Sprite sprite)
        {
            // Rework scalling to not always max out at 120px?
            return s_size * sprite.rect.size / 256.0f;
        }
    }
}
