using DV.Logic.Job;
using DV.ThingTypes;
using System.Collections.Generic;
using System.Linq;

namespace Signals.Game.Util
{
    public static class JobHelper
    {
        internal static bool ManagerInstanced = false;

        public static bool JobsActive => ManagerInstanced;

        public static HashSet<Job> GetActiveJobsForCars(IEnumerable<Car> cars, bool includeShunting)
        {
            if (!ManagerInstanced) return new HashSet<Job>();

            var manager = JobsManager.Instance;

            if (!includeShunting)
            {
                return cars
                    .Select(x => manager.GetJobOfCar(x, true))
                    .Where(x => x != null && x.jobType != JobType.ShuntingLoad && x.jobType != JobType.ShuntingUnload)
                    .ToHashSet();
            }

            return cars.Select(x => manager.GetJobOfCar(x, true)).Where(x => x != null).ToHashSet();
        }
    }
}
