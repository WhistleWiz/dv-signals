using Signals.Common;
using Signals.Game.Util;
using UnityEngine;

namespace Signals.Game.Lights
{
    public class SignalLight : MonoBehaviour
    {
        public const float SmallFloat = 0.0009999871f;

        private static Renderer? s_glare;
        private static Renderer Glare
        {
            get
            {
                if (s_glare == null)
                {
                    DV.Globals.G.Types.TryGetLivery("LocoDE2", out var de2);
                    //s_glare = de2.prefab.transform.Find("[headlights_de2]/FrontSide/HeadlightLeftLow/Glare");
                    s_glare = de2.interiorPrefab.transform.Find("DashCluster/HeadlightsFront/L_Headlights/glare").GetComponent<Renderer>();
                }

                return s_glare;
            }
        }

        private static Material? s_mat;
        private static Material GlareMat
        {
            get
            {
                if (s_mat == null)
                {
                    s_mat = new Material(Glare.sharedMaterial);
                    s_mat.SetFloat("_FadeoutPower", 2.2f);
                    s_mat.SetFloat("_LightAtten", 0.7f);
                    s_mat.SetFloat("_MaxAtten", 0.8f);
                }

                return s_mat;
            }
        }

        private Coroutine? _colourChange;

        protected LampControl.LampState InternalState = LampControl.LampState.None;

        public LampControl Lamp = null!;
        public SignalLightDefinition Definition = null!;

        public Signal Signal { get; private set; } = null!;

        public bool IsActive => Lamp.lampInd.EmissionValue > SmallFloat;

        internal IndicatorEmission Indicator => Lamp.lampInd;

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
            glare.transform.localPosition = Vector3.zero;
            glare.transform.localRotation = Quaternion.identity;
            glare.transform.localScale = Vector3.one;
            glare.transform.gameObject.SetActive(true);
            glare.sharedMaterial = GlareMat;

            return glare;
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

        public void ChangeColour(SignalLightColourChangerDefinition? changer)
        {
            if (_colourChange != null)
            {
                StopCoroutine(_colourChange);
            }

            _colourChange = StartCoroutine(ColourRoutine(changer == null ? Definition.Colour : changer.Colour));
        }

        private System.Collections.IEnumerator ColourRoutine(Color c)
        {
            var start = Indicator.emissionColor;

            if (Definition.Lag > 0.001f)
            {
                for (float f = 0; f < 1; f += Time.deltaTime / Definition.Lag)
                {
                    SetColour(Color.Lerp(start, c, f));
                    yield return null;
                }
            }

            SetColour(c);
        }

        private void SetColour(Color c)
        {
            Indicator.emissionColor = c;
            Indicator.glareColor = c;
            ReflectionHelpers.ForceSetColour(Indicator);
        }
    }
}
