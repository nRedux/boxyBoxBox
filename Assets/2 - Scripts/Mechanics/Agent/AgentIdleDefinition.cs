using System;
using UnityEngine;


[System.Serializable]
public class AgentIdle : IAgentBehavior, ICloneable
{
    public Agent Agent { get; private set; }

    public bool CanActivate()
    {
        return true;
    }

    public object Clone()
    {
        AgentIdle clone = new AgentIdle();
        clone.Agent = this.Agent;
        return clone;
    }

    public void Initialize( Agent agent )
    {
        this.Agent = agent;
    }

    public bool IsComplete()
    {
        return false;
    }

    public void OnActivate()
    {
        //Stop movement
    }


    public void OnDeactivate()
    {

    }


    public void Update()
    {

    }
}




[CreateAssetMenu( fileName = "Agent Idle", menuName = "Boxy/Behaviors/Agent Idle Behavior" )]
public class AgentIdleDefinition : AgentBehaviorDefinition<AgentIdle>
{

}
