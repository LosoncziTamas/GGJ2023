using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    
    [SerializeField] private LayerMask _walkableMask = -1;
    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] private float _maxAcceleration = 10f;

    private Renderer _renderer;
    private Rigidbody _rigidBody;
    private Vector3 _desiredVelocity;
    private Vector3 _velocity;
    private int _walkableCount;

    private bool OnWalkableSide => _walkableCount > 0;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var playerInput = Vector2.zero;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        _desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
        UpdateColor();
    }
    
    private void UpdateColor()
    {
        _renderer.material.SetColor(ColorProperty, OnWalkableSide ? Color.black : Color.white);
    }

    private void FixedUpdate()
    {
        _velocity = _rigidBody.velocity;
        var acceleration = _maxAcceleration;
        var maxSpeedChange = acceleration * Time.deltaTime;
        var currentX = _velocity.x;
        var currentZ = _velocity.z;
        var newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        var newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
        _velocity = new Vector3(newX, 0, newZ);
        _rigidBody.velocity = _velocity;
        ResetState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        EvaluateCollision(collisionInfo);
    }

    private void EvaluateCollision(Collision collision)
    {
        var layerIndex = collision.gameObject.layer;
        if (IsMaskSet(_walkableMask, layerIndex))
        {
            _walkableCount++;
        }
        else
        {
            
        }
    }

    private static bool IsMaskSet(LayerMask layerMask, int layerIndex)
    {
        return (layerMask & layerIndex) != 0;
    }

    private void ResetState()
    {
        _walkableCount = 0;
    }
    
}
