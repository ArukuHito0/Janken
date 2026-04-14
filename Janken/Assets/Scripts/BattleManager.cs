using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Audio.ProcessorInstance;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [SerializeField]
    private ScoreManager scoreManager;
    [SerializeField]
    private HandSetButton playerSelectedHand;
    [SerializeField]
    private HandSetButton enemySelectedHand;

    public event Action<int[], int> onCardsChanged;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void LockSetButton()
    {
        playerSelectedHand.GetComponent<Button>().enabled = false;
    }

    // じゃんけん
    public void Battle(GameResponse response)
    {
        enemySelectedHand.SetHand(response.p2_select);  // デバッグ用にplayer2の手を表示
                                                        // ※本番では対戦相手の手を表示する
    }

    // 盤面をリフレッシュ
    public void Refresh(GameResponse response, int playerNum)
    {
        playerSelectedHand.ResetHand();
        enemySelectedHand.ResetHand();

        var hand = playerNum == 1 ? response.p1_hand : response.p2_hand;

        onCardsChanged?.Invoke(DeckManager.GetHand(hand), response.open_card);
    }

    /// <summary>
    /// デバッグ用関数
    /// </summary>
    public void OnClickBattle()
    {
        StartCoroutine(CallBattleAPI(1));
    }

    public void OnClickRefresh()
    {
        StartCoroutine(CallRefleshAPI(1,1));
    }

    public void OnClickResetRoom()
    {
        StartCoroutine(CallResetAPI(1));
    }

    /// <summary>
    /// phpファイルを叩くコルーチン
    /// </summary>
    IEnumerator CallBattleAPI(int roomId)
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, roomId);

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
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }

    IEnumerator CallRefleshAPI(int roomId, int playerNum)
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, roomId);
        form.AddField(FormFields.playerNum, playerNum);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("refresh"), form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);

                playerSelectedHand.ResetHand();
                enemySelectedHand.ResetHand();

                onCardsChanged?.Invoke(DeckManager.GetHand(response.p1_hand), response.open_card);
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }

    IEnumerator CallResetAPI(int roomId)
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, roomId);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("reset"), form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);

                playerSelectedHand.ResetHand();
                enemySelectedHand.ResetHand();

                onCardsChanged?.Invoke(DeckManager.GetHand(response.p1_hand), response.open_card);
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }
}