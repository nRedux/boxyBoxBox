using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Yohash.PriorityQueue;


namespace MDC.Pathfinding
{

    public class AStarPath
    {
        public List<Vector2> Nodes { get; private set; }

        public AStarPath( List<Vector2> nodes )
        {
            if( nodes == null )
                throw new System.ArgumentNullException( $"Argument {nameof( nodes )} cannot be null" );

            Nodes = nodes;
        }

        public Vector3[] GetWorldPoints()
        {
            return Nodes.Select( node => new Vector3( node.x, node.y ) ).ToArray();
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
            SimplePriorityQueue<AStarNode> open = new SimplePriorityQueue<AStarNode>();
            AStarNode current = null;

            var goalNode = graph.GetNodeUnsafe( goal );
            var firstNode = graph.GetNodeUnsafe( start );
            
            if( !firstNode.Walkable )
                return null;
            if( !goalNode.Walkable )
                return null;

            RecalculateNodeValues( firstNode, goal, 0 );
            open.Enqueue( firstNode, 0 );
            firstNode.List = AStarList.Open;

            while( open.Count > 0 )
            {
                current = open.Dequeue();
                current.List = AStarList.Closed;

                for( int i = 0; i < _adjacentDirection.Length; i++ )
                {
                    var adjacentCoord = current.Position + _adjacentDirection[i];
                    if( adjacentCoord.x < 0 || adjacentCoord.y < 0 ||
                        adjacentCoord.x >= graph.Width || adjacentCoord.y >= graph.Height )
                        continue;
                    
                    var adjNode = graph.GetNodeUnsafe( adjacentCoord );
                    if( !adjNode.Walkable )
                        continue;

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


        private static bool IsDiagonalValid( AStarGraph graph, Vector2Int from, Vector2Int to )
        {
            Vector2Int delta = to - from;

            var a = graph.GetNodeUnsafe( from + new Vector2Int( delta.x, 0 ) );
            if( !a.Walkable )
                return false;
            var b = graph.GetNodeUnsafe( from + new Vector2Int( 0, delta.y ) );
            if( !b.Walkable )
                return false;

            return true;
        }


        private static bool IsDiagonal( Vector2Int from, Vector2Int to )
        {
            return GetManhattan( from, to ) == 2;
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
            if( adjNode.List == AStarList.Closed)
                return;

            float moveCost = GetMoveCost( current.Position, adjNode.Position );
            Vector2Int delta = current.Position - adjNode.Position;
            //Diagonal move must not clip
            if( IsDiagonal( current.Position, adjNode.Position ) && !IsDiagonalValid( current.Graph, current.Position, adjNode.Position ) )
                return;


            if( adjNode.List == AStarList.Open  )
            {
                var posDelta = adjNode.Position - current.Position;
                //Identify cardinal or diagonal
                float newG = current.G + moveCost;
                float newH = GetOctile( adjNode.Position, goal );
                float newF = newG + newH;
                if( newF < adjNode.F )
                {
                    adjNode.Parent = current;
                    adjNode.G = newG;
                    adjNode.F = newF;
                    open.UpdatePriority( adjNode, adjNode.F );
                }
            }
            else
            {
                adjNode.Parent = current;
                RelativeRecalculateNode( adjNode, goal );
                open.Enqueue( adjNode, adjNode.F );
                adjNode.List = AStarList.Open;
            }

        }


        /// <summary>
        /// Recalculate costs relative to parent and goal - expects updated parent
        /// </summary>
        /// <param name="node"></param>
        private static void RelativeRecalculateNode( AStarNode node, Vector2Int goal )
        {
            float ancestorG = node.Parent != null ? node.Parent.G : 0f; 
            //Identify cardinal or diagonal
            float moveCost = GetMoveCost(node.Parent.Position, node.Position);
            RecalculateNodeValues( node, goal, ancestorG + moveCost );
        }


        private static void RecalculateNodeValues( AStarNode node, Vector2Int goal, float moveCost )
        {
            node.G = moveCost;
            node.H = GetOctile( node.Position, goal );
            node.F = node.G + node.H;
        }


        private static float GetMoveCost( Vector2Int from, Vector2Int to )
        {
            return GetManhattan( from, to ) == 1 ? CARDINAL_MOVE_COST : DIAGONAL_MOVE_COST;
        }


        /// <summary>
        /// Backtrace the path, producing the final AStarPath
        /// </summary>
        /// <param name="node">The node we're backtracing from - goal node</param>
        /// <returns>AStarPath object containing the calculated path</returns>
        private static AStarPath BacktracePath( AStarNode node )
        {
            List<Vector2> points = new List<Vector2>();
            var current = node;
            while( current.Parent != null )
            {
                if( !current.Walkable )
                    Debug.LogError( $"Backtracing through wall at {current.Position}" );
                points.Add( current.WorldPosition );
                current = current.Parent;
            }
            points.Reverse();
            return new AStarPath( points );
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


        private static int GetManhattan( Vector2Int from, Vector2Int to )
        {
            int dx = Mathf.Abs( to.x - from.x );
            int dy = Mathf.Abs( to.y - from.y );
            return dx + dy;
        }


    }

}