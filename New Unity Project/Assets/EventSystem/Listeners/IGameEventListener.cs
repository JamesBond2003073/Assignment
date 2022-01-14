using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Totality.GameTemplate
{
    public interface IGameEventListener<T> 
    {

        void OnEventRaised(T item);
    }
}
