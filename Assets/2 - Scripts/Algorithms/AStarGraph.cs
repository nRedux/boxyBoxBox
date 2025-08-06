using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace MDC.Pathfinding
{
    public class AStarGraph
    {
        public readonly int Width = 0, Height = 0;
        private AStarNode[,] _graph = null;
        private readonly Vector2 _minPosition, _maxPosition;


        public AStarNode[,] Graph => _graph;


        public Vector2 CellSize => new Vector2( WorldSize.x / Width, WorldSize.y / Height );

        public Vector2 CellExtents => CellSize * .5f;


        public Vector2 WorldMin => _minPosition;


        public Vector2 WorldMax => _maxPosition;


        public Vector2 WorldSize => _maxPosition - _minPosition;


        public AStarGraph( int width, int height, Vector2 worldMin, Vector2 worldMax )
        {
            Width = width;
            Height = height;
            _graph = new AStarNode[width, height];
            _minPosition = worldMin;
            _maxPosition = worldMax;
            Build();
            ApplyBlockers();
        }


        private void ApplyBlockers()
        {
            var blockers = GameObject.FindObjectsByType<AStarBlocker>( FindObjectsSortMode.None );
            blockers.Do( x => x.Apply( this ) );
        }

        private void Build()
        {
            for( int x = 0; x < Width; x++ )
            {
                for( int y = 0; y < Height; y++ )
                {
                    Graph[x, y] = new AStarNode( this, new Vector2Int( x, y ), true );
                }
            }
        }


        public AStarNode GetNodeUnsafe( Vector2Int coordinate )
        {
            return _graph[coordinate.x, coordinate.y];
        }


        public void Reset()
        {
            for( int x = 0; x < Width; x++ )
            {
                for( int y = 0; y < Height; y++ )
                {
                    _graph[x, y].Reset();
                }
            }
        }

        public void DrawGizmos()
        {
            for( int x = 0; x < Width; x++ )
            {
                for( int y = 0; y < Height; y++ )
                {
                    var node = _graph[x, y];

                    if( node.Walkable )
                        Gizmos.DrawWireCube( (Vector3) node.WorldPosition + Vector3.back * 5f, node.MaxPosition - node.MinPosition );
                    else
                    {
                        var oldColor = Gizmos.color;
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube( (Vector3) node.WorldPosition + Vector3.back * 5f, node.MaxPosition - node.MinPosition );
                        Gizmos.color = oldColor;
                    }
                }
            }
        }

        public List<AStarNode> FindOverlapping( Bounds bounds )
        {
            var center = bounds.center;
            center.z = 0;
            bounds.center = center;
            var size = bounds.size;
            size.z = 1;
            bounds.size = size;

            List<AStarNode> result = new List<AStarNode>();

            for( int x = 0; x < Width; x++ )
            {
                for( int y = 0; y < Height; y++ )
                {
                    var node = _graph[x, y];
                    if( node.Overlap( bounds ) )
                        result.Add( node );
                }
            }

            return result;
        }
    }

}