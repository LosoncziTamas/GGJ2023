using System.Collections.Generic;
using UnityEngine;

public class PolygonBuilder : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D _polygonCollider2D;
    [SerializeField] private Transform _topLeftCorner;
    [SerializeField] private Transform _bottomLeftCorner;
    [SerializeField] private Transform _topRightCorner;
    [SerializeField] private Transform _bottomRightCorner;

    private readonly List<Vector2> _positions = new List<Vector2>();

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
        if (_positions.Count < 4)
        {
            // TODO: adding algorithm
            // _positions.Add(new Vector2(_topLeftCorner.position.x, _topLeftCorner.position.z));
            _positions.Add(new Vector2(_bottomLeftCorner.position.x, _bottomLeftCorner.position.z));
        }
        else
        {
            
        }
        _polygonCollider2D.points = _positions.ToArray();
    }

    public bool IsInside(Vector3 position)
    {
        return _polygonCollider2D.OverlapPoint(position);
    }
}
