using Signals.Common;
using System.Linq;
using UnityEngine;

namespace Signals.Game
{
    internal static class Extensions
    {
        public static SignalLight GetController(this SignalLightDefinition definition)
        {
            if (!definition.TryGetComponent(out SignalLight controller))
            {
                controller = definition.gameObject.AddComponent<SignalLight>();
                controller.Initialize(definition);
            }

            return controller;
        }

        public static bool IsOccupied(this RailTrack track, CrossingCheckMode check)
        {
            return TrackChecker.IsOccupied(track, check);
        }

        public static bool HasBogies(this RailTrack track)
        {
            return track.onTrackBogies.Count() > 0;
        }

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
    }
}
