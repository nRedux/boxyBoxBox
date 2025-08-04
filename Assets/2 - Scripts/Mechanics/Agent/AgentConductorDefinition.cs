using System;
using UnityEngine;


public abstract class AgentConductorBase : ScriptableObject
{
    public abstract ICloneable CloneData();
}

public abstract class AgentConductorDefinition<TConductor> : AgentConductorBase where TConductor : IAgentConductor, ICloneable
{
    [SerializeField]
    private TConductor _conductor;

    public override ICloneable CloneData()
    {
        return _conductor.Clone() as ICloneable;
    }
}
