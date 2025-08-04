using System;
using System.Linq;
using UnityEngine;


/// <summary>
/// Sorting behavior for agents
/// </summary>
[System.Serializable]
public class AgentSortBox : IAgentBehavior, ICloneable
{
    const float RANDOM_VERTICAL_OFFSET = 3;
    const float BOX_HOLD_DISTANCE = .4f;

    [SerializeField]
    private float _speed;

    public IEntity Target;

    private Grabbable _grabbedObject;
    private PathFollower _pathFollower;
    private bool _isComplete;
    private SortDestination[] _sortDestinations;

    public Agent Agent { get; private set; }


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

        var r = _grabbedObject.GetComponent<Rigidbody2D>();
        r.simulated = false;
        _grabbedObject.transform.SetParent( Agent.transform );

        var sortable = Target.GetQuality<Sortable>();
        var destination = GetDestination( sortable.SortType );
        Vector3 randomOffset  = Vector3.up * ( UnityEngine.Random.value - .5f ) * 2f * RANDOM_VERTICAL_OFFSET;
        _pathFollower = Agent.PathToLocation( destination.transform.position + randomOffset );
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
        _grabbedObject.transform.localPosition = Vector3.Lerp( _grabbedObject.transform.localPosition, Agent.Velocity.normalized * BOX_HOLD_DISTANCE, Time.deltaTime * 3f );

        if( _pathFollower == null || _pathFollower.IsComplete )
        {
            var r = _grabbedObject.GetComponent<Rigidbody2D>();
            r.simulated = true;
            _isComplete = true;
            _grabbedObject.transform.SetParent( null );
        }
    }
}




[CreateAssetMenu( fileName = "Agent Sort Box", menuName = "Boxy/Behaviors/Agent Sort Behavior" )]
public class AgentSortBoxDefinition : AgentBehaviorDefinition<AgentSortBox>
{

}
