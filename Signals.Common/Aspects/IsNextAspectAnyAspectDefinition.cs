namespace Signals.Common.Aspects
{
    public class IsNextAspectAnyAspectDefinition : AspectBaseDefinition
    {
        public string[] NextIds = new string[0];

        private void Reset()
        {
            Id = "NEXT_ANY_ASPECT";
        }
    }
}
