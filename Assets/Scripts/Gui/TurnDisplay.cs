using TMPro;
using UnityEngine;

namespace Gui
{
    public class TurnDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void SetCount(int count)
        {
            _text.text = $"Allowed turn count: {count}";
        }
    }
}