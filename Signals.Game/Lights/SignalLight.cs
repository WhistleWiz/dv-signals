using Signals.Common;
using UnityEngine;

namespace Signals.Game.Lights
{
    public class SignalLight : MonoBehaviour
    {
        public const float SmallFloat = 0.0009999871f;

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

        protected LampControl.LampState InternalState = LampControl.LampState.None;

        public LampControl Lamp = null!;
        public SignalLightDefinition Definition = null!;

        public Signal Signal { get; private set; } = null!;

        public bool IsActive => Lamp.lampInd.EmissionValue > SmallFloat;

        public virtual void Initialize(SignalLightDefinition def, Signal signal)
        {
            Definition = def;
            Signal = signal;

            // Prevent Awake from running on the indicator and lamp control.
            def.gameObject.SetActive(false);

            var indicator = def.gameObject.AddComponent<IndicatorEmission>();
            indicator.lag = def.Lag;
            indicator.lightIntensity = def.LightIntensity;
            indicator.emissionColor = def.Colour;
            indicator.emissionLight = def.Light;
            indicator.glareColor = def.Colour;
            indicator.renderers = def.Renderers;

            if (def.Glare != null)
            {
                indicator.glareRenderer = CreateGlare(def.Glare);
            }

            Lamp = indicator.gameObject.AddComponent<LampControl>();
            Lamp.lampInd = indicator;

            def.gameObject.SetActive(true);
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

        /// <summary>
        /// Turns on the light.
        /// </summary>
        /// <param name="blink">Optional blinking state.</param>
        public virtual void TurnOn(bool blink = false)
        {
            InternalState = blink ? LampControl.LampState.Blinking : LampControl.LampState.On;
            UpdateFromState();
        }

        /// <summary>
        /// Turns off the light.
        /// </summary>
        public virtual void TurnOff()
        {
            InternalState = LampControl.LampState.Off;
            UpdateFromState();
        }

        protected void ForceOff()
        {
            Lamp.SetLampState(LampControl.LampState.Off);
        }

        protected void UpdateFromState()
        {
            Lamp.SetLampState(InternalState);
        }
    }
}
