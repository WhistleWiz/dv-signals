﻿using Signals.Common;
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

        public static SignalControllerDefinition? GetForType(this SignalPack pack, SignalType type)
        {
            switch (type)
            {
                case SignalType.Mainline:
                    return pack.Signal;
                case SignalType.IntoYard:
                    return pack.IntoYardSignal;
                case SignalType.Shunting:
                    return pack.ShuntingSignal;
                case SignalType.Distant:
                    return pack.DistantSignal;
                default:
                    return null;
            }
        }

        #endregion

        #region Track

        public static bool IsOccupied(this RailTrack track, CrossingCheckMode check)
        {
            return TrackChecker.IsOccupied(track, check);
        }

        public static bool HasBogies(this RailTrack track)
        {
            return track.onTrackBogies.Count() > 0;
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
