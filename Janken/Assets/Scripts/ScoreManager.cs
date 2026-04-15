using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI myScoreText;
    [SerializeField]
    private TextMeshProUGUI enemyScoreText;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void SetScoreText(int playerNum, int p1_score, int p2_score)
    {
        if (playerNum == 1)
        {
            myScoreText.text = $"{p1_score} / 3";
            enemyScoreText.text = $"{p2_score} / 3";
        }
        else
        {
            myScoreText.text = $"{p2_score} / 3";
            enemyScoreText.text = $"{p1_score} / 3";
        }
    }
}
