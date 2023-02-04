using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileType _tileType;
    [SerializeField] private GameObject _highlightObject;

    public TileType TileType => _tileType;
    
    public void SetHighlightEnabled(bool highlightEnabled)
    {
        _highlightObject.gameObject.SetActive(highlightEnabled);
    }
}