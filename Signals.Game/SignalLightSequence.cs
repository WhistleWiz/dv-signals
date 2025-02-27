using Signals.Common;
using System.Collections;
using UnityEngine;

namespace Signals.Game
{
    public class SignalLightSequence : MonoBehaviour
    {
        private (SignalLight? Light, bool InitialState)[] _lights = null!;
        private Coroutine? _coro;

        public SignalLightSequenceDefinition Definition = null!;

        public void Initialize(SignalLightSequenceDefinition definition)
        {
            Definition = definition;

            int length = Definition.Lights.Length;
            _lights = new (SignalLight? Light, bool InitialState)[length];

            for (int i = 0; i < length; i++)
            {
                var light = Definition.Lights[i];

                _lights[i] = light != null ? (light.GetController(), Definition.States[i]) : (null, Definition.States[i]);
            }
        }

        private IEnumerator SequenceRolling()
        {
            int offset = 0;
            int length = _lights.Length;

            while (true)
            {
                for (int i = 0; i < length; i++)
                {
                    var light = _lights[(offset + i) % length];

                    if (light.Light == null) continue; 

                    if (_lights[i].InitialState)
                    {
                        light.Light.TurnOn();
                    }
                    else
                    {
                        light.Light.TurnOff();
                    }
                }

                offset = (offset + 1) % length;

                yield return new WaitForSeconds(Definition.Timing);
            }
        }

        public void Activate()
        {
            if (_coro != null) return;

            _coro = StartCoroutine(SequenceRolling());
        }

        public void Deactivate()
        {
            if (_coro == null) return;

            StopCoroutine(_coro);
            _coro = null;

            foreach (var light in _lights)
            {
                if (light.Light == null) continue;

                light.Light.TurnOff();
            }
        }
    }
}
