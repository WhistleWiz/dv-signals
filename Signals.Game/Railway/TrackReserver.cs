using Signals.Common;
using Signals.Game.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Railway
{
    public static class TrackReserver
    {
        private static Dictionary<RailTrack, BasicSignalController> s_reservations = new Dictionary<RailTrack, BasicSignalController>();
        private static Dictionary<BasicSignalController, Coroutine> s_clearRoutines = new Dictionary<BasicSignalController, Coroutine>();

        /// <summary>
        /// Called when a reservation is successfully made.
        /// </summary>
        public static Action<BasicSignalController>? ReservationMade;
        /// <summary>
        /// Called when a reservation is successfully cleared.
        /// </summary>
        public static Action<BasicSignalController>? ReservationCleared;

        /// <summary>
        /// Clears all track reservations.
        /// </summary>
        public static void ClearAll()
        {
            s_reservations.Clear();

            foreach (var routine in s_clearRoutines)
            {
                CoroutineManager.Instance.Stop(routine.Value);
            }

            s_clearRoutines.Clear();
        }

        /// <summary>
        /// Checks if a signal's tracks have been reserved by another one.
        /// </summary>
        /// <param name="signal">The signal to check.</param>
        /// <param name="crossingMode">How intersections with other tracks should be checked.</param>
        /// <returns><see langword="true"/> if another signal has reserved any of <paramref name="signal"/>'s tracks, <see langword="false"/> otherwise.</returns>
        public static bool IsSignalReservedByAnother(BasicSignalController signal, CrossingCheckMode crossingMode)
        {
            var block = signal.Block;

            if (block == null) return false;

            return block.AllTracks.Any(x => TrackChecker.IsReservedByAnother(x, signal, crossingMode));
        }

        /// <summary>
        /// Checks if a track is reserved by another signal.
        /// </summary>
        /// <param name="track">The track to check.</param>
        /// <param name="signal">The signal to check.</param>
        /// <returns><see langword="true"/> if the track is reserved by another signal, <see langword="false"/> otherwise.</returns>
        public static bool IsTrackReservedByAnother(RailTrack track, BasicSignalController signal)
        {
            return s_reservations.TryGetValue(track, out var by) && by != signal;
        }

        /// <summary>
        /// Checks if a track is reserved by a specific signal.
        /// </summary>
        /// <param name="track">The track to check.</param>
        /// <param name="signal">The signal to check.</param>
        /// <returns><see langword="true"/> if <paramref name="track"/> is reserved by <paramref name="signal"/>, <see langword="false"/> otherwise.</returns>
        public static bool IsTrackReservedBy(RailTrack track, BasicSignalController signal)
        {
            return s_reservations.TryGetValue(track, out var by) && by == signal;
        }

        /// <summary>
        /// Checks if a track is reserved.
        /// </summary>
        /// <param name="track">The track to check.</param>
        /// <param name="by">If reserved, which signal did it.</param>
        /// <returns><see langword="true"/> if <paramref name="track"/> is reserved, <see langword="false"/> otherwise.</returns>
        public static bool IsTrackReserved(RailTrack track, out BasicSignalController by)
        {
            return s_reservations.TryGetValue(track, out by);
        }

        /// <summary>
        /// Reserves a signal's tracks.
        /// </summary>
        /// <param name="signal">The signal reserving the tracks.</param>
        /// <returns><see langword="true"/> if the tracks were successfully reserved, <see langword="false"/> otherwise.</returns>
        /// <remarks>Any track can only be reserved by a single signal at once, so this method will fail if 2 reservations overlap.</remarks>
        public static bool ReserveForSignal(BasicSignalController signal)
        {
            if (signal.Block == null || IsSignalReservedByAnother(signal, CrossingCheckMode.WholeTrack))
            {
                return false;
            }

            foreach (var track in signal.Block.AllTracks)
            {
                if (!s_reservations.ContainsKey(track))
                {
                    s_reservations.Add(track, signal);
                }
            }

            ReservationMade?.Invoke(signal);
            return true;
        }

        /// <summary>
        /// Reserves a signal's tracks for some time.
        /// </summary>
        /// <param name="signal">The signal reserving the tracks.</param>
        /// <param name="duration">How long the reservation will last, in seconds. Must be larger than 0.</param>
        /// <returns><see langword="true"/> if the tracks were successfully reserved, <see langword="false"/> otherwise.</returns>
        /// <remarks>Any track can only be reserved by a single signal at once, so this method will fail if 2 reservations overlap.
        /// <para>If <paramref name="signal"/> already reserved tracks for a duration, the new duration will overwrite it.</para></remarks>
        public static bool ReserveForSignal(BasicSignalController signal, float duration)
        {
            if (duration <= 0)
            {
                Debug.LogError("Duration must be longer than 0");
                return false;
            }

            if (!ReserveForSignal(signal))
            {
                return false;
            }

            ClearFromSignalDelayed(signal, duration);
            return true;
        }

        /// <summary>
        /// Clear's all of a signal's reserved tracks.
        /// </summary>
        /// <param name="signal">The signal reserving the tracks.</param>
        public static void ClearFromSignal(BasicSignalController signal)
        {
            var reservedBy = s_reservations.Where(x => x.Value == signal).ToList();

            foreach (var item in reservedBy)
            {
                s_reservations.Remove(item.Key);
            }

            ReservationCleared?.Invoke(signal);
        }

        /// <summary>
        /// Clear's all of a signal's reserved tracks after some time.
        /// </summary>
        /// <param name="signal">The signal reserving the tracks.</param>
        /// <param name="delay">How long to wait until the tracks are cleared.</param>
        /// <remarks>If there is a delayed clearing for <paramref name="signal"/> already, it will be cancelled.</remarks>
        public static void ClearFromSignalDelayed(BasicSignalController signal, float delay)
        {
            if (s_clearRoutines.TryGetValue(signal, out var coroutine))
            {
                CoroutineManager.Instance.Stop(coroutine);
            }

            s_clearRoutines[signal] = CoroutineManager.Instance.StartCoroutine(ClearRoutine(signal, delay));
        }

        private static System.Collections.IEnumerator ClearRoutine(BasicSignalController signal, float delay)
        {
            yield return WaitFor.Seconds(delay);

            ClearFromSignal(signal);
            s_clearRoutines.Remove(signal);
        }
    }
}
