using Signals.Common;
using UnityEngine;

namespace Signals.Game
{
    internal class SignalLight : MonoBehaviour
    {
        private static Transform? s_glare;
        private static Transform Glare
        {
            get
            {
                if (s_glare == null)
                {
                    DV.Globals.G.Types.TryGetLivery("LocoDE2", out var de2);
                    s_glare = de2.prefab.transform.Find("[headlights_de2]/FrontSide/HeadlightLeftLow/Glare");
                }

                return s_glare;
            }
        }

        public LampControl Lamp = null!;
        public SignalLightDefinition Definition = null!;

        public void Initialize(SignalLightDefinition def)
        {
            def.gameObject.SetActive(false);

            var indicator = def.gameObject.AddComponent<IndicatorEmission>();
            indicator.lag = 0.2f;
            indicator.lightIntensity = 2.5f;
            indicator.emissionColor = def.Color;
            indicator.glareColor = def.Color;
            indicator.renderers = new[] { def.Renderer };

            if (def.Glare != null)
            {
                indicator.glareRenderer = CreateGlare(def.Glare);
            }

            Lamp = indicator.gameObject.AddComponent<LampControl>();
            Lamp.lampInd = indicator;

            def.gameObject.SetActive(true);
            Definition = def;
        }

        public void TurnOn(bool blink = false)
        {
            if (blink)
            {
                Lamp.SetLampState(LampControl.LampState.Blinking);
            }
            else
            {
                Lamp.SetLampState(LampControl.LampState.On);
            }
        }

        public void TurnOff()
        {
            Lamp.SetLampState(LampControl.LampState.Off);
        }

        private Renderer CreateGlare(Transform root)
        {
            var glare = Instantiate(Glare, root);
            glare.localPosition = Vector3.zero;
            glare.localRotation = Quaternion.identity;
            glare.localScale = Vector3.one;
            glare.gameObject.SetActive(true);

            return glare.GetComponent<Renderer>();
        }
    }
}
