using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using Gui;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Trail _trail;
    [SerializeField] private PlayerAnimationAndAudioController _animationAndAudioController;
    [SerializeField] private TileSequenceTracker _tileSequenceTracker;
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private ParticleSystem _aura;

    private Rect _allowedArea = new(-8.5f, -5f, 17f, 10f);
    private bool _isMoving;    
    private Tile _targetTile;
    private Vector3? _slidingVelocity;
    private Vector3 _velocity;
    private Vector3 _startPosition;
    private Vector3 _startScale;
    private Transform _transform;
    private float _speed;
    
    public bool IsSliding => _slidingVelocity.HasValue;

    public void ResetToDefault()
    {
        StopAllCoroutines();
        _speed = _gameConfig.PlayerStarterMaxSpeed;
        _targetTile = null;
        _slidingVelocity = null;
        _velocity = Vector3.zero;
        _isMoving = false;
        _trail.enabled = false;
        _animationAndAudioController.Stop();
        StartCoroutine(DoRepositionAnim());
    }

    public void RaiseSpeed(float scale)
    {
        _speed = _gameConfig.PlayerStarterMaxSpeed + _gameConfig.PlayerAdditionalMaxSpeed * scale;
    }

    private IEnumerator DoRepositionAnim()
    {
        _aura.gameObject.SetActive(false);
        yield return _transform.DOScale(Vector3.zero, 0.3f).WaitForCompletion();
        _transform.position = _startPosition;
        yield return _transform.DOScale(_startScale, 0.3f).WaitForCompletion();
        _aura.gameObject.SetActive(true);
        enabled = true;
    }
    
    private void Start()
    {
        _transform = transform;
        _startScale = _transform.localScale;
        _startPosition = _transform.position;
        _speed = _gameConfig.PlayerStarterMaxSpeed;
    }

    private void OnGUI()
    {
        GUILayout.Label("Speed " + _speed);
    }

    private void Update()
    {
        if (_isMoving)
        {
            return;
        }
        var newPosition = VelocityBasedMovement();
        if (!_allowedArea.Contains(new Vector2(newPosition.x, newPosition.z)))
        {
            if (newPosition.x < _allowedArea.xMin) 
            {
                newPosition.x = _allowedArea.xMin;
            }
            else if (newPosition.x > _allowedArea.xMax) 
            {
                newPosition.x = _allowedArea.xMax;
            }
            if (newPosition.z < _allowedArea.yMin) 
            {
                newPosition.z = _allowedArea.yMin;
            }
            else if (newPosition.z > _allowedArea.yMax) 
            {
                newPosition.z = _allowedArea.yMax;
            }
        }
        transform.localPosition = newPosition;
    }
    
    private Vector3 VelocityBasedMovement()
    {
        var playerInput = ReadInput();
        if (!_slidingVelocity.HasValue)
        {
            _velocity = new Vector3(playerInput.x, 0f, playerInput.y) * _speed;
        }
        UpdateAnimation();
        var desiredDisplacement = _velocity * Time.deltaTime;
        return transform.localPosition + desiredDisplacement;
    }

    private void UpdateAnimation()
    {
        if (_velocity.magnitude == 0)
        {
            _animationAndAudioController.Stop();
        }
        else
        {
            var x = _velocity.x;
            var z = _velocity.z;
            if (x > 0)
            {
                _animationAndAudioController.WalkRight();
            }
            else if (x < 0)
            {
                _animationAndAudioController.WalkLeft();
            }
            else if (z > 0)
            {
                _animationAndAudioController.WalkForward();
            }
            else if (z < 0)
            {
                _animationAndAudioController.WalkBackward();
            }
        }
    }

    public void Die()
    {
        _animationAndAudioController.Die();
        StopAllCoroutines();
        enabled = false;
    }
    
    private Vector2 ReadInput()
    {
        var playerInput = Vector2.zero;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        var xDelta = Mathf.Abs(playerInput.x);
        var yDelta = Mathf.Abs(playerInput.y);
        if (xDelta > yDelta)
        {
            playerInput = new Vector2(playerInput.x, 0);
        }
        else if (yDelta > xDelta)
        {
            playerInput = new Vector2(0, playerInput.y);
        }
        else
        {
            playerInput = Vector2.zero;
        }
        return playerInput;
    }

    private IEnumerator MoveToTile(Tile targetTile)
    {
        if (_isMoving)
        {
            yield break;
        }
        _isMoving = true;
        var targetPosition = new Vector3(targetTile.Center.x, targetTile.Center.y, transform.position.z);
        while (true)
        {
            // TODO: use more elegant fix for repositioning
            if (GameMaster.Instance.Initializing)
            {
                ResetToDefault();
                yield break;
            }
            var distance = Vector3.Distance(targetPosition, transform.position);
            if (distance < 0.01f)
            {
                transform.position = targetPosition;
                _isMoving = false;
                yield break;
            }
            var newPos = Vector3.MoveTowards(transform.position, targetPosition, _speed * Time.deltaTime);
            transform.position = newPos;
            yield return null;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        EvaluateTrigger(other);
    }

    private void OnTriggerStay(Collider other)
    {
        EvaluateTrigger(other);
    }

    private void EvaluateTrigger(Collider other)
    {
        if (_isMoving && IsSliding)
        {
            return;
        }
        if (!other.CompareTag(Tags.TileTag))
        {
            return;
        }
        var newTile = other.GetComponent<Tile>();
        if (_targetTile == null)
        {
            InitializeTargetTile(newTile);
        }        
        else if (newTile != _targetTile)
        {
            UpdateClosestTile(newTile);
        }
    }

    private void InitializeTargetTile(Tile newTile)
    {
        _targetTile = newTile;
        if (newTile.TileType == TileType.Walkable)
        {
            _slidingVelocity = null;
        }
        if (!IsTileBeingCaptured(_targetTile))
        {
            _targetTile.SetHighlightEnabled(true, enemy: false);
        }
        StartCoroutine(MoveToTile(_targetTile));
    }
    
    private void UpdateClosestTile(Tile newTile)
    {
        var selfPos = transform.position;
        var oldTile = _targetTile;
        var oldDistance = Vector3.Distance(oldTile.Center, selfPos);
        var newDistance = Vector3.Distance(newTile.Center, selfPos);
        if (newDistance < oldDistance)
        {
            var lastTileWasWalkable = oldTile.TileType == TileType.Walkable;
            if (newTile.TileType == TileType.Walkable && IsSliding)
            {
                EndSliding(newTile);
            }            
            else if (newTile.TileType == TileType.Slippery && lastTileWasWalkable)
            {
                BeginSliding(oldTile, newTile);
            }
            else if (IsSliding)
            {
                AddTileToSequence(newTile);
            }
            InitializeTargetTile(newTile);
            if (!IsTileBeingCaptured(oldTile))
            {
                oldTile.SetHighlightEnabled(false);
            }
        }
    }

    private bool IsTileBeingCaptured(Tile tile)
    {
        return _tileSequenceTracker.TileIsInSequence(tile);
    }
    
    private void BeginSliding(Tile oldTile, Tile newTile)
    {
        newTile.MarkCapturing();
        _tileSequenceTracker.BeginTracking(oldTile, newTile);
        _slidingVelocity = _velocity.normalized * _speed;
        _trail.enabled = true;
    }

    private void EndSliding(Tile newTile)
    {
        _trail.enabled = false;
        _tileSequenceTracker.EndTracking(newTile);
    }

    private void AddTileToSequence(Tile newTile)
    {
        _tileSequenceTracker.Add(newTile);
        newTile.MarkCapturing();
    }
}