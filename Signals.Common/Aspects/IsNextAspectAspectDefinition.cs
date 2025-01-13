namespace Signals.Common.Aspects
{
    public class IsNextAspectAspectDefinition : AspectBaseDefinition
    {
        public string NextId = string.Empty;

        private void Reset()
        {
            Id = "NEXT_ASPECT";
        }
    }
}
