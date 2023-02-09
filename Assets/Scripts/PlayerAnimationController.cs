using JetBrains.Annotations;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private static readonly int WalkForwardProperty = Animator.StringToHash("Walk Forward");
    private static readonly int WalkBackwardProperty = Animator.StringToHash("Walk Backward");
    private static readonly int WalkRightProperty = Animator.StringToHash("Strafe Right");
    private static readonly int WalkLeftProperty = Animator.StringToHash("Strafe Left");
    private static readonly int DieProperty = Animator.StringToHash("Die");
    
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _stepAudio;

    public void WalkForward()
    {
        Debug.Log("PlayerAnimationController WalkForward");
        _animator.SetBool(WalkRightProperty, true);
        
        _animator.SetBool(WalkBackwardProperty, false);
        _animator.SetBool(WalkForwardProperty, false);
        _animator.SetBool(WalkLeftProperty, false);
    }

    public void WalkBackward()
    {        
        Debug.Log("PlayerAnimationController WalkBackward");
        _animator.SetBool(WalkLeftProperty, true);
        
        _animator.SetBool(WalkRightProperty, false);
        _animator.SetBool(WalkBackwardProperty, false);
        _animator.SetBool(WalkForwardProperty, false);
    }

    public void WalkRight()
    {
        Debug.Log("PlayerAnimationController WalkRight");
        _animator.SetBool(WalkForwardProperty, true);
        
        _animator.SetBool(WalkBackwardProperty, false);
        _animator.SetBool(WalkRightProperty, false);
        _animator.SetBool(WalkLeftProperty, false);
    }
    
    public void WalkLeft()
    {
        Debug.Log("PlayerAnimationController WalkLeft");
        _animator.SetBool(WalkBackwardProperty, true);
        
        _animator.SetBool(WalkForwardProperty, false);
        _animator.SetBool(WalkRightProperty, false);
        _animator.SetBool(WalkLeftProperty, false);
    }

    public void Die()
    {
        _animator.SetTrigger(DieProperty);
    }

    public void Stop()
    {
        _animator.SetBool(WalkForwardProperty, false);
        _animator.SetBool(WalkBackwardProperty, false);
        _animator.SetBool(WalkRightProperty, false);
        _animator.SetBool(WalkLeftProperty, false);
        _stepAudio.Stop();
    }

    [UsedImplicitly]
    public void OnStep()
    {
        Debug.Log("OnStep");
        _stepAudio.Play();
    }
}