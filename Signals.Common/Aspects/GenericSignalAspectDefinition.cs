using UnityEngine;

namespace Signals.Common.Aspects
{
    public abstract class GenericSignalAspectDefinition : SignalAspectBaseDefinition
    {
        [Header("Required - IDs")]
        [SerializeField]
        private string _id = string.Empty;

        public override string Id => _id;
    }
}
