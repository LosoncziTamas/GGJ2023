using System;
using System.Collections.Generic;
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
        _polygonCollider2D.pathCount = 0;
        _polygonCollider2D.points = new Vector2[] { };
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
        else if (positionCount is < 4 and > 0)
        {
            // TODO: adding algorithm
            

            Add(_bottomLeft);
        }
        else
        {
            
        }
        _polygonCollider2D.points = _positions.ToArray();
    }

    private void HandleStraightLine(Vector2 startPos, Vector2 endPos)
    {
        var displacement = endPos - startPos;
        var horizontal = Mathf.Abs(displacement.x) > Mathf.Abs(displacement.y);
        var leftToRight = displacement.x > 0;
        var rightToLeft = displacement.x > 0;
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
                
        }
    }

    public bool IsInside(Vector3 position)
    {
        return _polygonCollider2D.OverlapPoint(position);
    }
}
