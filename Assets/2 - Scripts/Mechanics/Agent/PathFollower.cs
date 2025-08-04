using MDC.Pathfinding;
using UnityEngine;


public class PathFollower
{
    private const float PATHING_GOAL_DIST = .1f;

    private AStarPath _path = null;
    private Agent _agent = null;
    private Vector3[] _pathPoints = null;
    private int _pathIndex = 0;
    private float _pathGoalDistSqr = 0f;

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
        _pathPoints = _path.GetWorldPoints( world.WorldBounds.min );
        _pathIndex = 0;

        IsComplete = false;

    }

    public void FollowPath()
    {
        if( IsComplete )
            return;

        if( _pathPoints != null )
        {
            Vector3 toPoint = _pathPoints[_pathIndex] - _agent.transform.position;
            _agent.Move( toPoint );

            Vector3 postMoveToPoint = _pathPoints[_pathIndex] - _agent.transform.position;
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
}