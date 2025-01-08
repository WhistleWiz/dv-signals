using UnityEngine;

namespace Signals.Common.States
{
    public abstract class GenericSignalStateDefinition : SignalStateBaseDefinition
    {
        [SerializeField]
        private string _id = string.Empty;

        public override string Id => _id;
    }
}
