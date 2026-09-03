using Signals.Common;
using Signals.Game.Railway;
using System.Collections.Generic;

namespace Signals.Game.Controllers
{
    /// <summary>
    /// A controller for a signal placed at a junction.
    /// </summary>
    /// <remarks>
    /// Switching the junction automatically updates the signal.
    /// </remarks>
    public class JunctionSignalController : TrackSignalController
    {
        private bool _junctionFlag = false;

        /// <summary>
        /// If not <see langword="null"/>, the block will start at the specified track rather than the junction branches.
        /// </summary>
        public RailTrack? OverrideStart;

        public bool Left { get; protected set; }
        public Junction Junction { get; protected set; }

        public JunctionSignalController(SignalControllerDefinition def, Junction junction, RailTrack? starting, SignalPlacementInfo info) :
            base(def, starting ?? junction.GetCurrentBranch().track, TrackDirection.Out, info)
        {
            OverrideStart = starting;

            Junction = junction;
            Left = junction.IsLeft();

            Junction.Switched += JunctionSwitched;
            Destroyed += (x) => Junction.Switched -= JunctionSwitched;
        }

        private void JunctionSwitched(Junction.SwitchMode mode, int branch)
        {
            // This forces the track block to be considered as changed.
            _junctionFlag = true;

            // Force update the display because of junction branch updates even if
            // the state didn't change.
            Update(true, true);
        }

        public override Signal? GetActiveSignal()
        {
            if (Signals.Length == 0) return null;

            return Signals[Junction.selectedBranch % Signals.Length];
        }

        public override Signal? GetControllerShuntingSignal()
        {
            if (ShuntingSignals.Length == 0) return null;

            return ShuntingSignals[Junction.selectedBranch % ShuntingSignals.Length];
        }

        public override bool UpdateBlocks()
        {
            var updated = false;
            StartingTrack = OverrideStart ?? Junction.GetCurrentBranch().track;

            if (Signals.Length == 1)
            {
                updated |= CreateSingle(Signals[0], Type);
            }
            else
            {
                updated |= CreateMulti(Signals, Type);
            }

            if (ShuntingSignals.Length == 1)
            {
                updated |= CreateSingle(ShuntingSignals[0], SignalType.Shunting);
            }
            else
            {
                updated |= CreateMulti(ShuntingSignals, SignalType.Shunting);
            }

            _junctionFlag = false;
            return updated;

            bool CreateSingle(Signal signal, SignalType type)
            {
                var block = signal.Block;

                if (block != null && !block.ShouldBeUpdated && !_junctionFlag) return false;

                block = type switch
                {
                    SignalType.Spacing => TrackBlock.CreateForSpacing(StartingTrack, Direction, this),
                    SignalType.Shunting => TrackBlock.CreateForShunting(StartingTrack, Direction, this),
                    _ => TrackBlock.CreateUntilMainSignal(StartingTrack, Direction, this),
                };

                signal.SetBlock(block);
                return true;
            }

            bool CreateMulti(Signal[] signals, SignalType type)
            {
                var selected = Junction.selectedBranch;
                var anyUpdated = false;

                for (byte i = 0; i < signals.Length; i++)
                {
                    var block = signals[i].Block;

                    // No need to remake the block in this case.
                    if (block != null && !block.ShouldBeUpdated && !_junctionFlag) continue;

                    Junction.selectedBranch = (byte)(i % Junction.outBranches.Count);
                    var track = OverrideStart ?? Junction.GetCurrentBranch().track;
                    block = type switch
                    {
                        SignalType.Spacing => TrackBlock.CreateForSpacing(track, Direction, this),
                        SignalType.Shunting => TrackBlock.CreateForShunting(track, Direction, this),
                        _ => TrackBlock.CreateUntilMainSignal(track, Direction, this),
                    };

                    signals[i].SetBlock(block);
                    anyUpdated = true;
                }

                Junction.selectedBranch = selected;
                return anyUpdated;
            }
        }

        public override IEnumerable<(Signal Signal, Dictionary<BasicSignalController, float> Controllers)> GetPotentialNextControllers()
        {
            if (Signals.Length == 1)
            {
                var controllers = new Dictionary<BasicSignalController, float>();

                foreach (var branch in Junction.outBranches)
                {
                    var more = TrackWalker.GetAllPossibleMainControllers(branch.track, Direction, this);

                    foreach (var item in more)
                    {
                        if (controllers.ContainsKey(item.Key)) continue;

                        controllers.Add(item.Key, item.Value);
                    }
                }

                yield return (Signals[0], controllers);
            }
            else
            {
                var selected = Junction.selectedBranch;

                for (byte i = 0; i < Signals.Length; i++)
                {
                    Junction.selectedBranch = (byte)(i % Junction.outBranches.Count);
                    var track = OverrideStart ?? Junction.GetCurrentBranch().track;
                    yield return (Signals[i], TrackWalker.GetAllPossibleMainControllers(track, Direction, this));
                }

                Junction.selectedBranch = selected;
            }
        }

        public static JunctionSignalController? Replace(JunctionSignalController original, SignalControllerDefinition def)
        {
            if (!original.PlacementInfo.HasValue) return null;

            var replacement = new JunctionSignalController(def, original.Junction, original.OverrideStart, original.PlacementInfo.Value)
            {
                ActingAsDistant = original.ActingAsDistant,
                ShortDistance = original.ShortDistance
            };

            var group = original.Group;

            if (group != null)
            {
                replacement.Group = group;

                if (group.JunctionSignal == original)
                {
                    group.JunctionSignal = replacement;
                }
                if (group.ReverseJunctionSignal == original)
                {
                    group.ReverseJunctionSignal = replacement;
                }
                if (group.BranchSignals.Contains(original))
                {
                    group.BranchSignals.Add(replacement);
                }
            }

            original.Destroy();
            return replacement;
        }
    }
}
