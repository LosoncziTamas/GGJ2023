using System;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static List<Enemy> Instances = new List<Enemy>();

    [SerializeField] private AntAnimationController _animationController;
    [SerializeField] private EnemyConfig _enemyConfig;
    [SerializeField] private bool _drawGizmos;
    
    private Vector3 _velocity;
    private Tile _closestTile;
    private Tile _targetTile;

    private bool _isIdle;
    private TileSequenceTracker _sequenceTracker;
    private TileManager _tileManager;
    private Vector3 _originalScale;
    private Transform _transform;

    public bool Idle => _isIdle;

    private void Awake()
    {
        Instances.Add(this);
        _transform = transform;
        _sequenceTracker = FindObjectOfType<TileSequenceTracker>();
        _tileManager = FindObjectOfType<TileManager>();
        _originalScale = _transform.localScale;
        _transform.localScale = Vector3.one * 0.01f;
    }

    private void OnEnable()
    {
        _sequenceTracker.OnTilesCaptured += OnTilesCaptured;
    }
    
    private void OnDisable()
    {
        _sequenceTracker.OnTilesCaptured -= OnTilesCaptured;
    }

    private void OnTilesCaptured()
    {
        Debug.Log("OnTilesCaptured");
        if (!CheckCapture())
        {
            SelectRandomSlipperyTileAsTarget();
        }
    }

    private void SelectRandomSlipperyTileAsTarget()
    {
       var tiles =  _tileManager.GetTileByType(TileType.Slippery);
       if (tiles.Count > 0)
       {
           var newTile = tiles.GetRandom();
           while (newTile == _targetTile)
           {
               newTile = tiles.GetRandom();
               // TODO: check capture?
           }
           _targetTile = newTile;
       }
       Debug.Log("SelectRandomSlipperyTileAsTarget " + _targetTile.Coordinates);
    }

    private bool CheckCapture()
    {
        if (_closestTile.TileType == TileType.Walkable)
        {
            Die();
            return true;
        }
        var surroundingTiles = _tileManager.GetSurroundingTiles(_closestTile);
        var hasSlipperySurrounding = false;
        foreach (var surroundingTile in surroundingTiles)
        {
            if (surroundingTile.TileType == TileType.Slippery)
            {
                hasSlipperySurrounding = true;
                _targetTile = surroundingTile;
                break;
            }
        }
        if (!hasSlipperySurrounding)
        {
            Die();
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (_isIdle)
        {
            if (_animationController)
            {
                _animationController.Idle();
            }
            return;
        }

        if (Vector3.Distance(_targetTile.Center, transform.position) < 0.1f)
        {
            SelectRandomSlipperyTileAsTarget();
        }
        else
        {
            var input = _targetTile.Center - transform.position;
            input.Normalize();
            transform.up = input;
            _velocity = new Vector3(input.x, input.y, input.z) * _enemyConfig.EnemyMaxSpeed;
            var desiredDisplacement = _velocity * Time.deltaTime;
            var newPosition = transform.localPosition + desiredDisplacement;
            transform.position = newPosition;
        }
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 4.0f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 4.0f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 4.0f);
        var input = _targetTile.Center - transform.position;
        Gizmos.color = Color.magenta;
        input.Normalize();
        Gizmos.DrawLine(transform.position, transform.position + input * 4.0f);
    }


    private void OnTriggerEnter(Collider other)
    {
        EvaluateCollision(other);
    }

    private void OnTriggerStay(Collider other)
    {
        EvaluateCollision(other);
    }

    private void EvaluateCollision(Collider other)
    {
        if (_isIdle)
        {
            return;
        }
        if (!other.gameObject.CompareTag(Tags.TileTag))
        {
            return;
        }
        var newTile = other.gameObject.GetComponent<Tile>();
        if (_closestTile == null)
        {
            InitializeTargetTile(newTile, other);
        }        
        else if (newTile != _closestTile)
        {
            UpdateClosestTile(newTile, other);
        }
        if (_closestTile.TileType == TileType.Walkable)
        {
            CheckCapture();
        }
    }
    
    private void InitializeTargetTile(Tile newTile, Collider tileCollider)
    {
        _closestTile = newTile;
        if (newTile.TileType == TileType.Walkable)
        {
            CheckCapture();
        }
        else
        {
            _closestTile.SetHighlightEnabled(true, enemy: true);
        }
    }
    
    private void UpdateClosestTile(Tile newTile, Collider tileCollider)
    {
        var selfPos = transform.position;
        var oldTile = _closestTile;
        var oldDistance = Vector3.Distance(oldTile.Center, selfPos);
        var newDistance = Vector3.Distance(newTile.Center, selfPos);
        if (newDistance < oldDistance)
        {
            InitializeTargetTile(newTile, tileCollider);
            oldTile.SetHighlightEnabled(false);
        }
        else if (newTile.TileType == TileType.Walkable)
        {
            CheckCapture();
        }
    }

    public void ClearFromLevel()
    {
        _isIdle = true;
        _transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InOutSine);
        Destroy(gameObject, 0.4f);
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }

    private void Die()
    {
        PlayerAnimationAndAudioController.Instance.PlayLaughSound();
        _isIdle = true;
        _animationController.Die();
    }
    
    public void Init(EnemyConfig enemyConfig)
    {
        transform.position = enemyConfig.SpawnLocation;
        _enemyConfig = enemyConfig;
        SelectRandomSlipperyTileAsTarget();
        if (_animationController)
        {
            _animationController.Walk();
        }
        _transform.DOScale(_originalScale, 0.4f).SetEase(Ease.InOutSine);
    }
}