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
        // Can't completely stop updates or else signals reading states that are
        // far may be stuck at the wrong state.
        private const float OptimiseDistanceSqr = 1500 * 1500;
        private const float SkipDistanceSqr = 5000 * 5000;
        private const float UpdateTime = 1.0f;
        private const int MaxUpdateDelay = 5;

        // Used to add artifical delays so that signals don't all update on the same frame.
        private static System.Random s_random = new System.Random();

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

            Definition.StartCoroutine(UpdateRoutine());
            Junction.Switched += JunctionSwitched;
        }

        private void JunctionSwitched(Junction.SwitchMode mode, int branch)
        {
            // Force update the display because of junction branch updates even if
            // the state didn't change.
            UpdateAspect();
            UpdateDisplays();
        }

        private System.Collections.IEnumerator UpdateRoutine()
        {
            TurnOff();

            // Wait for the player to load.
            while (!StartingItemsController.Instance.itemsLoaded)
            {
                yield return null;
            }

            // Prevent cluttering log by setting states too early. Also add random delay to ungroup start times.
            yield return new WaitForSeconds((float)(s_random.NextDouble() + 0.1));

            // Initial update.
            UpdateAspect();

            while (true)
            {
                yield return new WaitForSeconds(UpdateTime);

                // Instanced signal is gone, stop the routine.
                if (Definition == null)
                {
                    Junction.Switched -= JunctionSwitched;
                    yield break;
                }

                // No camera, no update.
                if (PlayerManager.ActiveCamera == null)
                {
                    continue;
                }

                var dist = GetCameraDistance();

                // If the camera is too far from the signal, skip updating.
                // If the camera is far, but the signal is relatively close, use a slowed update rate.
                if (dist > SkipDistanceSqr || (dist > OptimiseDistanceSqr && _updateDelay < MaxUpdateDelay))
                {
                    _updateDelay++;
                    continue;
                }

                UpdateAspect();
                _updateDelay = 0;
            }
        }

        private float GetCameraDistance()
        {
            return Helpers.DistanceSqr(Definition.transform.position, PlayerManager.ActiveCamera.transform.position);
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
