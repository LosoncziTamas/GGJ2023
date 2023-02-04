using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    
    [SerializeField] private LayerMask _walkableMask = -1;
    [SerializeField] private LayerMask _slipperyMask = -1;
    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] private float _maxAcceleration = 10f;

    private Renderer _renderer;
    private Rigidbody _rigidBody;
    private Vector3 _desiredOffset;
    private Vector3 _position;
    private int _walkableCount;
    private int _slipperyCount;

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
        _desiredOffset = new Vector3(playerInput.x, 0f, playerInput.y) * _maxSpeed;
        UpdateColor();
    }
    
    private void UpdateColor()
    {
        _renderer.material.SetColor(ColorProperty, OnWalkableSide ? Color.white : Color.red);
    }

    private void FixedUpdate()
    {
        _position = _rigidBody.position;
        var acceleration = _maxAcceleration;
        var maxSpeedChange = acceleration * Time.deltaTime;
        var desiredPosition = _position + _desiredOffset;
        _position = Vector3.MoveTowards(_position, desiredPosition, maxSpeedChange);
        _rigidBody.position = _position;
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
        else if (IsMaskSet(_slipperyMask, layerIndex))
        {
            _slipperyCount++;
        }
    }

    private static bool IsMaskSet(LayerMask layerMask, int layerIndex)
    {
        return (layerMask & (1 << layerIndex)) != 0;
    }

    private void ResetState()
    {
        _walkableCount = _slipperyCount = 0;
    }
    
}
