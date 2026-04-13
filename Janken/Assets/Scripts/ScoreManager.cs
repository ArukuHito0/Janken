using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI p1_scoreText;
    [SerializeField]
    private TextMeshProUGUI p2_scoreText;

    private void OnEnable()
    {
        FindObjectOfType<BattleManager>().onScoreChanged += SetScoreText;
    }

    private void OnDisable()
    {
        FindObjectOfType<BattleManager>().onScoreChanged -= SetScoreText;
    }

    private void SetScoreText(int p1_score, int p2_score)
    {
        p1_scoreText.text = $"{p1_score} / 3";
        p2_scoreText.text = $"{p2_score} / 3";
    }
}
