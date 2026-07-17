using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Railway
{
    public static class TurntableHelper
    {
        private static List<TurntableRailTrack> s_turntableTracks = new List<TurntableRailTrack>();
        private static Dictionary<TurntableRailTrack, int> s_dirty = new Dictionary<TurntableRailTrack, int>();
        private static Dictionary<RailTrack, HashSet<TurntableRailTrack>> s_ends = new Dictionary<RailTrack, HashSet<TurntableRailTrack>>();

        public static List<TurntableRailTrack> TurntableTracks => s_turntableTracks;

        internal static void ClearCache()
        {
            s_turntableTracks.Clear();
            s_ends.Clear();
            s_dirty.Clear();
        }

        internal static void PrepareTracks()
        {
            var tracks = Object.FindObjectsOfType<TurntableRailTrack>();

            foreach (var track in tracks)
            {
                s_turntableTracks.Add(track);
                track.TracksUpdated += (f, r) => MarkDirty(track);

                foreach (var end in track.trackEnds)
                {
                    if (!s_ends.TryGetValue(end.track, out var set))
                    {
                        set = new HashSet<TurntableRailTrack>();
                        s_ends[end.track] = set;
                    }

                    set.Add(track);
                }
            }
        }

        internal static void ReduceDirty()
        {
            var keys = s_dirty.Keys.ToList();

            foreach (var key in keys)
            {
                var value = s_dirty[key];
                s_dirty[key] = value - 1;

                if (value < 1)
                {
                    s_dirty.Remove(key);
                }
            }
        }

        private static void MarkDirty(TurntableRailTrack track)
        {
            s_dirty[track] = 2;
        }

        public static bool IsDirty(RailTrack track)
        {
            return s_ends.TryGetValue(track, out var set) && set.Any(x => s_dirty.ContainsKey(x));
        }

        public static bool IsTurntableEnd(RailTrack track)
        {
            return s_ends.ContainsKey(track);
        }
    }
}
