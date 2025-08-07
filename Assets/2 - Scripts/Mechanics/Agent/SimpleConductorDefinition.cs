using System;
using System.Linq;
using UnityEngine;



[System.Serializable]
public class SimpleConductor : IAgentConductor, ICloneable
{

    [NonSerialized]
    public AgentIdle Idle;
    [NonSerialized]
    public AgentWalk Walk;
    [NonSerialized]
    public AgentSortBox Sort;
    [NonSerialized]
    public AgentSurprise Surprise;

    private BehaviorFSM _fsm = null;
    private Agent _agent = null;

    private IEntity _target;

    private bool _surpriseComplete = false;

    public SimpleConductor( AgentIdle idle, AgentWalk walk, AgentSortBox sort )
    {
        this.Idle = idle;
        this.Walk = walk;
        this.Sort = sort;
    }


    public void Initialize( Agent agent )
    {
        Idle.Initialize( agent );
        Walk.Initialize( agent );
        Sort.Initialize( agent );
        Surprise.Initialize( agent );

        _fsm = new BehaviorFSM();
        _fsm.ActivateBehavior( Surprise );
        _agent = agent;
    }


    public void Conduct()
    {
        _fsm.Update();



        if( _fsm.ActiveBehavior == Surprise && !_surpriseComplete && _target == null )
        {
            _target = _agent.World.TakeBox(_agent.transform.position);

            if( _target != null )
            {
                _surpriseComplete = true;
                Surprise.Target = _target;
            }
        }
        else if( _fsm.ActiveBehavior == Surprise && Surprise.IsComplete() )
        {
            _fsm.ActivateBehavior( Idle );
        }

        if( _fsm.ActiveBehavior == Idle )
        {
            _target = _agent.World.TakeBox( _agent.transform.position );

            if( _target != null )
            {
                var concreteTarget = _target as Entity;
                Walk.Destination = concreteTarget.transform.position;
                _fsm.ActivateBehavior( Walk );
            }
        }
        else if( _fsm.ActiveBehavior == Walk )
        {
            if( _fsm.ActiveBehavior.IsComplete() )
            {
                if( !TryBeginGrab() )
                    _fsm.ActivateBehavior( Idle );
            }
        }
        else if( _fsm.ActiveBehavior == Sort )
        {
            if( Sort.IsComplete() ) 
            {
                _target = null;
                _fsm.ActivateBehavior( Idle );
            }
        }

    }


    private bool TryBeginGrab()
    {
        if( _target == null )
            return false;
        Sort.Target = _target;
        if( Sort.CanActivate() )
        {
            Sort.Target = _target;
            _fsm.ActivateBehavior( Sort );
            return true;
        }

        return false;
    }


    public object Clone()
    {
        return new SimpleConductor( this.Idle, this.Walk, this.Sort );
    }
}



public class BehaviorFSM
{

    private IAgentBehavior _activeBehavior;


    public IAgentBehavior ActiveBehavior => _activeBehavior;

    public void Update()
    {
        if( _activeBehavior != null )
        {
            _activeBehavior.Update();
        }
    }

    public void ActivateBehavior( IAgentBehavior behavior )
    {
        if( _activeBehavior != null )
            _activeBehavior.OnDeactivate();

        _activeBehavior = behavior;
        if( _activeBehavior == null )
            return;
        _activeBehavior.OnActivate();
    }
}



[CreateAssetMenu( menuName = "Boxy/Behaviors/SimpleConductor" )]
public class SimpleConductorDefinition: AgentConductorDefinition<SimpleConductor>
{
    public AgentWalkDefinition WalkBehavior;
    public AgentIdleDefinition IdleBehavior;
    public AgentSortBoxDefinition SortBehavior;
    public AgentSurpriseBehaviorDefinition SurpriseBehavior;

    public override ICloneable CloneData()
    {
        var clone = base.CloneData();
        var concreteClone = clone as SimpleConductor;
        concreteClone.Idle = IdleBehavior.CloneData() as AgentIdle;
        concreteClone.Walk = WalkBehavior.CloneData() as AgentWalk;
        concreteClone.Sort = SortBehavior.CloneData() as AgentSortBox;
        concreteClone.Surprise = SurpriseBehavior.CloneData() as AgentSurprise;
        return clone;
    }

}
