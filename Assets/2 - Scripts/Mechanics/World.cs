using UnityEngine;
using UnityEngine.Tilemaps;
using MDC.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;


public class World : MonoBehaviour
{

    public static World Instance => _instance;

    [Tooltip( "The tilemap which represents the pathable world" )]
    [SerializeField]
    private Tilemap _tileMap;
    [Tooltip("Controls the density of the pathing graph")]
    [SerializeField]
    private int PathingDensity = 2;

    private static World _instance;
    private AStarGraph _graph = null;
    private bool _hasPath;
    private AStarPath _path;
    private Camera _camera;
    private List<Entity> _boxes = new List<Entity>();


    private Bounds _bounds = new Bounds();

    public int BoxCount => _boxes.Count;


    public bool Playing { get; set; } = false;


    public Bounds WorldBounds { get; private set; }


    public Camera Camera => _camera;


    public void SetPath( AStarPath path )
    {
        _hasPath = true;
        _path = path;
    }


    private void Awake()
    {
        _camera = Camera.main;
        _instance = this;
        //Boxes.Add( Box );
        Initialize();
    }


    public void AddBox(Entity box )
    {
        _boxes.Add( box );
    }


    public Entity TakeBox( Vector3 agentPos )
    {
        Entity closest = null;
        float smallestDist = float.MaxValue;

        foreach( var b in _boxes )
        {
            float dist = Vector3.SqrMagnitude( agentPos - b.transform.position );
            if( dist < smallestDist )
            {
                closest = b;
                smallestDist = dist;
            }
        }

        _boxes.Remove( closest );
        return closest;
    }


    private void Initialize()
    {
        InitializeBounds();
        InitializeTilemap();
        InitializePathing();
    }


    private void InitializeBounds()
    {
        if( _tileMap == null )
            return;
        _tileMap.CompressBounds();
        _bounds = _tileMap.localBounds;
    }


    private void InitializeTilemap()
    {
        if( _tileMap == null )
            _tileMap = GetComponent<Tilemap>();

        _tileMap.CompressBounds();
        WorldBounds = _tileMap.localBounds;
    }


    private void InitializePathing()
    {
        if( _tileMap == null )
            return;

        _graph = new AStarGraph( Mathf.CeilToInt( WorldBounds.size.x ) * PathingDensity, Mathf.CeilToInt( WorldBounds.size.y ) * PathingDensity, _bounds.min, _bounds.max );
    }


    private void Update()
    {
        /*
        if( Input.GetKeyDown(KeyCode.Space) )
        {
            Vector2Int s = WorldToPathingSpace(new Vector3( 6, -5, 0 ));
            Vector2Int g = WorldToPathingSpace( new Vector3( -7, -5, 0 ));

            AStarPath path = AStar.FindPath( _graph, s, g);

            if( path == null )
                Debug.Log( "No path found" );
            else
                Debug.Log( path.Nodes.Count );
            if( path != null )
            {
                _path = path;
                _hasPath = true;
            }
        }
        */
    }



    private void OnDrawGizmos()
    {
        if( _hasPath )
        {
            //var nodes = _path.Nodes.Select( node => new Vector3( node.x, node.y, .1f ) ).ToArray();
            var points = _path.GetWorldPoints();
            for( int i = 1; i < points.Length; i++ )
            {
                Gizmos.DrawLine( points[i-1], points[i] );
            }
        }

        if( _tileMap == null )
            return;
        _tileMap.CompressBounds();
        var bounds = _tileMap.localBounds;
        Gizmos.DrawWireCube( bounds.center, bounds.size );

        if( _graph != null)
            _graph.DrawGizmos();
    }


    public Vector2Int WorldToPathingSpace( Vector3 world )
    {
        var vec = new Vector2Int( Mathf.RoundToInt((world.x - _graph.WorldMin.x) / _graph.CellSize.x) , Mathf.RoundToInt( ( world.y - _graph.WorldMin.y ) / _graph.CellSize.y ) );
        return vec;
    }


    public AStarPath CalculatePath( Vector2Int start, Vector2Int goal )
    {
        return AStar.FindPath( _graph, start, goal );
    }


    public void BeginPlay()
    {
        Playing = true;
    }
}
