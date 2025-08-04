using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using MDC.Pathfinding;
using static UnityEngine.RuleTile.TilingRuleOutput;




public class Agent : MonoBehaviour
{
    //Ideally - Visual represention would be referenced here in something like the following
    //The following AgentVisuzlization would be responsible for location/loading the visualization implementation as well as 
    //private AgentVisualization _agentVisualization;

    [SerializeReference]
    public AgentConductorBase Conductor;

    public float Speed = 1f;
    public float PathGoalDist = .1f;


    private Vector3 _velocity, _animatorFacing;
    private Animator _animator = null;
    private AStarPath _path = null;
    private Vector3[] _pathPoints = null;
    private int _pathIndex = 0;
    private float _pathGoalDistSqr = 0f;
    private PathFollower _pathFollower;
    private IAgentConductor _conductor;

    public World World { get; private set; }



    public void Move( Vector3 direction )
    {
        if( direction == Vector3.zero )
        {
            _velocity = Vector3.zero;
        }
        else
        {
            _animatorFacing = _velocity = direction.normalized * Speed * Time.deltaTime;
            transform.position += _velocity;
        }
    }

    public PathFollower PathToLocation( Vector3 goal )
    {
        var pathStart = World.WorldToPathingSpace( transform.position );
        var pathEnd = World.WorldToPathingSpace( goal );
        var path = World.CalculatePath( pathStart, pathEnd );

        if( path != null )
        {
            World.SetPath( path );
            _pathFollower = new PathFollower( path, this, World );
            return _pathFollower;
        }

        return null;
    }


    private void Awake()
    {
        var world = FindFirstObjectByType<World>();
        Initialize( world );
    }

    public void Initialize( World world )
    {
        _animator = GetComponent<Animator>();
        this.World = world;
        _pathGoalDistSqr = PathGoalDist * PathGoalDist;
        if( this.World == null )
            enabled = false;

        _conductor = Conductor.CloneData() as IAgentConductor;
        _conductor.Initialize( this );
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
        UpdatePathFollowing();
        UpdateConductor();
    }


    private void UpdateConductor()
    {
        if( _conductor == null )
            return;
        _conductor.Conduct();
    }


    private void UpdatePathFollowing()
    {
        if( _pathFollower != null )
        {
            _pathFollower.FollowPath();
            if( _pathFollower.IsComplete )
                _pathFollower = null;
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
            PathToLocation( hit.point );
        }
        else
        {
            Debug.LogError( "No mouse hit" );
        }
    }
}
