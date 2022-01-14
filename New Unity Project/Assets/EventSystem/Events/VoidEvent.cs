using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Totality.GameTemplate
{
    [CreateAssetMenu (fileName = "New Void Event", menuName = "GameTemplate/Events/Void Event")]
    public class VoidEvent : BaseGameEvent<Void>
    {
        public void Raise() => Raise(new Void());
    }

}
