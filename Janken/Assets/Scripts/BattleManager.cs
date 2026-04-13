using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BattleManager : MonoBehaviour
{
    [SerializeField]
    private ScoreManager scoreManager;
    [SerializeField]
    private HandSetButton playerSelectedHand;
    [SerializeField]
    private HandSetButton enemySelectedHand;

    public event Action<int[], int> onCardsChanged;
    public event Action<int, int> onScoreChanged;

    public void OnClickBattle()
    {
        StartCoroutine(Battle());
    }

    public void OnClickRefresh()
    {
        StartCoroutine(Refresh());
    }

    public void OnClickResetRoom()
    {
        StartCoroutine(ResetRoom());
    }

    IEnumerator Battle()
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, 1);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("battle"), form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);

                enemySelectedHand.SetHand(response.p2_select);  // デバッグ用にplayer2の手を表示
                                                        // ※本番では対戦相手の手を表示する

                Debug.Log($"player 1の手: {(JankenCard.Hand)response.p1_select} / player 2の手: {(JankenCard.Hand)response.p2_select}");
                if(response.winner == 0)
                    Debug.Log($"あいこ");
                else
                    Debug.Log($"勝者: player {response.winner}");

                onScoreChanged?.Invoke(response.p1_score, response.p2_score);
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }

    IEnumerator Refresh()
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, 1);
        form.AddField(FormFields.playerNum, 1);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("refresh"), form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);

                playerSelectedHand.ResetHand();
                enemySelectedHand.ResetHand();

                onCardsChanged?.Invoke(DeckManager.GetHand(response.p1_hand), response.open_card);
                onScoreChanged?.Invoke(response.p1_score, response.p2_score);
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }

    IEnumerator ResetRoom()
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, 1);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("reset"), form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);

                playerSelectedHand.ResetHand();
                enemySelectedHand.ResetHand();

                onScoreChanged?.Invoke(response.p1_score, response.p2_score);
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }
}