using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class JunctionBranchAspect : AspectBase<JunctionBranchAspectDefinition>
    {
        public JunctionBranchAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

        public override bool MeetsConditions()
        {
            var junction = Controller.GroupJunction;

            if (junction == null) return false;

            return Definition.Mode switch
            {
                JunctionBranchAspectDefinition.JunctionAspectMode.ActiveOnThrough =>
                    junction.IsSetToThrough(),
                JunctionBranchAspectDefinition.JunctionAspectMode.ActiveOnDiverging =>
                    !junction.IsSetToThrough(),
                JunctionBranchAspectDefinition.JunctionAspectMode.ActiveOnBranch =>
                    junction.selectedBranch == Definition.ActiveOnBranch,
                _ => false,
            };
        }
    }
}
