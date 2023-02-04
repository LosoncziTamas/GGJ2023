using UnityEngine;

public class SimplifiedPlayerController : MonoBehaviour
{
    private const string TileTag = "Tile";
    
    [SerializeField, Range(0f, 1f)] private float _bounciness = 0.5f;
    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
    
    private Rect _allowedArea = new(-8.5f, -5f, 17f, 10f);

    private Tile _closestTile;
    
    private void Update()
    {
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
        var playerInput = Vector2.zero;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        var velocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
        var displacement = velocity * Time.deltaTime;
        return transform.localPosition + displacement;
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
        if (!other.CompareTag(TileTag))
        {
            return;
        }
        var newTile = other.GetComponent<Tile>();
        if (_closestTile == null)
        {
            _closestTile = newTile;
            _closestTile.SetHighlightEnabled(true);
        }
        else if (newTile != _closestTile)
        {
            var selfPos = transform.position;
            var oldDistance = Vector3.Distance(_closestTile.transform.position, selfPos);
            var newDistance = Vector3.Distance(newTile.transform.position, selfPos);
            if (newDistance < oldDistance)
            {
                _closestTile.SetHighlightEnabled(false);
                _closestTile = newTile;
                _closestTile.SetHighlightEnabled(true);
            }
        }
    }
}