using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerAnimationAndAudioController : MonoBehaviour
{
    public static PlayerAnimationAndAudioController Instance;
    
    private static readonly int WalkForwardProperty = Animator.StringToHash("Walk Forward");
    private static readonly int WalkBackwardProperty = Animator.StringToHash("Walk Backward");
    private static readonly int WalkRightProperty = Animator.StringToHash("Strafe Right");
    private static readonly int WalkLeftProperty = Animator.StringToHash("Strafe Left");
    private static readonly int DieProperty = Animator.StringToHash("Die");
    
    [SerializeField] private Animator _animator;
    [SerializeField] private List<AudioClip> _stepAudioClips;
    [SerializeField] private AudioClip _laughAudioClip;
    [SerializeField] private AudioClip _dieAudioClip;
    [SerializeField] private AudioClip _impactAudioClip;
    [SerializeField] private AudioClip _woodHitAudioClip;
    [SerializeField] private AudioSource _stepsAudioSource;
    [SerializeField] private AudioSource _casualAudioSource;
    [SerializeField] private AudioSource _impactAudioSource;

    private PlayerController _playerController;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        Instance = this;
    }

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
        _stepsAudioSource.Stop();
    }

    [UsedImplicitly]
    public void OnStep()
    {
        var clip = _stepAudioClips.GetRandom();
        _stepsAudioSource.clip = clip;
        _stepsAudioSource.Play();
    }
    
    public void PlayDieSound()
    {
        _casualAudioSource.clip = _dieAudioClip;
        _casualAudioSource.Play();
    }

    public void PlayFallSound()
    {
        _stepsAudioSource.clip = _woodHitAudioClip;
        _stepsAudioSource.Play();
    }

    public void PlayLaughSound()
    {
        _casualAudioSource.clip = _laughAudioClip;
        _casualAudioSource.Play();
    }

    public void PlayImpactSound()
    {
        _impactAudioSource.clip = _impactAudioClip;
        _impactAudioSource.Play();
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}