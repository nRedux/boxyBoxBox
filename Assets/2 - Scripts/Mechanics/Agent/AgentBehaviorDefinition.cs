using System;
using UnityEngine;


public interface IAgentBehaviorDefinition { }


public interface IAgentBehavior
{
    public Agent Agent { get; }

    /// <summary>
    /// Implementations should return true whether this state, in it's current configuration, can activate
    /// </summary>
    /// <returns>True if it can activate, false otherwise</returns>
    public bool CanActivate();
    /// <summary>
    /// Initialize the state preparing it for use
    /// </summary>
    /// <param name="agent">The agent the state will be operating on</param>
    public void Initialize( Agent agent );
    /// <summary>
    /// Implement logic for state activation
    /// </summary>
    public void OnActivate();
    /// <summary>
    /// Implement logic for when the state is deactivated
    /// </summary>
    public void OnDeactivate();
    /// <summary>
    /// logic to execute for state behavior each frame
    /// </summary>
    public void Update();

    /// <summary>
    /// Is the state finished?
    /// </summary>
    /// <returns>True if the state is complete, false otherwise</returns>
    public bool IsComplete();
}



/// <summary>
/// Base class for simple creation of agent behavior definitions
/// </summary>
/// <typeparam name="TBehaviour"></typeparam>
public abstract class AgentBehaviorDefinition<TBehaviour> : ScriptableObject, IAgentBehaviorDefinition where TBehaviour: IAgentBehavior, ICloneable
{
    [SerializeField]
    private TBehaviour _behavior;

    public ICloneable CloneData()
    {
        return _behavior.Clone() as ICloneable;
    }
}
