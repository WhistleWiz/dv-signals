using Signals.Common;

namespace Signals.Game
{
    internal static class Extensions
    {
        public static SignalLight GetController(this SignalLightDefinition definition)
        {
            if (!definition.TryGetComponent(out SignalLight controller))
            {
                controller = definition.gameObject.AddComponent<SignalLight>();
                controller.Initialize(definition);
            }

            return controller;
        }
    }
}
