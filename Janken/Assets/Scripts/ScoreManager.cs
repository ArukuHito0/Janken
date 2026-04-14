using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI p1_scoreText;
    [SerializeField]
    private TextMeshProUGUI p2_scoreText;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void SetScoreText(int p1_score, int p2_score)
    {
        p1_scoreText.text = $"{p1_score} / 3";
        p2_scoreText.text = $"{p2_score} / 3";
    }
}
