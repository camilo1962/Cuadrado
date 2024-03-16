using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Squared
{
    [AddComponentMenu("Squared/Level Completed UI")]
    [DisallowMultipleComponent]
    public class LevelCompletedUI : MonoBehaviour
    {
        public static LevelCompletedUI instance;

        #region Inspector Fields
        [SerializeField] private Button _menuButton = null;
        [SerializeField] private Button _nextButton = null;

      
        [SerializeField] private SceneTransition _sceneTransition = null;
        [SerializeField] private LevelSO[] _levelSOs = { };
        #endregion

        #region Unity Methods
        public void Awake()
        {
            instance = this;
            _menuButton.onClick.AddListener(() => _sceneTransition.TransitionTo("Menu"));
            _nextButton.onClick.AddListener(() =>
            {
                Board.LevelSOIndex++;
                _sceneTransition.TransitionTo("Game");
            });

            if (Board.LevelSOIndex == _levelSOs.Length - 1) _nextButton.interactable = false;
        }
        #endregion
       
    }
}
