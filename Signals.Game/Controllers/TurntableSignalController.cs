using Signals.Common;
using UnityEngine;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// A controller for a turntable.
    /// </summary>
    public class TurntableSignalController : BasicSignalController
    {
        private const float ValidConnectionDelay = 1.0f;
        private Coroutine? _routine;

        public TurntableRailTrack Track { get; private set; }
        public TrackDirection Direction { get; private set; }
        public bool IsConnected { get; private set; }

        public TurntableSignalController(SignalControllerDefinition def, TurntableRailTrack track, TrackDirection direction, SignalPlacementInfo? info) :
            base(def, info)
        {
            Track = track;
            Direction = direction;

            Track.TracksUpdated += TracksUpdated;
            Destroyed += (x) => Track.TracksUpdated -= TracksUpdated;

            TracksUpdated(Track.frontClosest?.track, Track.rearClosest?.track);
        }

        protected override string GenerateName()
        {
            return $"{Track.uniqueID}-{(Direction.IsOut() ? 1 : 2)}";
        }

        public override bool ShouldUpdate() => false;

        private void TracksUpdated(RailTrack? frontTrack, RailTrack? backTrack)
        {
            var connected = (Direction.IsOut() ? frontTrack : backTrack) != null;

            // Use a delay to prevent false triggers when passing by a connection.
            if (connected && _routine == null)
            {
                _routine = Definition.StartCoroutine(AcceptConnection(ValidConnectionDelay));
                return;
            }

            if (_routine != null)
            {
                Definition.StopCoroutine(_routine);
                _routine = null;
            }

            IsConnected = false;
            Update(true, false);
        }

        private System.Collections.IEnumerator AcceptConnection(float delay)
        {
            yield return WaitFor.Seconds(delay);

            IsConnected = true;
            Update(true, false);
        }
    }
}
