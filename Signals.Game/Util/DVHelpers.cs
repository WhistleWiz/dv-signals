using DV;
using UnityEngine.Audio;

namespace Signals.Game.Util
{
    internal class DVHelpers
    {
        private static AudioMixerGroup? s_outdoors;

        public static AudioMixerGroup OutdoorsMixerGroup
        {
            get
            {
                if (s_outdoors == null)
                {
                    Globals.G.Types.TryGetLivery("LocoS282A", out var s282);
                    s_outdoors = s282.parentType.audioPrefab.transform.Find("[sim] Engine/Fire/Fire_Layered")
                        .GetComponent<LayeredAudio>().audioMixerGroup;
                }

                return s_outdoors;
            }
        }
    }
}
