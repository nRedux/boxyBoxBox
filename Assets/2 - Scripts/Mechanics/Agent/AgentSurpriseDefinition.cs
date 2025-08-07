using System;
using System.Collections;
using System.Linq;
using UnityEngine;


/// <summary>
/// Sorting behavior for agents
/// </summary>
[System.Serializable]
public class AgentSurprise : IAgentBehavior, ICloneable
{

    public IEntity Target;


    private WaitForSeconds _longWait = new WaitForSeconds( 4f );
    private WaitForSeconds _waitMedium = new WaitForSeconds( 2.5f );
    private WaitForSeconds _waitShort = new WaitForSeconds( 1f );
    private WaitWhile _waitHalfPath = null;
    private PathFollower _pathFollower;
    private bool _isComplete = false;

    public Agent Agent { get; private set; }


    public bool CanActivate()
    {
        if( Agent == null )
            return false;
        if( Target == null )
            return false;
        return true;
    }


    public object Clone()
    {
        AgentSurprise clone = new AgentSurprise();
        clone.Agent = this.Agent;
        return clone;
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
        Agent.StartCoroutine( DoSequence() );
    }


    public void OnDeactivate()
    {
        _pathFollower = null;
        _isComplete = false;
    }


    public void Update()
    {
    }


    private IEnumerator DoSequence()
    {
        while( Target == null )
            yield return null;

        //Wait for target assignment
        var targetEntity = ( Target as Entity );
        while( targetEntity == null )
            yield return null;

        yield return _waitShort;
        Vector3 targetPos = targetEntity.transform.position;
        Vector3 direction = ( targetPos - Agent.transform.position ).normalized;
        Agent.SetFacingDirection( direction );
        Agent.Emotes.ShowEmote( EmoteType.Exclaim );
        yield return _waitMedium;
        Agent.Emotes.HideEmotes();
        yield return _waitMedium;

        _pathFollower = Agent.PathToLocation( targetEntity.transform.position );
        if( _pathFollower == null )
        {
            _isComplete = true;
            yield break;
        }

        //I want him to walk toward the first box, but only travel part of the way to it.
        _waitHalfPath ??= new WaitWhile( () => { return _pathFollower.CalculateTimeOnPath( Agent.transform.position ) <= .5f; } );
        yield return _waitHalfPath;
        Agent.StopPathing();

        //Show question mark for X
        
        yield return _waitMedium;

        Agent.Emotes.HideEmotes();
        yield return _waitShort;
        Agent.Emotes.ShowEmote( EmoteType.Question );
        yield return _waitShort;

        Agent.Emotes.HideEmotes();
        Agent.SetFacingDirection( Vector3.down );

        yield return _waitShort;

        Agent.Emotes.ShowEmote( EmoteType.Idea );
        yield return _waitMedium;

        Agent.Emotes.HideEmotes();
        _isComplete = true;

    }
}



[CreateAssetMenu( fileName = "Agent Surprise", menuName = "Boxy/Behaviors/Agent Surprise Behavior" )]
public class AgentSurpriseBehaviorDefinition : AgentBehaviorDefinition<AgentSurprise>
{

}
