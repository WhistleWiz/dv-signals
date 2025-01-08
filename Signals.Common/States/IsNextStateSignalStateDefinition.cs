using UnityEngine;

namespace Signals.Common.States
{
    public class IsNextStateSignalStateDefinition : GenericSignalStateDefinition
    {
        [SerializeField]
        private string _nextId = string.Empty;

        public string NextId => _nextId;
    }
}
