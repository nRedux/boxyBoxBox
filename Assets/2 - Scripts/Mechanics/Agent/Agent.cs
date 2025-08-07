using System.Collections;
using UnityEngine;
using MDC.Pathfinding;
using TMPro;
using System.Linq;

public enum EmoteType
{
    Question,
    Exclaim,
    Idea
}



[System.Serializable]
public class Emote
{
    public EmoteType Type;
    public GameObject GameObject;

    public void Show()
    {
        if( GameObject == null )
            return;
        GameObject.SetActive( true );
    }

    public void Hide()
    {
        if( GameObject == null )
            return;
        GameObject.SetActive( false );
    }
}



[System.Serializable]
public class Emotes
{
    [SerializeField]
    private Emote[] _emotes;

    public void ShowEmote( EmoteType type )
    {
        HideEmotes();
        var emote = _emotes.FirstOrDefault( x => x.Type == type );
        if( emote == null ) return;
        emote.Show();
    }

    public void HideEmotes()
    {
        _emotes.Do( x =>
        {
            x.Hide();
        } );
    }
}



public class Agent : MonoBehaviour
{
    private const float FOOTSTEP_FREQUENCY = .35f;
    private const float SLIDE_OFF_RATE = 1.3f;
    private const string ANIM_FACING_X = "facing_x";
    private const string ANIM_FACING_Y = "facing_y";
    private const string ANIM_VELOCITY = "velocity";

    [Tooltip( "The AI behavior conductor" )]
    [SerializeReference]
    public AgentConductorBase Conductor;

    [Tooltip( "The audio which plays for footstep sounds." )]
    [SerializeField]
    private AudioSource _footstepSource = null;

    [Tooltip( "The rate of moment of the agent" )]
    [SerializeField]
    public float _speed = 1f;

    [Tooltip( "How close to the current path node before we look to move to the next." )]
    [SerializeField]
    private float _pathGoalDist = .1f;

    public Emotes Emotes;

    private Vector3 _velocity = Vector3.zero, _animatorFacing = Vector3.zero;
    private Animator _animator = null;
    private AStarPath _path = null;
    private Vector3[] _pathPoints = null;
    private int _pathIndex = 0;
    private float _pathGoalDistSqr = 0f;
    private PathFollower _pathFollower = null;
    private IAgentConductor _conductor = null;
    private Rigidbody2D _rigidBody = null;
    private Coroutine _footstepsRoutine = null;
    private WaitForSeconds _footstepDelay = new WaitForSeconds( FOOTSTEP_FREQUENCY );
    private int _animFacingXHash = -1;
    private int _animFacingYHash = -1;
    private int _animFacingVelHash = -1;


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


    public void SetFacingDirection( Vector3 direction )
    {
        _animatorFacing = direction;
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


    public void StopPathing()
    {
        _velocity = Vector3.zero;
        if( _pathFollower == null ) return;
        _pathFollower.Following = false;
    }

    public void ResumePathing()
    {
        if( _pathFollower == null ) return;
        _pathFollower.Following = true;
    }


    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        var world = FindFirstObjectByType<World>();
        
        Initialize( world );
    }
    

    private void OnCollisionStay2D( Collision2D collision )
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if( rb == null )
            return;

        Vector3 deltaPos = transform.position - collision.transform.position;
        deltaPos = deltaPos - Vector3.Project( deltaPos, _rigidBody.linearVelocity.normalized );
        if( deltaPos.magnitude < .001f )
            deltaPos = new Vector3( deltaPos.y, deltaPos.z, 0f );

        deltaPos.z = 0f;
        rb.AddForce( -deltaPos.normalized * SLIDE_OFF_RATE / Time.fixedDeltaTime );
    }


    public void Initialize( World world )
    {
        Emotes.HideEmotes();
        _animator = GetComponent<Animator>();
        this.World = world;
        if( this.World == null )
            enabled = false;   
        _pathGoalDistSqr = _pathGoalDist * _pathGoalDist;
        _conductor = Conductor.CloneData() as IAgentConductor;
        _conductor.Initialize( this );
        CacheAnimHashes();
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



    private void CacheAnimHashes()
    {
        _animFacingXHash = Animator.StringToHash( ANIM_FACING_X );
        _animFacingYHash = Animator.StringToHash( ANIM_FACING_Y );
        _animFacingVelHash = Animator.StringToHash( ANIM_VELOCITY );
    }

    private void UpdateAnimator()
    {
        if( _animator == null )
            return;
        _animator.SetFloat( _animFacingXHash, _animatorFacing.x );
        _animator.SetFloat( _animFacingYHash, _animatorFacing.y );
        _animator.SetFloat( _animFacingVelHash, (_velocity / Time.deltaTime).magnitude > .001f ? .8f : 0.1f );
    }


    public void Update()
    {
        if( !World.Playing )
            return;
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
