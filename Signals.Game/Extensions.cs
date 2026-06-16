using DV.Logic.Job;
using DV.PointSet;
using Signals.Common;
using Signals.Game.Lights;
using Signals.Game.Railway;
using Signals.Game.Util;
using System.Linq;
using UnityEngine;

using Branch = Junction.Branch;

namespace Signals.Game
{
    internal static class Extensions
    {
        #region Enums

        public static bool IsOut(this TrackDirection direction) => direction == TrackDirection.Out;

        public static TrackDirection Flipped(this TrackDirection direction) => direction == TrackDirection.Out ? TrackDirection.In : TrackDirection.Out;

        public static bool IsFullyManual(this SignalOperationMode mode) => mode == SignalOperationMode.FullManual;

        public static bool IsEntry(this SignalType type) => type switch
        {
            SignalType.Entry => true,
            _ => false,
        };

        public static bool IsAnyExit(this SignalType type) => type switch
        {
            SignalType.Exit => true,
            SignalType.ExitPax => true,
            SignalType.ExitMainline => true,
            _ => false,
        };

        #endregion

        #region Definitions

        public static SignalLight GetController(this SignalLightDefinition definition, Signal signal)
        {
            if (!definition.TryGetComponent(out SignalLight controller))
            {
                if (definition.NightOnly)
                {
                    controller = definition.gameObject.AddComponent<SignalNightLight>();
                }
                else
                {
                    controller = definition.gameObject.AddComponent<SignalLight>();
                }

                controller.Initialize(definition, signal);
            }

            return controller;
        }

        public static SignalLightSequence GetController(this SignalLightSequenceDefinition definition, Signal signal)
        {
            if (!definition.TryGetComponent(out SignalLightSequence controller))
            {
                controller = definition.gameObject.AddComponent<SignalLightSequence>();
                controller.Initialize(definition, signal);
            }

            return controller;
        }

        #endregion

        #region Track

        private const string YardNameStart = "[Y]";
        private const string NoSign = "[#]";

        public static bool IsOccupied(this RailTrack track, CrossingCheckMode crossingMode)
        {
            return TrackChecker.IsOccupied(track, crossingMode);
        }

        public static bool HasBogies(this RailTrack track)
        {
            return track.BogiesOnTrack().Count() > 0;
        }

        public static bool IsReservedByAnother(this RailTrack track, Signal signal)
        {
            return TrackChecker.IsReservedByAnother(track, signal);
        }

        public static bool IsPartOfYard(this RailTrack track)
        {
            return track.name.StartsWith(YardNameStart);
        }

        public static bool IsNonSign(this RailTrack track)
        {
            return track.name.StartsWith(NoSign);
        }

        public static bool IsPartOfStation(this RailTrack track)
        {
            return track.IsPartOfYard() || track.IsNonSign();
        }

        public static double GetLength(this RailTrack track)
        {
            return track.LogicTrack().length;
        }

        public static TrackID GetId(this RailTrack track)
        {
            return track.LogicTrack().ID;
        }

        #endregion

        #region Junctions

        private const string JunctionLeft = "junc-l";
        private const string TrackThrough = "[track through]";
        private const string DoubleTrackTag = "DT-";
        private const string DoubleTrackStationTag = "DTS-";

        public static Branch GetCurrentBranch(this Junction junction)
        {
            return junction.outBranches[junction.selectedBranch];
        }

        public static Branch GetDefaultBranch(this Junction junction)
        {
            return junction.outBranches[junction.defaultSelectedBranch];
        }

        public static RailTrack[] GetAllTracks(this Junction junction)
        {
            return junction.outBranches.Select(x => x.track).ToArray();
        }

        public static bool IsSetToThrough(this Junction junction)
        {
            return junction.GetCurrentBranch().track.name == TrackThrough;
        }

        public static bool IsLeft(this Junction junction)
        {
            return junction.transform.parent.name.StartsWith(JunctionLeft);
        }

        public static bool IsFromDoubleTrackMod(this Junction junction)
        {
            return junction.junctionData.junctionIdLong.StartsWith(DoubleTrackTag);
        }

        public static bool IsFromDoubleTrackModStation(this Junction junction)
        {
            return junction.junctionData.junctionIdLong.StartsWith(DoubleTrackStationTag);
        }

        public static string GetStation(this Junction junction)
        {
            return TrackUtils.JunctionStation(junction);
        }

        public static bool IsThroughTrack(this Branch branch)
        {
            return branch.track.name == TrackThrough;
        }

        #endregion

        #region Other

        public static float Volume(this Bounds bounds)
        {
            var size = bounds.size;
            return size.x * size.y * size.z;
        }

        public static float Area2D(this Bounds bounds)
        {
            var size = bounds.size;
            return size.x * size.z;
        }

        public static int GetIndex(this EquiPointSet set, int index, TrackDirection direction)
        {
            return direction.IsOut() ? Helpers.ClampBounds(index, set.points) : Helpers.ClampBounds(set.points.Length - index - 1, set.points);
        }

        public static double GetSpan(this EquiPointSet set, double span, TrackDirection direction)
        {
            return Helpers.ClampD(direction.IsOut() ? span : set.span - span, 0, set.span);
        }

        #endregion
    }
}
