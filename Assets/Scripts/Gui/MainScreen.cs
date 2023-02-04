using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace Gui
{
    public class MainScreen : MonoBehaviour
    {
        [SerializeField] private float _fadeAnimDuration = 0.6f;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _canvasGroup.alpha = 0.0f;
            _canvasGroup.DOFade(1.0f, _fadeAnimDuration);
        }

        private void OnEnable()
        {
            _startButton.onClick.AddListener(OnStartButtonClick);
            _creditsButton.onClick.AddListener(OnCreditsButtonClick);
        }

        private void OnDisable()
        {
            _startButton.onClick.RemoveListener(OnStartButtonClick);
            _creditsButton.onClick.RemoveListener(OnCreditsButtonClick);
        }

        private async void OnStartButtonClick()
        {
            await CreateDisappearAnimSequence().AsyncWaitForCompletion();
            var result = await GameMaster.Instance.StartGame();
            await CreateAppearAnimSequence().AsyncWaitForCompletion();
            if (result == GameResult.Completed)
            {
                // TODO: display modal
            }
        }
        
        private void OnCreditsButtonClick()
        {
            
        }
        
        private Sequence CreateDisappearAnimSequence()
        {
            return DOTween.Sequence(PanelFadeAnim(visible: false));
        }
        
        private Sequence CreateAppearAnimSequence()
        {
            return  DOTween.Sequence(PanelFadeAnim(visible: true));
        }

        private TweenerCore<float, float, FloatOptions> PanelFadeAnim(bool visible)
        {
            void OnFadeComplete()
            {
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }
            return _canvasGroup.DOFade(visible ? 1.0f : 0.0f, _fadeAnimDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(OnFadeComplete);
        }
    }
}