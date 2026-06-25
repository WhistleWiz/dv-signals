using UnityEngine;

namespace Signals.Common
{
    [AddComponentMenu("DV Signals/Transform Mover Target")]
    public class TransformMoverTarget : MonoBehaviour
    {
        public TransformMover Mover = null!;
        public Vector3 Position = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
        public Vector3 Scale = Vector3.one;

        public void Reset()
        {
            Position = transform.localPosition;
            Rotation = transform.localRotation.eulerAngles;
            Scale = transform.localScale;
        }

        public void Apply()
        {
            Mover.SetTarget(this);
        }

        public void Unapply()
        {
            Mover.SetTarget(null);
        }
    }
}
