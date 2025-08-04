using System;
using UnityEngine;


/// <summary>
/// Walk behavior for agents
/// </summary>
[System.Serializable]
public class AgentWalk : IAgentBehavior, ICloneable
{

    [SerializeField]
    private float _speed;

    private PathFollower _pathFollower;
    private bool _isComplete;

    public AgentWalk( AgentWalk other )
    {
        this.Agent = other.Agent;
        this._speed = other._speed;
    }

    public Agent Agent { get; private set; }

    public Vector3 Destination { get; set; }

    public bool CanActivate()
    {
        return true;
    }

    public object Clone()
    {
        return new AgentWalk( this );
    }

    public void Initialize( Agent agent )
    {
        this.Agent = agent;
    }

    public bool IsComplete()
    {
        return _isComplete;
    }

    public void OnActivate()
    {
        _pathFollower = Agent.PathToLocation( Destination );
    }


    public void OnDeactivate()
    {
        _pathFollower = null;
        _isComplete = false;
    }


    public void Update()
    {
        if( _pathFollower == null || _pathFollower.IsComplete )
            _isComplete = true;
    }
}



[CreateAssetMenu(fileName = "Agent Walk", menuName = "Boxy/Behaviors/Agent Walk Behavior" )]
public class AgentWalkDefinition : AgentBehaviorDefinition<AgentWalk>
{

}
