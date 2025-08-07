using MDC.Pathfinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;


public class PathFollower
{
    private const float PATHING_GOAL_DIST = .1f;

    private AStarPath _path = null;
    private Agent _agent = null;
    private Vector3[] _pathPoints = null;
    private int _pathIndex = 0;
    private float _pathGoalDistSqr = 0f;

    public bool Following { get; set; } = true; 

    public bool IsComplete { get; private set; } = false;

    private PathFollower() { }

    public PathFollower( AStarPath path, Agent agent, World world )
    {
        if( path == null )
            throw new System.ArgumentNullException( $"Argument {nameof( path )} cannot be null" );
        if( agent == null )
            throw new System.ArgumentNullException( $"Argument {nameof( agent )} cannot be null" );

        _agent = agent;
        _path = path;
        _pathGoalDistSqr = PATHING_GOAL_DIST * PATHING_GOAL_DIST; ;
        _path = path;
        _pathPoints = _path.GetWorldPoints();
        _pathIndex = 0;

        IsComplete = false;

    }

    public void FollowPath()
    {
        if( !Following )
            return;

        if( IsComplete )
            return;

        if( _pathPoints != null )
        {
            Vector3 toPoint = (_pathPoints[_pathIndex] + Vector3.up * .25f + Vector3.right * .25f) - _agent.transform.position;
            _agent.Move( toPoint );
            float timeNorm = CalculateTimeOnPath( _agent.transform.position );
            Debug.Log( timeNorm );
            Vector3 postMoveToPoint = ( _pathPoints[_pathIndex] + Vector3.up * .25f + Vector3.right * .25f ) - _agent.transform.position;
            if( postMoveToPoint.sqrMagnitude <= _pathGoalDistSqr )
            {
                _pathIndex++;
            }

            if( _pathIndex >= _pathPoints.Length )
            {
                _pathPoints = null;
                _path = null;
                _agent.Move( Vector3.zero );
                IsComplete = true;
                return;
            }
        }
        else
        {
            _agent.Move( Vector3.zero );
        }
    }


    /// <summary>
    /// Project a point onto a line segment
    /// </summary>
    /// <param name="segA">Start of line segment</param>
    /// <param name="segB">End of line segment</param>
    /// <param name="point">The point we're projecting onto the segment</param>
    /// <returns>The projected point on the line segment</returns>
    private Vector3 ProjectOnLineSeg( Vector3 segA, Vector3 segB, Vector3 point )
    {
        Vector3 dir = segB - segA;
        Vector3 w = point - segA;
        float proj = Vector3.Dot( w, dir );
        if( proj <= 0f )
            return segA;
        else 
        {
            float mag = ( segB - segA ).sqrMagnitude;
            return segA + ( proj / mag ) * dir;
        }        
    }


    /// <summary>
    /// Find the projected point and it's segment index on the path.
    /// </summary>
    /// <param name="position">The position in world space of the projection onto the path</param>
    /// <param name="index">The index of the line segment which was closest to the point. If there was a failure to project the point, will have the value -1.</param>
    /// <returns>The projected point</returns>
    public Vector3 ClosestPointAndIndex( Vector3 position, out int index )
    {
        index = -1;
        float smallestDistSqr = float.MaxValue;
        Vector3 point = Vector3.zero;

        Debug.Assert( _path != null, "Path is null" );
        Debug.Assert( _path.Nodes != null, "Path is null" );

        for( int i = 1; i < _path.Nodes.Count; i++ )
        {
            Vector3 proj = ProjectOnLineSeg( _path.Nodes[i], _path.Nodes[i - 1], position );
            Vector3 toPosition = position - proj;
            float thisDistSqr = toPosition.sqrMagnitude;
            if( thisDistSqr < smallestDistSqr )
            {
                point = proj;
                index = i - 1;
                smallestDistSqr = thisDistSqr;
            }
        }

        return point;
    }


    public float CalculateTimeOnPath( Vector3 point )
    {
        int index = -1;
        Vector3 projected = ClosestPointAndIndex( point, out index );
        if( index == -1 )
            return float.PositiveInfinity;

        Vector3 prevIndexPos = _path.Nodes[index];
        Vector3 toClosest = projected - prevIndexPos;
        float prevIndexLength = _path.CalculateLength( index );


        float lengthAtPoint = prevIndexLength + Vector3.Magnitude( toClosest );
        float fullPathLen = _path.CalculateLength();
        return lengthAtPoint / fullPathLen;
    }
}