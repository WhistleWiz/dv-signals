using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Signals.Game.Railway
{
    /// <summary>
    /// Class for making and checking track reservations.
    /// </summary>
    /// <remarks>
    /// Tracks reserved by different signals in the same controller do not interfere with eachother.
    /// </remarks>
    public static class TrackReserver
    {
        private static readonly Dictionary<RailTrack, Signal> s_reservations = new Dictionary<RailTrack, Signal>();
        private static readonly Dictionary<Signal, Coroutine> s_clearRoutines = new Dictionary<Signal, Coroutine>();
        private static readonly HashSet<Signal> s_signals = new HashSet<Signal>();

        /// <summary>
        /// Called when a reservation is successfully made.
        /// </summary>
        public static Action<Signal>? ReservationMade;
        /// <summary>
        /// Called when a reservation is successfully cleared.
        /// </summary>
        public static Action<Signal>? ReservationCleared;

        /// <summary>
        /// Clears all track reservations.
        /// </summary>
        public static void ClearAll()
        {
            foreach (var routine in s_clearRoutines)
            {
                CoroutineManager.Instance.Stop(routine.Value);
            }

            s_reservations.Clear();
            s_clearRoutines.Clear();
            s_signals.Clear();
        }

        /// <summary>
        /// Checks if a signal's tracks have been reserved by another one.
        /// </summary>
        /// <param name="signal">The signal to check.</param>
        /// <param name="crossingMode">How intersections with other tracks should be checked.</param>
        /// <returns><see langword="true"/> if another signal has reserved any of <paramref name="signal"/>'s tracks, <see langword="false"/> otherwise.</returns>
        public static bool IsSignalReservedByAnother(Signal signal)
        {
            var block = signal.Block;

            if (block == null) return false;

            return block.AllTracks.Any(x => TrackChecker.IsReservedByAnother(x, signal));
        }

        /// <summary>
        /// Checks if a track is reserved by a signal from another controller.
        /// </summary>
        /// <param name="track">The track to check.</param>
        /// <param name="signal">The signal to check.</param>
        /// <returns><see langword="true"/> if the track is reserved by a signal from another controller, <see langword="false"/> otherwise.</returns>
        public static bool IsTrackReservedByAnother(RailTrack track, Signal signal)
        {
            return s_reservations.TryGetValue(track, out var by) && by.Controller != signal.Controller;
        }

        /// <summary>
        /// Checks if a track is reserved by a specific signal.
        /// </summary>
        /// <param name="track">The track to check.</param>
        /// <param name="signal">The signal to check.</param>
        /// <returns><see langword="true"/> if <paramref name="track"/> is reserved by <paramref name="signal"/>, <see langword="false"/> otherwise.</returns>
        public static bool IsTrackReservedBy(RailTrack track, Signal signal)
        {
            return s_reservations.TryGetValue(track, out var by) && by == signal;
        }

        /// <summary>
        /// Checks if a track is reserved.
        /// </summary>
        /// <param name="track">The track to check.</param>
        /// <param name="by">If reserved, which signal did it.</param>
        /// <returns><see langword="true"/> if <paramref name="track"/> is reserved, <see langword="false"/> otherwise.</returns>
        public static bool IsTrackReserved(RailTrack track, out Signal by)
        {
            return s_reservations.TryGetValue(track, out by);
        }

        /// <summary>
        /// Checks if a signal already reserved tracks.
        /// </summary>
        /// <param name="signal">The signal to check.</param>
        /// <returns><see langword="true"/> if <paramref name="signal"/> has any track reservations, <see langword="false"/> otherwise.</returns>
        public static bool HasReservation(Signal signal)
        {
            return s_signals.Contains(signal);
        }

        /// <summary>
        /// Reserves a signal's tracks.
        /// </summary>
        /// <param name="controller">The signal reserving the tracks.</param>
        /// <returns><see langword="true"/> if the tracks were successfully reserved, <see langword="false"/> otherwise.</returns>
        /// <remarks>Any track can only be reserved by a single signal at once, so this method will fail if 2 reservations overlap.
        /// <para>If the signal has already reserved tracks, they will be cleared before being reserved again.</para></remarks>
        public static bool ReserveForSignal(Signal signal)
        {
            if (signal.Block == null || IsSignalReservedByAnother(signal))
            {
                return false;
            }

            if (HasReservation(signal))
            {
                ClearFromSignal(signal);
            }

            var hasTracks = false;

            foreach (var track in signal.Block.AllTracks)
            {
                if (!s_reservations.ContainsKey(track))
                {
                    s_reservations.Add(track, signal);
                    hasTracks = true;
                }
            }

            // Check if any track was actually reserved.
            if (hasTracks == false)
            {
                return false;
            }

            s_signals.Add(signal);
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
        public static bool ReserveForSignal(Signal signal, float duration)
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
        /// Clears all of a signal's reserved tracks.
        /// </summary>
        /// <param name="signal">The signal reserving the tracks.</param>
        public static void ClearFromSignal(Signal signal)
        {
            var reservedBy = s_reservations.Where(x => x.Value == signal).ToList();

            // If there's no reserved tracks, don't even invoke the event.
            if (reservedBy.Any()) return;

            foreach (var item in reservedBy)
            {
                s_reservations.Remove(item.Key);
            }

            s_signals.Remove(signal);
            ReservationCleared?.Invoke(signal);
        }

        /// <summary>
        /// Clear's all of a signal's reserved tracks after some time.
        /// </summary>
        /// <param name="signal">The signal reserving the tracks.</param>
        /// <param name="delay">How long to wait until the tracks are cleared.</param>
        /// <remarks>If there is a delayed clearing for <paramref name="signal"/> already, it will be cancelled.</remarks>
        public static void ClearFromSignalDelayed(Signal signal, float delay)
        {
            if (s_clearRoutines.TryGetValue(signal, out var coroutine))
            {
                CoroutineManager.Instance.Stop(coroutine);
            }

            s_clearRoutines[signal] = CoroutineManager.Instance.StartCoroutine(ClearRoutine(signal, delay));
        }

        public static bool UpdateReservation(Signal signal)
        {
            if (!HasReservation(signal) || signal.Block == null) return false;

            foreach (var track in signal.Block.AllTracks)
            {
                // This means the reservation update would overlap with another, so it is rejected.
                if (TrackChecker.IsReservedByAnother(track, signal))
                {
                    return false;
                }
            }

            ClearFromSignal(signal);
            ReserveForSignal(signal);

            return true;
        }

        private static System.Collections.IEnumerator ClearRoutine(Signal signal, float delay)
        {
            yield return WaitFor.Seconds(delay);

            ClearFromSignal(signal);
            s_clearRoutines.Remove(signal);
        }
    }
}
