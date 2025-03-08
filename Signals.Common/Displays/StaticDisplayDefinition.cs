namespace Signals.Common.Displays
{
    public class StaticDisplayDefinition : InfoDisplayDefinition
    {
        public string DisplayedText = string.Empty;

        private void Reset()
        {
            Mode = UpdateMode.AtStart;
        }
    }
}
