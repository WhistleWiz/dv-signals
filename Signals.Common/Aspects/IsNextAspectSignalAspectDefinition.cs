using UnityEngine;

namespace Signals.Common.Aspects
{
    public class IsNextAspectSignalAspectDefinition : GenericSignalAspectDefinition
    {
        [SerializeField]
        private string _nextId = string.Empty;

        public string NextId => _nextId;
    }
}
