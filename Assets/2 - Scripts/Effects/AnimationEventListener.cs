
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


/// <summary>
/// Defines an animation event which can be responded to.
/// </summary>
[System.Serializable]
public class AnimationEvent
{
    public string Event;
    public UnityEvent OnEvent;
}


/// <summary>
/// Listens for raised animation events on the same object
/// </summary>
public class AnimationEventListener : MonoBehaviour
{

    [SerializeField]
    private AnimationEvent[] _events;

    public void AnimationEvent( string evt )
    {
        _events.Where( e => e.Event == evt ).Do( x => x.OnEvent.Invoke() );
    }
}
