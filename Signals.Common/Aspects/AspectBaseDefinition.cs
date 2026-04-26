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

        [Space]
        public bool DisallowPassing = false;
        public bool RequireAcknowledging = false;

        [Space]
        public SignalLightDefinition[] OnLights = new SignalLightDefinition[0];
        public SignalLightDefinition[] BlinkingLights = new SignalLightDefinition[0];
        public SignalLightSequenceDefinition[] LightSequences = new SignalLightSequenceDefinition[0];

        [Space]
        public TransformMover[] Movers = new TransformMover[0];

        [Space]
        [Tooltip("A random entry is selected to play when this aspect is activated\n" +
            "Sound is played from this object's transform")]
        public AudioClip[] ActivationAudios = new AudioClip[0];
    }
}
