using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrailElement : MonoBehaviour
{

    [SerializeField] private Transform _child;
    
    private void Start()
    {
        _child.rotation = Random.rotation;
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
        if (collision.gameObject.CompareTag(Tags.EnemyTag))
        {
            var gm = GameMaster.Instance;
            gm.LifeLost();
        }
    }
}