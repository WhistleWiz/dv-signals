using Signals.Common.Aspects;

namespace Signals.Game.Aspects
{
    public class JunctionBranchAspect : AspectBase
    {
        private JunctionBranchAspectDefinition _fullDef;

        public JunctionBranchAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal)
        {
            _fullDef = (JunctionBranchAspectDefinition)definition;
        }

        public override bool MeetsConditions()
        {
            var junction = Controller.GroupJunction;

            if (junction == null) return false;

            return _fullDef.Mode switch
            {
                JunctionBranchAspectDefinition.JunctionAspectMode.ActiveOnThrough =>
                    junction.IsSetToThrough(),
                JunctionBranchAspectDefinition.JunctionAspectMode.ActiveOnDiverging =>
                    !junction.IsSetToThrough(),
                JunctionBranchAspectDefinition.JunctionAspectMode.ActiveOnBranch =>
                    junction.selectedBranch == _fullDef.ActiveOnBranch,
                _ => false,
            };
        }
    }
}
