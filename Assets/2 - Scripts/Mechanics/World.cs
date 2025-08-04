using UnityEngine;
using UnityEngine.Tilemaps;
using MDC.Pathfinding;
using System.Collections.Generic;
using System.Linq;


public class World : MonoBehaviour
{

    public static World Instance => _instance;
    [SerializeField]
    private Tilemap _tileMap;

    private static World _instance;
    private AStarGraph _graph = null;
    private bool _hasPath;
    private AStarPath _path;
    private Camera _camera;
    private List<Entity> _boxes = new List<Entity>();


    public int BoxCount => _boxes.Count;

    
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


    public Entity TakeBox()
    {
        var box = _boxes.FirstOrDefault();
        if( box != null )
            _boxes.Remove( box );
        return box;
    }


    private void Initialize()
    {
        InitializeTilemap();
        InitializePathing();
    }


    private void Update()
    {
        if( Input.GetKeyDown(KeyCode.Space) )
        {
            Vector2Int s = new Vector2Int( Random.Range( 0, _graph.Width ), Random.Range( 0, _graph.Height ) );
            Vector2Int g = new Vector2Int( Random.Range( 0, _graph.Width ), Random.Range( 0, _graph.Height ) );

            Debug.Log( $"{s} to {g}" );
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

        _graph = new AStarGraph( Mathf.CeilToInt(WorldBounds.size.x), Mathf.CeilToInt(WorldBounds.size.y) );
    }


    private void OnDrawGizmos()
    {
        if( _hasPath )
        {
            //var nodes = _path.Nodes.Select( node => new Vector3( node.x, node.y, .1f ) ).ToArray();
            var points = _path.GetWorldPoints( WorldBounds.min );
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
    }


    public Vector2Int WorldToPathingSpace( Vector3 world )
    {
        var vec = new Vector2Int( Mathf.RoundToInt( world.x - WorldBounds.min.x ), Mathf.RoundToInt(world.y - WorldBounds.min.y ) );
        return vec;
    }


    public AStarPath CalculatePath( Vector2Int start, Vector2Int goal )
    {
        return AStar.FindPath( _graph, start, goal );
    }
}
