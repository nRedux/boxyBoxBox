using UnityEngine;


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


        public int CompareTo( AStarNode other )
        {
            return F.CompareTo( other.F );
        }

        public AStarNode( Vector2Int position, bool walkable )
        {
            this.Walkable = walkable;
            this.Position = position;
        }


        /// <summary>
        /// Prepare for next pathfinding request
        /// </summary>
        public void Reset()
        {
            Parent = null;
            F = G = H = 0f;
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
    }

}