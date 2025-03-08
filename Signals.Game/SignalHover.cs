using DV.Hovering;
using DV.HUD.Signs;
using DV.Signs;
using DV.UI.LocoHUD;
using Signals.Game.Controllers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Signals.Game
{
    internal class SignalHover : SignHover
    {
        private const string BlankSpace = " ";

        private static Vector2 s_size = new Vector2(120, 120);
        private static Dictionary<Sprite, GameObject> s_sprites = new Dictionary<Sprite, GameObject>();
        private static ContentSizeFitter? s_sizeFitter;
        private static SignDisplayElement? s_template;
        private static SignDisplayElement Template
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

        public void UpdateStateDisplay(BasicSignalController controller, Sprite? sprite)
        {
            signTypes.Clear();

            if (sprite != null)
            {
                signTypes.Add(new SignDisplay.SignDisplayInstance()
                {
                    prefab = GetPrefabFromSprite(sprite),
                    text = BlankSpace
                });
            }

            foreach (var item in controller.AllDisplays)
            {
                if (item == null || !item.ShouldDisplayHUD) continue;

                var go = GetPrefabFromSprite(item.Definition.HUDBackground);
                var text = go.GetComponentInChildren<TMP_Text>();

                if (text != null)
                {
                    text.color = item.Definition.HUDTextColour;
                }

                signTypes.Add(new SignDisplay.SignDisplayInstance()
                {
                    prefab = go,
                    text = item.DisplayText
                });
            }

            foreach (var item in controller.AllIndicators)
            {
                if (item == null || !item.ShouldDisplayHUD) continue;

                var go = GetPrefabFromSprite(item.Definition.HUDSprite);

                signTypes.Add(new SignDisplay.SignDisplayInstance()
                {
                    prefab = go,
                    text = BlankSpace
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
