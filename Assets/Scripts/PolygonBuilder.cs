using System.Collections.Generic;
using UnityEngine;

public class PolygonBuilder : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D _polygonCollider2D;
    [SerializeField] private Transform _topLeftCorner;
    [SerializeField] private Transform _bottomLeftCorner;
    [SerializeField] private Transform _topRightCorner;
    [SerializeField] private Transform _bottomRightCorner;

    private List<Vector2> _positions;

    public void Add(Vector3 position)
    {
        var points = new List<Vector2>(){position};
        points.Add(new Vector2(_topLeftCorner.position.x, _topLeftCorner.position.z));
        points.Add(new Vector2(_bottomLeftCorner.position.x, _bottomLeftCorner.position.z));
        _polygonCollider2D.points = points.ToArray();
    }

    public void Clear()
    {
        _polygonCollider2D.pathCount = 0;
        _polygonCollider2D.points = new Vector2[] { };
    }
    
    public void Build(List<Vector3> points)
    {
       /* var newPoints = new List<Vector2>();
        foreach (var point in points)
        {
            var vec2 = new Vector2(point.x, point.y);
            newPoints.Add(vec2);
        }
        // TODO: remove test data
        newPoints.Add(new Vector2(_topLeftCorner.position.x, _topLeftCorner.position.z));
        newPoints.Add(new Vector2(_bottomLeftCorner.position.x, _bottomLeftCorner.position.z));*/
    }

    public bool IsInside(Vector3 position)
    {
        return _polygonCollider2D.OverlapPoint(position);
    }
}
