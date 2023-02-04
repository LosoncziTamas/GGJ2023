using UnityEngine;

public class TrailElement : MonoBehaviour
{
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