using DV.Logic.Job;
using Signals.Common;
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

        #endregion

        #region Definitions

        public static SignalLight GetController(this SignalLightDefinition definition)
        {
            if (!definition.TryGetComponent(out SignalLight controller))
            {
                controller = definition.gameObject.AddComponent<SignalLight>();
                controller.Initialize(definition);
            }

            return controller;
        }

        public static SignalLightSequence GetController(this SignalLightSequenceDefinition definition)
        {
            if (!definition.TryGetComponent(out SignalLightSequence controller))
            {
                controller = definition.gameObject.AddComponent<SignalLightSequence>();
                controller.Initialize(definition);
            }

            return controller;
        }

        public static SignalControllerDefinition? GetForType(this SignalPack pack, SignalType type, bool old) => type switch
        {
            SignalType.Mainline => old ? pack.OldSignal : pack.Signal,
            SignalType.IntoYard => old ? pack.OldIntoYardSignal : pack.IntoYardSignal,
            SignalType.Shunting => old ? pack.OldShuntingSignal : pack.ShuntingSignal,
            SignalType.Distant => old ? pack.OldDistantSignal : pack.DistantSignal,
            _ => null,
        };

        #endregion

        #region Track

        public static bool IsOccupied(this RailTrack track, CrossingCheckMode check)
        {
            return TrackChecker.IsOccupied(track, check);
        }

        public static bool HasBogies(this RailTrack track)
        {
            return track.BogiesOnTrack().Count() > 0;
        }

        public static double GetLength(this RailTrack track)
        {
            return RailTrackRegistry.RailTrackToLogicTrack[track].length;
        }

        public static TrackID GetID(this RailTrack track)
        {
            return RailTrackRegistry.RailTrackToLogicTrack[track].ID;
        }

        #endregion

        #region Junctions

        public static Branch GetCurrentBranch(this Junction junction)
        {
            return junction.outBranches[junction.selectedBranch];
        }

        public static bool IsSetToThrough(this Junction junction)
        {
            return junction.GetCurrentBranch().track.name == "[track through]";
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

        #endregion
    }
}
