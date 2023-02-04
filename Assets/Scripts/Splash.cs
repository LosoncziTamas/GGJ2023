using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{
    public const string MainSceneName = "Game Scene";
    public const string LoaderSceneName = "Splash Scene";
    
    private const float FadeDuration = 1.0f;
    
    [SerializeField] private CanvasGroup _loadingOverlay;
    
    private IEnumerator Start()
    {
        var sceneLoad = SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Additive);
        sceneLoad.allowSceneActivation = false;
        _loadingOverlay.DOFade(1, FadeDuration).SetEase(Ease.Linear);
        while (sceneLoad.progress < 0.9f)
        {
            yield return null;
        }
        sceneLoad.allowSceneActivation = true;
        var mainScene = SceneManager.GetSceneByName(MainSceneName);
        while (!mainScene.isLoaded)
        {
            yield return null;
        }
        _loadingOverlay.blocksRaycasts = false;
        _loadingOverlay.interactable = false;
        SceneManager.SetActiveScene(mainScene);
        yield return _loadingOverlay.DOFade(0, FadeDuration).SetEase(Ease.Linear).WaitForCompletion();
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(LoaderSceneName));
    }
}