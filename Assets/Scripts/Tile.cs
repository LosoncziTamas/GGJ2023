using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject _highlightObject;
    
    public void SetHighlightEnabled(bool highlightEnabled)
    {
        _highlightObject.gameObject.SetActive(highlightEnabled);
    }
}