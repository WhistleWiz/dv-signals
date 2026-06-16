using Signals.Common.Aspects;
using Signals.Game.Util;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class DepartureAspect : AspectBase<DepartureAspectDefinition>
    {
        private HashSet<TrainCar> _stoppedCache;

        public DepartureAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _stoppedCache = new HashSet<TrainCar>();
        }

        public override bool MeetsConditions()
        {
            if (!JobHelper.JobsActive) return false;

            if (!Controller.PlacementInfo.HasValue) return false;

            if (Controller.Group == null) return false;

            var station = Controller.Group.Station;

            if (string.IsNullOrEmpty(station)) return false;

            var track = Controller.PlacementInfo.Value.Track;
            var trainCars = track.BogiesOnTrack().Select(x => x.Car);

            // Remove anything that isn't on the track anymore.
            _stoppedCache.RemoveWhere(x => !trainCars.Contains(x));

            foreach (var car in trainCars)
            {
                // Is stopped.
                if (car.GetAbsSpeed() < 0.1)
                {
                    _stoppedCache.Add(car);
                }
            }

            var logicCars = _stoppedCache.Select(x => x.logicCar).Where(x => x != null);

            if (!logicCars.Any()) return false;

            var jobs = JobHelper.GetActiveJobsForCars(logicCars, Definition.IncludeShunting);

            return ApplyInvert(jobs.Any(x => !x.chainData.chainDestinationYardId.Equals(station)), Definition.Invert);
        }
    }
}
