
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


[System.Serializable]
public class AnimationEvent
{
    public string Event;
    public UnityEvent OnEvent;
}


public class AnimationEventListener : MonoBehaviour
{

    [SerializeField]
    private AnimationEvent[] _events;

    public void AnimationEvent( string evt )
    {
        _events.Where( e => e.Event == evt ).Do( x => x.OnEvent.Invoke() );
    }
}
