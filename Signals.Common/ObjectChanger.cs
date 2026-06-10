using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Object Changer")]
    public class ObjectChanger : MonoBehaviour
    {
        public GameObject[] EnableObjects = new GameObject[0];
        public GameObject[] DisableObjects = new GameObject[0];

        public void Apply()
        {
            ApplyTo(EnableObjects, true);
            ApplyTo(DisableObjects, false);
        }

        public void Unapply()
        {
            ApplyTo(EnableObjects, false);
            ApplyTo(DisableObjects, true);
        }

        private static void ApplyTo(GameObject[] array, bool state)
        {
            foreach(var item in array)
            {
                item.SetActive(state);
            }
        }
    }
}
