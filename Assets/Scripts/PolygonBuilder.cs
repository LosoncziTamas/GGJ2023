using System.Collections.Generic;
using UnityEngine;

public class PolygonBuilder : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D _polygonCollider2D;
    [SerializeField] private Transform _topLeftCorner;
    [SerializeField] private Transform _bottomLeftCorner;
    [SerializeField] private Transform _topRightCorner;
    [SerializeField] private Transform _bottomRightCorner;

    public void Build(List<Vector3> points)
    {
        var newPoints = new List<Vector2>();
        foreach (var point in points)
        {
            var vec2 = new Vector2(point.x, point.z);
            newPoints.Add(vec2);
        }
        // TODO: remove test data
        newPoints.Add(new Vector2(_topLeftCorner.position.x, _topLeftCorner.position.z));
        newPoints.Add(new Vector2(_bottomLeftCorner.position.x, _bottomLeftCorner.position.z));
        _polygonCollider2D.points = newPoints.ToArray();
    }

    public bool IsInside(Vector3 position)
    {
        var vec2 = new Vector3(position.x, position.z);
        return _polygonCollider2D.OverlapPoint(vec2);
    }
}
