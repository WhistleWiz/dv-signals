using UnityEngine;

namespace Signals.Common.States
{
    public abstract class SignalStateBaseDefinition : MonoBehaviour
    {
        public abstract string Id { get; }

        [Header("HUD Display")]
        [Tooltip("This is the sprite displayed on the HUD\n" +
            "The maximum recommended size is 256x256px")]
        public Sprite? HUDSprite;

        [Header("Optional - Lights")]
        public SignalLightDefinition[] OnLights = new SignalLightDefinition[0];
        public SignalLightDefinition[] BlinkingLights = new SignalLightDefinition[0];

        [Header("Optional - Animation")]
        public string? AnimationName;
        public float AnimationTime = 1.0f;
        [Tooltip("Disables the animator after changing the signal state to this\n" +
            "Only disable if you want to keep playing an animation after changing the state (i.e. \"wigwag\" style signals)")]
        public bool DisableAnimatorAfterChanging = true;

        [Header("Optional - Sound")]
        public AudioClip[] ActivationAudios = new AudioClip[0];
        [Tooltip("If not set, plays audio from the signal root transform")]
        public Transform? AudioPosition;
    }
}
