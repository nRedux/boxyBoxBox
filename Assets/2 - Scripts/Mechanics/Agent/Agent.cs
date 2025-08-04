using System.Collections;
using UnityEngine;
using MDC.Pathfinding;
using TMPro;


public class Agent : MonoBehaviour
{
    private const float FOOTSTEP_FREQUENCY = .35f;
    private const float SLIDE_OFF_RATE = .8f;
    [SerializeReference]
    public AgentConductorBase Conductor;

    [SerializeField]
    private AudioSource _footstepSource = null;

    [SerializeField]
    public float _speed = 1f;
    [SerializeField]
    private float _pathGoalDist = .1f;


    private Vector3 _velocity, _animatorFacing;
    private Animator _animator = null;
    private AStarPath _path = null;
    private Vector3[] _pathPoints = null;
    private int _pathIndex = 0;
    private float _pathGoalDistSqr = 0f;
    private PathFollower _pathFollower;
    private IAgentConductor _conductor;
    private Rigidbody2D _rigidBody;
    private Coroutine _footstepsRoutine = null;
    private WaitForSeconds _footstepDelay = new WaitForSeconds( FOOTSTEP_FREQUENCY );

    public World World { get; private set; }
    public Vector3 Velocity => _velocity;

    public void Move( Vector3 direction )
    {
        if( direction == Vector3.zero )
        {
            _velocity = Vector3.zero;
        }
        else
        {
            _animatorFacing = _velocity = direction.normalized * _speed;
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
        _rigidBody = GetComponent<Rigidbody2D>();
        var world = FindFirstObjectByType<World>();
        Initialize( world );
    }
    

    private void OnCollisionStay2D( Collision2D collision )
    {
        Rigidbody2D e = collision.collider.GetComponent<Rigidbody2D>();
        Vector3 deltaPos = transform.position - collision.transform.position;
        deltaPos = deltaPos - Vector3.Project( deltaPos, _rigidBody.linearVelocity.normalized );
        if( deltaPos.magnitude < .001f )
            deltaPos = new Vector3( deltaPos.y, deltaPos.z, 0f );

        deltaPos.z = 0f;
        e.AddForce( -deltaPos.normalized * SLIDE_OFF_RATE / Time.fixedDeltaTime );
    }


    public void Initialize( World world )
    {
        _animator = GetComponent<Animator>();
        this.World = world;
        _pathGoalDistSqr = _pathGoalDist * _pathGoalDist;
        if( this.World == null )
            enabled = false;   
        _conductor = Conductor.CloneData() as IAgentConductor;
        _conductor.Initialize( this );
    }


    private void StartFootsteps()
    {
        if( _footstepSource == null )
            return;
        if( _footstepsRoutine != null )
            return;
        _footstepsRoutine = StartCoroutine( FootstepsRoutine() );
    }

    private void StopFootsteps()
    {
        if( _footstepsRoutine != null )
            StopCoroutine( _footstepsRoutine );
        _footstepsRoutine = null;
    }

    IEnumerator FootstepsRoutine()
    {
        if( _footstepSource == null )
            yield break;
        while( true )
        {
            _footstepSource.Play();
            yield return _footstepDelay;
        }
    }


    private void FixedUpdate()
    {
        var r = GetComponent<Rigidbody2D>();
        r.linearVelocity = _velocity;
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
            StartFootsteps();
            _pathFollower.FollowPath();
            if( _pathFollower.IsComplete ) 
            {
                StopFootsteps();
                _pathFollower = null;
            }
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
