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
        public SignalLightDefinition[] OnLights = new SignalLightDefinition[0];
        public SignalLightDefinition[] BlinkingLights = new SignalLightDefinition[0];
        public SignalLightSequenceDefinition[] LightSequences = new SignalLightSequenceDefinition[0];

        [Space]
        public string? AnimationName;
        public float AnimationTime = 1.0f;
        [Tooltip("Disables the animator after changing the signal state to this\n" +
            "Only disable this option if you want to keep playing an animation after changing the state " +
            "(i.e. \"wigwag\" style signals)")]
        public bool DisableAnimatorAfterChanging = true;

        [Space]
        [Tooltip("A random entry is selected to play when this aspect is activated\n" +
            "Sound is played from this object's transform")]
        public AudioClip[] ActivationAudios = new AudioClip[0];
    }
}
