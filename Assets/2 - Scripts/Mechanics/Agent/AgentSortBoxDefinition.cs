using System;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class AgentSortBox : IAgentBehavior, ICloneable
{
    public Agent Agent { get; private set; }

    [SerializeField]
    private float _speed;

    public IEntity Target;

    private Grabbable _grabbedObject;

    private PathFollower _pathFollower;
    private bool _isComplete;
    private SortDestination[] _sortDestinations;

    public bool CanActivate()
    {
        if( Target == null )
            return false;
        if( Target.GetQuality<Grabbable>() == null )
            return false;
        if( Target.GetQuality<Sortable>() == null )
            return false;
        return true;
    }

    public object Clone()
    {
        AgentSortBox clone = new AgentSortBox();
        clone.Agent = this.Agent;
        return clone;
    }

    public void Initialize( Agent agent )
    {
        this.Agent = agent;
        _sortDestinations = GameObject.FindObjectsByType<SortDestination>( FindObjectsSortMode.None );
    }

    public bool IsComplete()
    {
        return _isComplete;
    }

    public void OnActivate()
    {
        _grabbedObject = Target.GetQuality<Grabbable>();
        _grabbedObject.transform.SetParent( Agent.transform );

        var sortable = Target.GetQuality<Sortable>();
        var destination = GetDestination( sortable.SortType );
        _pathFollower = Agent.PathToLocation( destination.transform.position );
    }

    private SortDestination GetDestination( SortType sort )
    {
        return _sortDestinations.Where( x => x.SortType == sort ).FirstOrDefault();
    }


    public void OnDeactivate()
    {
        _pathFollower = null;
        _isComplete = false;
    }


    public void Update()
    {
        if( _pathFollower.IsComplete )
        {
            _isComplete = true;
            _grabbedObject.transform.SetParent( null );
        }
    }
}




[CreateAssetMenu( fileName = "Agent Sort Box", menuName = "Boxy/Behaviors/Agent Sort Behavior" )]
public class AgentSortBoxDefinition : AgentBehaviorDefinition<AgentSortBox>
{

}
