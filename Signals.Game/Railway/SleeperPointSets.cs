using DV.PointSet;
using System.Collections.Generic;

namespace Signals.Game.Railway
{
    public static class SleeperPointSets
    {
        private static Dictionary<RailTrack, EquiPointSet> s_sleeperPoints = new Dictionary<RailTrack, EquiPointSet>();

        public static void ClearCache()
        {
            s_sleeperPoints.Clear();
        }

        public static EquiPointSet GetSleepers(RailTrack track)
        {
            if (s_sleeperPoints.TryGetValue(track, out var set)) return set;

            set = EquiPointSet.ResampleEquidistant(track.GetKinkedPointSet(),
                track.baseType.sleeperDistance,
                track.baseType.sleeperDistance * 0.5f,
                true, true);

            s_sleeperPoints[track] = set;
            return set;
        }
    }
}
