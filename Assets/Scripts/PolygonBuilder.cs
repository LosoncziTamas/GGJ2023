using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolygonBuilder : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D _polygonCollider2D;
    [SerializeField] private Transform _topLeftCorner;
    [SerializeField] private Transform _bottomLeftCorner;
    [SerializeField] private Transform _topRightCorner;
    [SerializeField] private Transform _bottomRightCorner;

    private Vector2 _topLeft;
    private Vector2 _bottomLeft;
    private Vector2 _topRight;
    private Vector2 _bottomRight;

    private readonly List<Vector2> _positions = new();
    private Vector3 _colliderCenter;

    private void Start()
    {
        _bottomLeft = new Vector2(_bottomLeftCorner.position.x, _bottomLeftCorner.position.y);
        _topLeft = new Vector2(_topLeftCorner.position.x, _topLeftCorner.position.y);
        _topRight = new Vector2(_topRightCorner.position.x, _topRightCorner.position.y);
        _bottomRight = new Vector2(_bottomRightCorner.position.x, _bottomRightCorner.position.y);
    }

    public void Add(Vector3 position)
    {
        _positions.Add(position);
    }

    public void Clear()
    {
        _positions.Clear();
        _polygonCollider2D.pathCount = 0;
        _polygonCollider2D.points = null;
    }
    
    public void Build()
    {
        var positionCount = _positions.Count;
        if (positionCount < 2)
        {
            Debug.LogError("Not enough positions.");
            return;
        }
        var startPos = _positions[0];
        var endPos = _positions[positionCount - 1];
        var isStraightLine = positionCount == 2;
        if (isStraightLine)
        {
            HandleStraightLine(startPos, endPos);
        }
        else if (positionCount == 3)
        {
            HandleTriangle(startPos, _positions[1], endPos);
            Debug.Log("HandleTriangle " + startPos + " " + endPos);
        }
        else
        {
            HandlePolygon(startPos, endPos);
            Debug.Log("HandlePolygon " + startPos + " " + endPos);
        }
        PerformClockwiseOrdering();
    }

    private void PerformClockwiseOrdering()
    {
        _polygonCollider2D.points = _positions.ToArray();
        _colliderCenter = _polygonCollider2D.bounds.center;
        var ordered = _positions.OrderBy(x => Math.Atan2(x.x - _colliderCenter.x, x.y - _colliderCenter.y)).ToList();
        _polygonCollider2D.points = ordered.ToArray();
    }

    private void HandleTriangle(Vector2 start, Vector2 mid, Vector2 end)
    {
        var midAboveEnd = Greater(mid.y, end.y);
        if (midAboveEnd)
        {
            Add(new Vector2(start.x, end.y));
        }
        else if (Approximately(mid.y, end.y))
        {
            Add(new Vector2(end.x, start.y));
        }
        else if (Approximately(mid.y, start.y))
        {
            Add(new Vector2(start.x, end.y));
        }
        else
        {
            var midIsBetween = mid.y < end.y && mid.y > start.y;
            if (midIsBetween)
            {
                Add(new Vector2(end.x, start.y));
            }
            else
            {
                Debug.Break();
            }
        }
    }

    private bool Greater(float a, float b, float threshold = 0.01f)
    {
        var result = a - b;
        return result > threshold;
    }

    private bool Approximately(float a, float b, float threshold = 0.01f)
    {
        var diff = Mathf.Abs(a - b);
        return diff < threshold;
    }

    private void HandlePolygon(Vector2 start, Vector2 end)
    {
        
    }

    private float CalculateAccumulatedDistanceToCorner(List<Vector2> points, Vector2 corner)
    {
        var accumulatedDistance = 0f;
        foreach (var point in points)
        {
            accumulatedDistance += Vector2.Distance(point, corner);
        }
        return accumulatedDistance;
    }

    private Vector2? GetClosestCornerToPoints(List<Vector2> points)
    {
        var topLeftDistance = CalculateAccumulatedDistanceToCorner(points, _topLeft);
        var topRightDistance = CalculateAccumulatedDistanceToCorner(points, _topRight);
        var bottomLeftDistance = CalculateAccumulatedDistanceToCorner(points, _bottomLeft);
        var bottomRightDistance = CalculateAccumulatedDistanceToCorner(points, _bottomRight);
        var minDistance = Mathf.Min(topLeftDistance, bottomLeftDistance, topRightDistance, bottomRightDistance);
        if (Mathf.Approximately(minDistance, topLeftDistance))
        {
            return _topLeft;
        }
        if (Mathf.Approximately(minDistance, topRightDistance))
        {
            return _topRight;
        }
        if (Mathf.Approximately(minDistance, bottomLeftDistance))
        {
            return _bottomLeft;
        }
        if (Mathf.Approximately(minDistance, bottomRightDistance))
        {
            return _bottomRight;
        }
        return null;
    }

    private void HandleStraightLine(Vector2 startPos, Vector2 endPos)
    {
        var displacement = endPos - startPos;
        var horizontal = Mathf.Abs(displacement.x) > Mathf.Abs(displacement.y);
        var leftToRight = displacement.x > 0;
        var bottomToTop = displacement.y > 0;
        if (horizontal)
        {
            var yTopDistance = Mathf.Abs(startPos.y - _topLeft.y);
            var yBottomDistance = Mathf.Abs(startPos.y - _bottomLeft.y);
            if (yTopDistance < yBottomDistance)
            {
                Add(leftToRight ? _topRight : _topLeft);
                Add(leftToRight ? _topLeft : _topRight);
            }
            else
            {
                Add(leftToRight ? _bottomRight : _bottomLeft);
                Add(leftToRight ? _bottomLeft : _bottomRight);
            }
        }
        else
        {
            var xLeftDistance = Mathf.Abs(startPos.x - _bottomLeft.x);
            var xRightDistance = Mathf.Abs(startPos.x - _bottomRight.x);
            if (xLeftDistance < xRightDistance)
            {
                Add(bottomToTop ? _topLeft : _bottomLeft);
                Add(bottomToTop ? _bottomLeft : _topLeft);
            }
            else
            {
                Add(bottomToTop ? _topRight : _bottomRight);
                Add(bottomToTop ? _bottomRight : _topRight);
            }
        }
    }

    public void UpdateCorners(List<Vector3> resolvedPositions)
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_colliderCenter, Vector3.one);
        foreach (var position in _positions)
        {
            Gizmos.DrawWireSphere(position, 0.2f);
        }
        //Gizmos.DrawCube(_topLeft, Vector3.one);
        //Gizmos.DrawCube(_bottomLeft, Vector3.one);
        //Gizmos.DrawCube(_topRight, Vector3.one);
        //Gizmos.DrawCube(_bottomRight, Vector3.one);
    }

    public bool IsInside(Vector3 position)
    {
        return _polygonCollider2D.OverlapPoint(position);
    }
}
