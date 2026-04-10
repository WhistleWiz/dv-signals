using Signals.Game.Controllers;
using UnityEngine;

namespace Signals.Game
{
    internal class DebugComp : MonoBehaviour
    {
        public BasicSignalController? Controller;

        public static DebugComp? AddToDef(BasicSignalController controller)
        {
            var def = controller.Definition;

            if (def == null)
            {
                Debug.LogError($"Definition for {controller.Id} is null!");
                return null;
            }

            var comp = def.gameObject.AddComponent<DebugComp>();
            comp.Controller = controller;
            return comp;
        }
    }
}
