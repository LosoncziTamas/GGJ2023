using UnityEngine;

public class AntAnimationController : MonoBehaviour
{
    private static readonly int WalkProperty = Animator.StringToHash("Walk");
    private static readonly int DieProperty = Animator.StringToHash("Die");
    
    [SerializeField] private Animator _animator;

    public void Walk()
    {
        _animator.SetBool(WalkProperty, true);
        _animator.SetBool(DieProperty, false);
    }

    public void Idle()
    {
        _animator.SetBool(WalkProperty, false);
        _animator.SetBool(DieProperty, false);
    }
    
    public void Die()
    {
        _animator.SetBool(WalkProperty, false);
        _animator.SetBool(DieProperty, true);
    }
}