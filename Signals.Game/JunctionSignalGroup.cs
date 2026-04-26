using Signals.Game.Controllers;
using Signals.Game.Railway;
using System.Collections.Generic;

namespace Signals.Game
{
    public class JunctionSignalGroup
    {
        private string? _stationId;

        public Junction Junction { get; private set; }
        public JunctionSignalController? JunctionSignal;
        public TrackSignalController? ReverseJunctionSignal;
        public List<TrackSignalController> BranchSignals;

        public string Station
        {
            get
            {
                _stationId ??= TrackUtils.JunctionStation(Junction);

                return _stationId;
            }
        }

        public IEnumerable<TrackSignalController> AllSignals
        {
            get
            {
                if (JunctionSignal != null) yield return JunctionSignal;
                if (ReverseJunctionSignal != null) yield return ReverseJunctionSignal;

                foreach (var item in BranchSignals)
                {
                    yield return item;
                }
            }
        }

        public JunctionSignalGroup(Junction junction)
        {
            Junction = junction;
            BranchSignals = new List<TrackSignalController>();
        }

        public JunctionSignalGroup(Junction junction, JunctionSignalController? junctionSignal, List<TrackSignalController> branchSignals)
        {
            Junction = junction;
            JunctionSignal = junctionSignal;
            BranchSignals = branchSignals;

            AssignSelfToSignals();
        }

        public JunctionSignalGroup(Junction junction, JunctionSignalController? junctionSignal, TrackSignalController? reverseJunctionSignal)
        {
            Junction = junction;
            JunctionSignal = junctionSignal;
            ReverseJunctionSignal = reverseJunctionSignal;
            BranchSignals = new List<TrackSignalController>();

            AssignSelfToSignals();
        }

        private void AssignSelfToSignals()
        {
            if (JunctionSignal != null) JunctionSignal.Group = this;
            if (ReverseJunctionSignal != null) ReverseJunctionSignal.Group = this;

            BranchSignals.ForEach(x => x.Group = this);
        }

        /// <summary>
        /// Checks if there is a controller at the specified junction branch track.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="signal"></param>
        /// <returns></returns>
        public bool TryGetControllerForTrack(RailTrack track, out TrackSignalController signal)
        {
            signal = BranchSignals.Find(x => x.StartingTrack == track);
            return signal != null;
        }
    }
}
