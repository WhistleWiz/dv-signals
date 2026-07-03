using UnityEngine;

namespace Signals.Common.Animation
{
    [AddComponentMenu("DV Signals/Animation/Play Sound From Animation Events")]
    public class PlaySoundFromAnimationEvents : MonoBehaviour
    {
        // Source mixer group is replaced by the outdoors one from DV.
        public AudioSource Source = null!;
        public AudioClip[] Clips = new AudioClip[0];

        public void PlaySound(int index)
        {
            // Match the index sent by the event to the clip.
            // This allows multiple clips from a single component.
            Source.PlayOneShot(Clips[index]);
        }
    }
}
