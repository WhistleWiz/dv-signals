using Signals.Common;
using UnityEngine;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// Controls a signal with a junction.
    /// </summary>
    /// <remarks>
    /// Signal aspect is updated every second when a player is within 2km of the signal, and slows to
    /// an update every 5 seconds when further away.
    /// </remarks>
    public class JunctionSignalController : BasicSignalController
    {
        // Signals at over this distance from the camera update at a slower rate.
        private const float SlowUpdateDistanceSqr = 1500 * 1500;
        private const float SkipUpdateDistanceSqr = 5000 * 5000;
        // Maximum number of times the update can be delayed.
        private const int MaxUpdateDelay = 5;

        private int _updateDelay = 0;

        public Junction Junction { get; protected set; }
        /// <summary>
        /// Whether the signal refers to the junction's branches or the inbound track.
        /// </summary>
        public TrackDirection Direction { get; protected set; }

        public override string Name => string.IsNullOrEmpty(NameOverride) ? $"{Junction.junctionData.junctionIdLong}-{(Direction.IsOut() ? 'T' : 'F')}" : NameOverride;

        public JunctionSignalController(SignalControllerDefinition def, Junction junction, TrackDirection direction) : base(def)
        {
            Junction = junction;
            Direction = direction;

            SignalManager.Instance.RegisterSignal(this);
            Junction.Switched += JunctionSwitched;

            OnDestroyed += (x) => Junction.Switched -= JunctionSwitched;
        }

        private void JunctionSwitched(Junction.SwitchMode mode, int branch)
        {
            // Force update the display because of junction branch updates even if
            // the state didn't change.
            UpdateAspect();
            UpdateDisplays(true);
        }

        public override bool ShouldSkipUpdate()
        {
            if (ManualOperationOnly) return true;

            var dist = GetCameraDistanceSqr();

            // If the camera is too far from the signal, skip updating.
            // If the camera is far, but the signal is relatively close, use a slowed update rate.
            if (dist > SkipUpdateDistanceSqr || (dist > SlowUpdateDistanceSqr && _updateDelay < MaxUpdateDelay))
            {
                _updateDelay++;
                return true;
            }

            _updateDelay = 0;
            return false;
        }

        /// <summary>
        /// Updates the current aspect based on the conditions of <see cref="AllAspects"/>.
        /// </summary>
        public override void UpdateAspect()
        {
            // Precompute this information so each state doesn't have to call the same functions
            // over and over again.
            TrackInfo = TrackWalker.WalkUntilNextSignal(this);

            base.UpdateAspect();
        }

        /// <summary>
        /// Returns the other signal at the assigned junction.
        /// </summary>
        public JunctionSignalController? GetPaired()
        {
            SignalManager.Instance.TryGetSignal(Junction, Direction.Flipped(), out var controller);
            return controller;
        }
    }
}
