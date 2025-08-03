using System;
using UnityEngine;


public interface IAgentBehavior
{
    public MonoAgent Agent { get; }

    public bool CanActivate();
    public void Initialize( MonoAgent agent );
    public void OnActivate();
    public void OnDeactivate();
    public void Update();
}


[System.Serializable]
public class AgentIdle : IAgentBehavior, ICloneable
{
    public MonoAgent Agent => throw new NotImplementedException();

    public bool CanActivate()
    {
        return true;
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public void Initialize( MonoAgent agent )
    {
        throw new System.NotImplementedException();
    }


    public void OnActivate()
    {
        throw new System.NotImplementedException();
    }


    public void OnDeactivate()
    {
        throw new System.NotImplementedException();
    }


    public void Update()
    {
        throw new System.NotImplementedException();
    }
}


[System.Serializable]
public class AgentWalk : IAgentBehavior, ICloneable
{

    [SerializeField]
    private float _speed;

    /*
     * I would consider dynamically linking back to the source asset which this prototype was instantiated from in a full implementation.
     * This is a simpler example of a way to have designer constructed behaviors which come from scriptable object sources.
     */

    public AgentWalk( AgentWalk other )
    {
        this.Agent = other.Agent;
        this._speed = other._speed;
    }

    //Normally serialized via JSON attribution (JSON.net) in my experience.
    public MonoAgent Agent { get; private set; }

    public bool CanActivate()
    {
        return true;
    }

    public object Clone()
    {
        return new AgentWalk( this );
    }

    public void Initialize( MonoAgent agent )
    {
        
    }


    public void OnActivate()
    {
        throw new System.NotImplementedException();
    }


    public void OnDeactivate()
    {
        throw new System.NotImplementedException();
    }


    public void Update()
    {
        throw new System.NotImplementedException();
    }


}


public class AgentBehaviorDefinition<TBehaviour> : ScriptableObject where TBehaviour: IAgentBehavior, ICloneable
{
    [SerializeField]
    private TBehaviour _behavior;

    public ICloneable CloneData()
    {
        return _behavior.Clone() as ICloneable;
    }
}


[CreateAssetMenu(fileName = "AgentBehavior", menuName = "Scriptable Objects/AgentBehavior")]
public class AgentWalkDefinition : AgentBehaviorDefinition<AgentWalk>
{

}



[CreateAssetMenu( fileName = "AgentBehavior", menuName = "Scriptable Objects/AgentBehavior" )]
public class AgentIdleDefinition : AgentBehaviorDefinition<AgentWalk>
{

}
