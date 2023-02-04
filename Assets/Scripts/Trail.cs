using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    [SerializeField] private GameObject _trailPrefab;
    [SerializeField] private GameConfig _gameConfig;

    private float _startTime;
    private float _accumulator;

    private List<GameObject> _trailObjects = new();

    private void OnEnable()
    {
        _accumulator = 0;
    }

    private void OnDisable()
    {
        foreach (var trailObject in _trailObjects)
        {
            Destroy(trailObject);
        }
        _trailObjects.Clear();
    }

    private void Update()
    {
        if (_accumulator > _gameConfig.TimeBetweenTrailSpawn)
        {
            _accumulator = 0;
            var trail = Instantiate(_trailPrefab, transform.position, Quaternion.identity);
            _trailObjects.Add(trail);
        }
        _accumulator += Time.deltaTime;
    }
}