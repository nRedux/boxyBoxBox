using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using MDC.Pathfinding;

[System.Serializable]
public class AgentTransform
{
    [SerializeField]
    private Vector2 _position;


    [SerializeField]
    private Vector2 _velocity;


    public void Step()
    {
        _position += _velocity * Time.deltaTime;
    }


    public Vector2 Position { get => this._position; set => this._position =  value ; }


    public Vector2 Velocity { get => this._velocity; set => this._velocity = value; }
}


public interface IAgentLocomotion
{
    public bool IgnoreLower();
    public void Step( AgentTransform transform );
    public bool IsAlive();
}


public class AgentLocomotion : IAgentLocomotion
{
    public bool IgnoreLower()
    {
        return false;
    }

    public bool IsAlive()
    {
        return true;
    }

    public Vector2 Step( AgentTransform transform )
    {
        return Vector3.zero;
    }

    void IAgentLocomotion.Step( AgentTransform transform )
    {
        throw new System.NotImplementedException();
    }
}


public class AgentLocomotionSet
{
    private List<IAgentLocomotion> _locomotionSet = new List<IAgentLocomotion>();

    public void AddLocomotion( IAgentLocomotion locomotion )
    {
        if( locomotion == null )
            throw new System.ArgumentNullException( $"Argument {nameof( locomotion )} cannot be null." );
        _locomotionSet.Add( locomotion );
    }


    public void RemoveLocomotion( IAgentLocomotion locomotion )
    {
        if( locomotion == null )
            throw new System.ArgumentNullException( $"Argument {nameof( locomotion )} cannot be null." );
        _locomotionSet.Add( locomotion );
    }


    public void Step( AgentTransform transform )
    {
        for( int i = _locomotionSet.Count - 1; i >= 0; i-- )
        {
            var loc = _locomotionSet[i];
            loc.Step( transform );

            if( !loc.IsAlive() )
                _locomotionSet.RemoveAt( i );

            if( loc.IgnoreLower() )
                break;
        }
    }
}


[System.Serializable]
public class Agent
{

    //Delegate the actual locomotion updates for adaptable "physics" implementations. Static objects simply don't assign a locomotion handler here ( = null )
    public AgentLocomotionSet Locomotion { get; private set; } = new AgentLocomotionSet();

    //Separated agent properties. Serialization friendly - assuming source comes from some persistent data source.
    public AgentTransform Transform { get; private set; } = new AgentTransform();


    public Agent()
    {

    }

    public void Update()
    {
        StepLocomotion();
    }

    public void StepLocomotion()
    {
        Locomotion.Step( Transform );
    }
}



public class MonoAgent : MonoBehaviour
{
    //Delegate the actual locomotion updates for adaptable "physics" implementations. Static objects simply don't assign a locomotion handler here ( = null )
    public Agent Agent { get; private set; }

    //Ideally - Visual represention would be referenced here in something like the following
    //The following AgentVisuzlization would be responsible for location/loading the visualization implementation as well as 
    //private AgentVisualization _agentVisualization;

    public World World { get; private set; }

    public float Speed = 1f;
    public float PathGoalDist = .1f;


    private Vector3 _velocity, _animatorFacing;
    private Animator _animator = null;
    
    
    private AStarPath _path = null;
    private Vector3[] _pathPoints = null;
    private int _pathIndex = 0;

    private float _pathGoalDistSqr = 0f;

    public void Initialize( World world )
    {
        this.World = world;
        _pathGoalDistSqr = PathGoalDist * PathGoalDist;
    }


    private void Awake()
    {
        _animator = GetComponent<Animator>();

        this.World = FindFirstObjectByType<World>();
        if( this.World == null )
            enabled = false;  
        _pathGoalDistSqr = PathGoalDist * PathGoalDist;
    }


    private void UpdateAnimator()
    {
        if( _animator == null )
            return;
        _animator.SetFloat( "facing_x", _animatorFacing.x );
        _animator.SetFloat( "facing_y", _animatorFacing.y );
        _animator.SetFloat( "velocity", (_velocity / Time.deltaTime).magnitude > .001f ? .8f : 0.1f );
    }


    public void Update()
    {
        ClickTest();
        UpdateAnimator();

        if( _pathPoints != null )
        {
            Vector3 toPoint = _pathPoints[_pathIndex] - transform.position;
            _animatorFacing = _velocity = toPoint.normalized * Speed * Time.deltaTime;
            transform.position += _velocity;

            Vector3 postMoveToPoint = _pathPoints[_pathIndex] - transform.position;
            if( postMoveToPoint.sqrMagnitude <= _pathGoalDistSqr )
            {
                _pathIndex++;
            }

            if( _pathIndex >= _pathPoints.Length )
            {
                _pathPoints = null;
                _path = null;
                return;
            }
        }
        else
        {
            _velocity = Vector3.zero;
        }
    }


    private void ClickTest()
    {
        if( !Input.GetMouseButtonDown( 0 ) )
            return;
        Vector3 point = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        RaycastHit2D hit = Physics2D.Raycast( point, Vector2.zero );
        if( hit.collider != null )
        {
            var pathStart = World.WorldToPathingSpace( transform.position );
            var pathEnd = World.WorldToPathingSpace( hit.point );
            var path = World.CalculatePath( pathStart, pathEnd );

            Debug.Log( pathStart );
            Debug.Log( pathEnd );
            if( path != null )
            {
                World.SetPath( path );
                _path = path;
                _pathPoints = _path.GetWorldPoints( World.WorldBounds.min );
                _pathIndex = 0;
            }
            else
            {
                Debug.LogError( "No path found!" );
            }
        }
        else
        {
            Debug.LogError( "No mouse hit" );
        }
    }


    private void UpdateGraphics()
    {
        transform.position = Agent.Transform.Position;
    }
}
