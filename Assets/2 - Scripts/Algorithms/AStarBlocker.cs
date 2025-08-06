using MDC.Pathfinding;
using UnityEngine;

public class AStarBlocker : MonoBehaviour
{
    public Collider2D[] Colliders;

    public void Apply( AStarGraph graph )
    {
        Colliders.Do( x =>
        {
            var overlapping = graph.FindOverlapping( x.bounds );
            overlapping.Do( x => {
                    x.Walkable = false;
                } );
        } );
    }
}
