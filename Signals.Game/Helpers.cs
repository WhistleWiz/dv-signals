using UnityEngine;

namespace Signals.Game
{
    internal class Helpers
    {
        public static System.Collections.IEnumerator DisableBehaviour(Behaviour behaviour, float time)
        {
            yield return new WaitForSeconds(time);
            behaviour.enabled = false;
        }
    }
}
