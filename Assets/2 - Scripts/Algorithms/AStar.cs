using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Yohash.PriorityQueue;


namespace MDC.Pathfinding
{

    public class AStarPath
    {
        public List<Vector2Int> Nodes { get; private set; }

        public AStarPath( List<Vector2Int> nodes )
        {
            if( nodes == null )
                throw new System.ArgumentNullException( $"Argument {nameof( nodes )} cannot be null" );

            Nodes = nodes;
        }

        public Vector3[] GetWorldPoints( Vector3 worldOrigin )
        {
            return Nodes.Select( node => new Vector3( node.x + worldOrigin.x, node.y + worldOrigin.y ) ).ToArray();
        }
    }


    public static class AStar
    {
        private const float CARDINAL_MOVE_COST = 1f;
        private const float DIAGONAL_MOVE_COST = 1.4142f;


        /// <summary>
        /// Adjacent node offset lookup
        /// </summary>
        private static readonly Vector2Int[] _adjacentDirection = {
            new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
            new Vector2Int( 0, 1),                     new Vector2Int( 0, -1),
            new Vector2Int( 1, -1), new Vector2Int( 1, 0), new Vector2Int( 1, 1),
        };


        /// <summary>
        /// Calculate a path from a start location to a destination location
        /// </summary>
        /// <param name="graph">The A* graph</param>
        /// <param name="start">The start of the path test</param>
        /// <param name="goal">The goal destination</param>
        /// <param name="result">The resulting path information</param>
        /// <returns>True when a successful path is calculated, false in the case of failing to find a path to the goal.</returns>
        public static AStarPath FindPath( AStarGraph graph, Vector2Int start, Vector2Int goal )
        {
            if( start == goal )
                return null;


            graph.Reset();
            List<Vector2Int> resultNodes = new List<Vector2Int>();
            SimplePriorityQueue<AStarNode> open = new SimplePriorityQueue<AStarNode>();
            HashSet<AStarNode> closed = new HashSet<AStarNode>();
            AStarNode current = null;

            var first = graph.GetNodeUnsafe( start );
            open.Enqueue( first, 0 );

            while( open.Count > 0 )
            {
                current = open.Dequeue();
                closed.Add( current );
                current.List = AStarList.Closed;

                for( int i = 0; i < _adjacentDirection.Length; i++ )
                {
                    var adjacentCoord = current.Position + _adjacentDirection[i];
                    if( adjacentCoord.x < 0 || adjacentCoord.y < 0 ||
                        adjacentCoord.x >= graph.Width || adjacentCoord.y >= graph.Height )
                        continue;
                    
                    var adjNode = graph.GetNodeUnsafe( adjacentCoord );

                    if( adjNode.Position == goal )
                    {
                        adjNode.Parent = current;
                        return BacktracePath( adjNode );
                    }

                    EvaluateAdjacentNode( adjNode, current, goal, open );
                }

            }

            return null;
        }


        /// <summary>
        /// Evaluate adjacent nodes to determine handling
        /// </summary>
        /// <param name="adjNode"></param>
        /// <param name="current"></param>
        /// <param name="goal"></param>
        /// <param name="open"></param>
        private static void EvaluateAdjacentNode( AStarNode adjNode, AStarNode current, Vector2Int goal, SimplePriorityQueue<AStarNode> open )
        {
            if( !adjNode.Walkable ) return;
            if( adjNode.List == AStarList.Closed /*&& adjNode.F <= current.F*/ ) return;
            if( adjNode.List == AStarList.Open && adjNode.F <= current.F ) return;

            adjNode.Parent = current;
            RelcalculateNodeCosts( adjNode, goal );

            open.Enqueue( adjNode, adjNode.F );
            adjNode.List = AStarList.Open;
        }


        /// <summary>
        /// Backtrace the path, producing the final AStarPath
        /// </summary>
        /// <param name="node">The node we're backtracing from - goal node</param>
        /// <returns>AStarPath object containing the calculated path</returns>
        private static AStarPath BacktracePath( AStarNode node )
        {
            List<Vector2Int> points = new List<Vector2Int>();
            var current = node;
            while( current.Parent != null )
            {
                points.Add( current.Position );
                current = current.Parent;
            }
            points.Reverse();
            return new AStarPath( points );
        }


        private static int GetManhattan( Vector2Int delta )
        {
            return Mathf.Abs( delta.x ) + Mathf.Abs( delta.y );
        }


        /// <summary>
        /// Heuristic calculation
        /// </summary>
        /// <param name="from">Start</param>
        /// <param name="to">Destination</param>
        /// <returns>Estimate cost to goal taking account of cardinal vs diagonal movement</returns>
        private static float GetOctile( Vector2Int from, Vector2Int to )
        {
            int dx = Mathf.Abs( to.x - from.x );
            int dy = Mathf.Abs( to.y - from.y );
            return CARDINAL_MOVE_COST * ( dx + dy )
                 + ( DIAGONAL_MOVE_COST - 2 * CARDINAL_MOVE_COST ) * Mathf.Min( dx, dy );
        }


        /// <summary>
        /// Recalculate costs relative to parent and goal - expects updated parent
        /// </summary>
        /// <param name="node"></param>
        private static void RelcalculateNodeCosts( AStarNode node, Vector2Int goal )
        {
            var posDelta = node.Position - node.Parent.Position;
            //Identify cardinal or diagonal
            float moveCost = GetManhattan( posDelta ) == 1 ? CARDINAL_MOVE_COST : DIAGONAL_MOVE_COST;
            node.G = node.Parent.G + moveCost;
            node.H = GetOctile( node.Position, goal );
            node.F = node.G + node.H;
        }

    }

}