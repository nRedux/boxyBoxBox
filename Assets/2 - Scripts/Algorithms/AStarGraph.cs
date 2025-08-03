using UnityEngine;


namespace MDC.Pathfinding
{
    public class AStarGraph
    {
        public readonly int Width = 0, Height = 0;
        private AStarNode[,] _graph = null;

        public AStarNode[,] Graph => _graph;


        public AStarGraph( int width, int height )
        {
            Width = width;
            Height = height;
            _graph = new AStarNode[width, height];
            Build();
        }


        private void Build()
        {
            for( int x = 0; x < Width; x++ )
            {
                for( int y = 0; y < Height; y++ )
                {
                    Graph[x, y] = new AStarNode( new Vector2Int( x, y ), true );
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
    }

}