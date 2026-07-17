//using Signals.Common.Aspects;
//using Signals.Game.Railway;

//namespace Signals.Game.Aspects
//{
//    public class SpecialRequireReservationAspect : AspectBase<SpecialRequireReservationAspectDefinition>
//    {
//        public SpecialRequireReservationAspect(AspectBaseDefinition definition, Signal signal) : base(definition, signal) { }

//        public override bool MeetsConditions()
//        {
//            return SignalsMod.Settings.SpecialReservation && !TrackReserver.HasReservation(Signal);
//        }
//    }
//}
