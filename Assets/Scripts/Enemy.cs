using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;
    
    private Vector2 _randomInput;
    private Vector3 _velocity;
    private Tile _closestTile;
    
    private void Start()
    {
        var randomDirection = Random.insideUnitCircle;
        Debug.Log("randomDirection " + randomDirection);
        _randomInput = randomDirection;
    }

    private void Update()
    {
        _velocity = new Vector3(_randomInput.x, _randomInput.y, 0f) * _gameConfig.EnemyMaxSpeed;
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
    }
    
    private void InitializeTargetTile(Tile newTile, Collision collision)
    {
        _closestTile = newTile;
        if (newTile.TileType == TileType.Walkable)
        {
            ChangeDirection(collision);
        }
        _closestTile.SetHighlightEnabled(true);
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
        _randomInput = contactNormal;
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}