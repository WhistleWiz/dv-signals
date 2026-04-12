using Signals.Game.Controllers;
using UnityEngine;

namespace Signals.Game
{
    internal class SignalDefinitionToController : MonoBehaviour
    {
        public BasicSignalController Controller = null!;

        public static SignalDefinitionToController? AddToDef(BasicSignalController controller)
        {
            var def = controller.Definition;

            if (def == null)
            {
                Debug.LogError($"Definition for {controller.Id} is null!");
                return null;
            }

            var comp = def.gameObject.AddComponent<SignalDefinitionToController>();
            comp.Controller = controller;
            return comp;
        }
    }
}
