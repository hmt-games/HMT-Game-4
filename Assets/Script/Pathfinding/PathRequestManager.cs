using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    public static PathRequestManager Instance;

    private Queue<PathRequest> _pathRequests;
    private bool _processingPath = false;
    private PathRequest _currentPathRequest;
    private AStarPathfinding _aStarPathfinding;

    private void Awake()
    {
        Instance = this;
        _aStarPathfinding = GetComponent<AStarPathfinding>();
        _pathRequests = new Queue<PathRequest>();
    }

    public void RequestPath(Vector3 startPosition, Vector3 targetPosition, Action<Vector3[], bool> callback) 
    {
        _pathRequests.Enqueue(new PathRequest(startPosition, targetPosition, callback));
        ProcessNextPath();
    }

    private void ProcessNextPath()
    {
        if (!_processingPath && _pathRequests.Count > 0)
        {
            PathRequest pathRequest = _pathRequests.Dequeue();
            _currentPathRequest = pathRequest;
            _processingPath = true;
            _aStarPathfinding.TryFindPath(_currentPathRequest.StartPosition, _currentPathRequest.TargetPosition);
        }
    }

    public void FinishProcessingPath(Vector3[] waypoints, bool success)
    {
        _processingPath = false;
        _currentPathRequest.Callback(waypoints, success);
        ProcessNextPath();
    }
    
    private struct PathRequest
    {
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public Action<Vector3[], bool> Callback;

        public PathRequest(Vector3 startPosition, Vector3 targetPosition, Action<Vector3[], bool> callback)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Callback = callback;
        }
    }
}
