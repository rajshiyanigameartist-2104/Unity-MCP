using TMPro;
using UnityEngine;

namespace Match3.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text movesText;
        [SerializeField] private GameObject completePopup;
        [SerializeField] private TMP_Text completeLabel;

        public void Refresh(int score, int moves)
        {
            scoreText.text = $"Score: {score}";
            movesText.text = $"Moves: {moves}";
        }

        public void ShowComplete(bool win)
        {
            completePopup.SetActive(true);
            completeLabel.text = win ? "Level Complete!" : "Try Again";
        }

        public void HideComplete()
        {
            completePopup.SetActive(false);
        }
    }
}
