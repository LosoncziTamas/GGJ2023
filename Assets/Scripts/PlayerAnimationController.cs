using System.Collections.Generic;
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
    [SerializeField] private List<AudioClip> _stepAudioClips;
    [SerializeField] private AudioSource _audioSource;

    public void WalkForward()
    {
        _animator.SetBool(WalkRightProperty, true);
        _animator.SetBool(WalkBackwardProperty, false);
        _animator.SetBool(WalkForwardProperty, false);
        _animator.SetBool(WalkLeftProperty, false);
    }

    public void WalkBackward()
    {        
        _animator.SetBool(WalkLeftProperty, true);
        _animator.SetBool(WalkRightProperty, false);
        _animator.SetBool(WalkBackwardProperty, false);
        _animator.SetBool(WalkForwardProperty, false);
    }

    public void WalkRight()
    {
        _animator.SetBool(WalkForwardProperty, true);
        _animator.SetBool(WalkBackwardProperty, false);
        _animator.SetBool(WalkRightProperty, false);
        _animator.SetBool(WalkLeftProperty, false);
    }
    
    public void WalkLeft()
    {
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
        _audioSource.Stop();
    }

    [UsedImplicitly]
    public void OnStep()
    {
        var clip = _stepAudioClips.GetRandom();
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}