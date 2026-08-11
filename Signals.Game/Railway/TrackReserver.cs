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
        private static readonly Dictionary<Signal, float> s_times = new Dictionary<Signal, float>();
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
                if (routine.Value != null)
                {
                    CoroutineManager.Instance.Stop(routine.Value);
                }
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
            return s_reservations.TryGetValue(track, out var by) && by.Controller != signal.Controller && CheckShuntingReserveAnother(signal, by);
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
        /// Checks if a signal already reserved tracks.
        /// </summary>
        /// <param name="signal">The signal to check.</param>
        /// <param name="time">The time left in the reservation if the reservation exists and will be cleared automatically, or <c>-1</c> otherwise.</param>
        /// <returns><see langword="true"/> if <paramref name="signal"/> has any track reservations, <see langword="false"/> otherwise.</returns>
        public static bool HasReservation(Signal signal, out float time)
        {
            if (!s_times.TryGetValue(signal, out time))
            {
                time = -1;
            }

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
            signal.Controller.UpdateBlocks();

            if (signal.Block == null || IsSignalReservedByAnother(signal))
            {
                SignalsMod.Warning("Reservation failed: block was null or overlapped reservation");
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
                SignalsMod.Warning("Reservation failed: no tracks could be reserved");
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
            // Wrap since the clear routine flag is for internal use only,
            // to avoid remaking the routine when updating a reservation.
            ClearFromSignalInternal(signal, true);
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

            // Maintain time from the previous reservation by not clearing the routine.
            ClearFromSignalInternal(signal, false);
            ReserveForSignal(signal);

            return true;
        }

        /// <returns>All signal IDs with reservations.</returns>
        public static IEnumerable<int> GetSignalIdsWithReservations()
        {
            foreach (var item in s_signals)
            {
                yield return item.Id;
            }
        }

        private static void ClearFromSignalInternal(Signal signal, bool clearRoutine)
        {
            var reservedBy = s_reservations.Where(x => x.Value == signal).ToList();

            // If there's no reserved tracks, don't even invoke the event.
            if (!reservedBy.Any()) return;

            foreach (var item in reservedBy)
            {
                s_reservations.Remove(item.Key);
            }

            if (clearRoutine)
            {
                if (s_clearRoutines.TryGetValue(signal, out var coroutine))
                {
                    CoroutineManager.Instance.Stop(coroutine);
                    s_clearRoutines.Remove(signal);
                }

                s_times.Remove(signal);
            }

            s_signals.Remove(signal);
            ReservationCleared?.Invoke(signal);
        }

        private static bool CheckShuntingReserveAnother(Signal reserving, Signal reserved)
        {
            // Only applies to shunting signals.
            if (!reserving.IsShunting) return true;

            // We know the blocks aren't null at this point.
            var block = reserving.Block!;
            var otherTracks = reserved.Block!.Tracks;

            foreach (var track in block.Tracks)
            {
                var check = true;

                for (int i = 0; i < otherTracks.Length; i++)
                {
                    if (otherTracks[i].Track == track.Track)
                    {
                        // If we have overlapping tracks, they must be in the same direction.
                        if (otherTracks[i].Direction != track.Direction)
                        {
                            return true;
                        }

                        check = false;
                    }
                }

                // If the check wasn't triggered, that means the block isn't contained entirely
                // within the main signal, so block the reservation.
                if (check) return true;
            }

            // Shunting block was contained entirely within the main block, so don't count as reserved.
            return false;
        }

        private static System.Collections.IEnumerator ClearRoutine(Signal signal, float delay)
        {
            while (delay > 0)
            {
                s_times[signal] = delay -= Time.deltaTime;
                yield return null;
            }

            if (MultiplayerIntegration.IsHost)
            {
                MultiplayerIntegration.SendReservationCancelRequest(signal);
            }
            else
            {
                ClearFromSignal(signal);
            }
        }
    }
}
