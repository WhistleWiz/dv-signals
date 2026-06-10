using UnityEngine;

namespace Signals.Common.Aspects
{
    public abstract class AspectBaseDefinition : MonoBehaviour
    {
        [Tooltip("The ID of this aspect, so it can be detected from other aspects")]
        public string Id = string.Empty;
        [Tooltip("This is the sprite displayed on the HUD\n" +
            "The recommended size is 256x256px")]
        public Sprite? HUDSprite;
        [Tooltip("Lower values are placed above higher values\n" +
            "Only used for indicators")]
        public int HUDDisplayOrder = 1;

        [Space]
        public bool DisallowPassing = false;
        public bool RequireAcknowledging = false;
        public bool UsePassingSpeed = false;
        public float PassingSpeed = 20;

        [Space]
        public SignalLightDefinition[] OnLights = new SignalLightDefinition[0];
        public SignalLightDefinition[] BlinkingLights = new SignalLightDefinition[0];
        public SignalLightSequenceDefinition[] LightSequences = new SignalLightSequenceDefinition[0];
        public SignalLightColourChangerDefinition[] ColourChangers = new SignalLightColourChangerDefinition[0];

        [Space]
        public TransformMoverTarget[] Movers = new TransformMoverTarget[0];
        public ObjectChanger? Changer = null;

        [Space]
        [Tooltip("A random entry is selected to play when this aspect is activated\n" +
            "Sound is played from this object's transform")]
        public AudioClip[] ActivationAudios = new AudioClip[0];
    }
}
