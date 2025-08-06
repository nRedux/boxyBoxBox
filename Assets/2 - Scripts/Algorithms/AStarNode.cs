using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace MDC.Pathfinding
{
    public enum AStarList
    {
        Unassigned,
        Open,
        Closed
    }

    public class AStarNode
    {
        /// <summary>
        /// Pathing score, heuristic, cost
        /// </summary>
        public float F = 0, H = 0, G = 0;

        /// <summary>
        /// Location in grid
        /// </summary>
        public readonly Vector2Int Position = Vector2Int.zero;

        /// <summary>
        /// Traversable?
        /// </summary>
        public bool Walkable = false;

        /// <summary>
        /// Which list is the node in
        /// </summary>
        public AStarList List = AStarList.Unassigned;


        /// <summary>
        /// Parent in current pathing execution.
        /// </summary>
        public AStarNode Parent = null;


        public readonly AStarGraph Graph;
        private Vector2 CellCenterOffset =>  Graph.CellSize * .5f;
        public Vector2 WorldPosition =>  Graph.WorldMin + CellCenterOffset + new Vector2( Position.x * Graph.CellSize.x, Position.y * Graph.CellSize.y );
        public Vector2 MinPosition => WorldPosition + new Vector2( -Graph.CellExtents.x, -Graph.CellExtents.y );
        public Vector2 MaxPosition => WorldPosition + new Vector2( Graph.CellExtents.x, Graph.CellExtents.y );

        public int CompareTo( AStarNode other )
        {
            return F.CompareTo( other.F );
        }

        public AStarNode( AStarGraph graph, Vector2Int position, bool walkable )
        {
            this.Walkable = walkable;
            this.Position = position;
            this.Graph = graph;
        }


        /// <summary>
        /// Prepare for next pathfinding request
        /// </summary>
        public void Reset()
        {
            Parent = null;
            G = H = 0f;
            F = int.MaxValue;
            List = AStarList.Unassigned;
        }


        public static bool operator ==( AStarNode lhs, AStarNode rhs )
        {
            if( ReferenceEquals( lhs, rhs ) ) return true;
            if( lhs is null || rhs is null ) return false;
            return lhs.Position == rhs.Position;
        }


        public static bool operator !=( AStarNode lhs, AStarNode rhs )
        {
            return !( lhs == rhs );
        }


        public override bool Equals( object obj )
        {
            if( obj is AStarNode other )
            {
                return this.Position == other.Position;
            }
            return false;
        }


        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }


        public bool PointWithinBounds( Vector2 point )
        {
            return point.x >= MinPosition.x && point.y >= MinPosition.y && point.x <= MaxPosition.x && point.x <= MaxPosition.y;
        }

        internal bool Overlap( Bounds bounds )
        {
            var dimension = MaxPosition - MinPosition;
            Bounds thisBounds = new Bounds(MinPosition + Graph.CellSize * .5f, Graph.CellSize);
            var size = thisBounds.size;
            size.z = 1f;
            thisBounds.size = size;
            var center = thisBounds.center;
            center.z = 1f;
            thisBounds.center = center;
            return thisBounds.Intersects( bounds );
        }
    }

}