using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Squared
{
    [AddComponentMenu("Squared/Level Choice")]
    [DisallowMultipleComponent]
    public class LevelChoice : MonoBehaviour
    {
        public static LevelChoice instance;

        #region Inspector Fields
        [SerializeField] private TextMeshProUGUI _numberLabel = null;

        [SerializeField] private Button _selectButton = null;
        #endregion
        
        #region Runtime Fields
        public int _levelIndex = 0;
       // public float record;
        #endregion
        public void Awake()
        {
            instance = this;
           
        }
        #region Public Methods
        public void Initialize(int levelIndex, System.Action<int> onSelect)
        {
            _levelIndex = levelIndex;
            _numberLabel.text = $"{_levelIndex + 1}";
           
            _selectButton.onClick.AddListener(() => onSelect(_levelIndex));
          

        }

        #endregion
       
    }
}
