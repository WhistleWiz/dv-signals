using Signals.Common.Aspects;
using Signals.Game.Util;
using System.Linq;

namespace Signals.Game.Aspects
{
    public class DepartureAspect : AspectBase<DepartureAspectDefinition>
    {
        public DepartureAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            if (!JobHelper.JobsActive) return false;

            if (!Controller.PlacementInfo.HasValue) return false;

            var junction = Controller.GroupJunction;

            if (junction == null) return false;

            var station = junction.GetStation();

            if (string.IsNullOrEmpty(station)) return false;

            var track = Controller.PlacementInfo.Value.Track;
            var cars = track.BogiesOnTrack().Select(x => x.Car.logicCar).Where(x => x != null);

            if (!cars.Any()) return false;

            var jobs = JobHelper.GetActiveJobsForCars(cars, Definition.IncludeShunting);

            return ApplyInvert(jobs.Any(x => !x.chainData.chainDestinationYardId.Equals(station)), Definition.Invert);
        }
    }
}
