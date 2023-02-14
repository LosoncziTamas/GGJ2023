using UnityEngine;

public class AntAnimationController : MonoBehaviour
{
    private static readonly int WalkProperty = Animator.StringToHash("Walk");
    
    [SerializeField] private Animator _animator;

    public void Walk()
    {
        _animator.SetBool(WalkProperty, true);
    }

    public void Idle()
    {
        _animator.SetBool(WalkProperty, false);
    }
    
    public void Die()
    {
        _animator.SetBool(WalkProperty, false);
    }
    
}