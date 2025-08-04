using System;
using UnityEngine;


public interface IAgentBehaviorDefinition { }


public interface IAgentBehavior
{
    public Agent Agent { get; }

    public bool CanActivate();
    public void Initialize( Agent agent );
    public void OnActivate();
    public void OnDeactivate();
    public void Update();

    public bool IsComplete();
}



public abstract class AgentBehaviorDefinition<TBehaviour> : ScriptableObject, IAgentBehaviorDefinition where TBehaviour: IAgentBehavior, ICloneable
{
    [SerializeField]
    private TBehaviour _behavior;

    public ICloneable CloneData()
    {
        return _behavior.Clone() as ICloneable;
    }
}
