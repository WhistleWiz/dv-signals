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
        private const float OptimiseDistance = 2000;
        private const float OptimiseDistanceSqr = OptimiseDistance * OptimiseDistance;
        private const float UpdateTime = 1.0f;
        private const float LongUpdateTime = 5.0f;

        // Used to add artifical delays so that signals don't all update on the same frame.
        private static System.Random s_random = new System.Random();

        public Junction Junction { get; protected set; }
        /// <summary>
        /// Whether the signal refers to the junction's branches or the inbound track.
        /// </summary>
        public bool TowardsBranches { get; protected set; }

        public override string Name => $"{Junction.junctionData.junctionIdLong}-{(TowardsBranches ? 'T' : 'F')}";

        public JunctionSignalController(SignalControllerDefinition def, Junction junction, bool direction) : base(def)
        {
            Junction = junction;
            TowardsBranches = direction;

            Definition.StartCoroutine(UpdateRoutine());
            Junction.Switched += JunctionSwitched;
        }

        private void JunctionSwitched(Junction.SwitchMode mode, int branch)
        {
            if (Definition == null)
            {
                Junction.Switched -= JunctionSwitched;
                return;
            }

            UpdateAspect();
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

            while (true)
            {
                UpdateAspect();
                yield return new WaitForSeconds(GetUpdateTime());
            }
        }

        private float GetUpdateTime()
        {
            if (PlayerManager.ActiveCamera != null &&
                Helpers.DistanceSqr(Definition.transform.position, PlayerManager.ActiveCamera.transform.position) > OptimiseDistanceSqr)
            {
                return LongUpdateTime;
            }

            return UpdateTime;
        }

        /// <summary>
        /// Updates the current aspect based on the conditions of <see cref="AllAspects"/>.
        /// </summary>
        public override BasicSignalController? UpdateAspect()
        {
            // Precompute this information so each state doesn't have to call the same functions
            // over and over again.
            var info = TrackWalker.WalkUntilNextSignal(this);

            for (int i = 0; i < AllAspects.Length; i++)
            {
                if (AllAspects[i].MeetsConditions(info))
                {
                    ChangeAspect(i);
                    return info.NextMainlineSignal;
                }
            }

            // Turn off if no conditions are met.
            TurnOff();
            return info.NextMainlineSignal;
        }

        /// <summary>
        /// Returns the other signal at the assigned junction.
        /// </summary>
        public JunctionSignalController? GetPaired()
        {
            SignalManager.Instance.TryGetSignal(Junction, !TowardsBranches, out var controller);
            return controller;
        }
    }
}
