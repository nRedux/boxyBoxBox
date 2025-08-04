using UnityEngine;
using UnityEngine.Tilemaps;
using MDC.Pathfinding;
using System.Collections.Generic;
using System.Linq;


public class AStarPathAdapter
{

    private AStarPath _path;

    public AStarPathAdapter( AStarPath path )
    {
        this._path = path;
    }

}


public class World : MonoBehaviour
{


    public Tilemap TileMap;

    public Agent AgentPrefab;

    public Entity Box;


    private static World _instance;
    private AStarGraph _graph = null;
    private bool _hasPath;
    private AStarPath _path;
    private Camera _camera;

    public List<Entity> Boxes = new List<Entity>();

    public int BoxCount => Boxes.Count;

    public static World Instance => _instance;
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
        Boxes.Add( Box );
        Initialize();
    }


    public void AddBox(Entity box )
    {
        Boxes.Add( box );
    }


    public Entity TakeBox()
    {
        var box = Boxes.FirstOrDefault();
        if( box != null )
            Boxes.Remove( box );
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
        if( TileMap == null )
            TileMap = GetComponent<Tilemap>();

        TileMap.CompressBounds();
        WorldBounds = TileMap.localBounds;
    }


    private void InitializePathing()
    {
        if( TileMap == null )
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

        if( TileMap == null )
            return;
        TileMap.CompressBounds();
        var bounds = TileMap.localBounds;
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
