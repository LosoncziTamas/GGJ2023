using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileType _tileType;
    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private GameObject _markObject;

    public TileType TileType => _tileType;
    
    public void SetHighlightEnabled(bool highlightEnabled)
    {
        _highlightObject.gameObject.SetActive(highlightEnabled);
    }

    public void SetMarkEnabled(bool markEnabled)
    {
        if (_markObject)
        {
            _markObject.gameObject.SetActive(markEnabled);
        }
    }
}