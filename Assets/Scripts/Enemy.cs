using Configs;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private AntAnimationController _animationController;
    
    private Vector2 _randomInput;
    private Vector3 _velocity;
    private Tile _closestTile;
    private EnemyConfig _enemyConfig;

    private bool _captured;

    public void Init(EnemyConfig enemyConfig)
    {
        transform.position = enemyConfig.SpawnLocation;
        _enemyConfig = enemyConfig;
        if (enemyConfig.StartDirection.magnitude > 0)
        {
            _randomInput = enemyConfig.StartDirection.normalized;
        }
        else
        {
            _randomInput = Random.insideUnitCircle;
        }

        if (_animationController)
        {
            _animationController.Walk();
        }
    }

    private void Update()
    {
        if (_captured)
        {
            if (_animationController)
            {
                _animationController.Idle();
            }
            return;
        }
        _velocity = new Vector3(_randomInput.x, _randomInput.y, 0f) * _enemyConfig.EnemyMaxSpeed;
        var desiredDisplacement = _velocity * Time.deltaTime;
        var newPosition = transform.localPosition + desiredDisplacement;
        transform.position = newPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        EvaluateCollision(collisionInfo);
    }
    
    private void EvaluateCollision(Collision other)
    {
        if (_captured)
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
            _captured = CheckCapture();
        }
    }

    private bool CheckCapture()
    {
        // TODO: optimize
        var hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale, Quaternion.identity);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag(Tags.TileTag))
            {
                var nearbyTile = hitCollider.GetComponent<Tile>();
                if (nearbyTile.TileType == TileType.Slippery)
                {
                    Debug.Log("CheckCapture: false");
                    return false;
                }
            }
        }
        Debug.Log("CheckCapture: true");
        return true;
    }

    private void InitializeTargetTile(Tile newTile, Collision collision)
    {
        _closestTile = newTile;
        if (newTile.TileType == TileType.Walkable)
        {
            ChangeDirection(collision);
        }
        else
        {
            _closestTile.SetHighlightEnabled(true);
        }
    }
    
    private void UpdateClosestTile(Tile newTile, Collision collision)
    {
        var selfPos = transform.position;
        var oldTile = _closestTile;
        var oldDistance = Vector3.Distance(oldTile.transform.position, selfPos);
        var newDistance = Vector3.Distance(newTile.transform.position, selfPos);
        if (newDistance < oldDistance)
        {
            InitializeTargetTile(newTile, collision);
            oldTile.SetHighlightEnabled(false);
        }
        else if (newTile.TileType == TileType.Walkable)
        {
            ChangeDirection(collision);
        }
    }

    private void ChangeDirection(Collision collision)
    {
        var contactNormal = Vector3.zero;
        for (var i = 0; i < collision.contactCount; i++)
        {
            var normal = collision.GetContact(i).normal;
            contactNormal += normal;
        }
        contactNormal.Normalize();
        var reflection = Vector3.Reflect(_velocity, transform.InverseTransformDirection(contactNormal));
        _randomInput = reflection.normalized;
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}