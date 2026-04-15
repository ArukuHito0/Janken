using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField]
    private TextMeshProUGUI jankenText;
    [SerializeField]
    private TextMeshProUGUI resultText;
    [SerializeField]
    private TextMeshProUGUI countDownText;

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
        if(playerSelectedHand.TryGetComponent<UnityEngine.UI.Button>(out var button))
        {
            button.interactable = false;
        }
    }

    // じゃんけん
    public IEnumerator Battle(GameResponse response, int playerNum)
    {
        jankenText.text = "jan";
        yield return new WaitForSeconds(1f);
        jankenText.text = "ken";
        yield return new WaitForSeconds(1f);
        jankenText.text = "pon!!";

        enemySelectedHand.SetHand(playerNum == 1 ? response.p2_select :  response.p1_select);
    }

    // 盤面をリフレッシュ
    public void Refresh(GameResponse response, int playerNum)
    {
        jankenText.text = "vs";
        resultText.text = "janken";

        playerSelectedHand.ResetHand();
        enemySelectedHand.ResetHand();

        var hand = playerNum == 1 ? response.p1_hand : response.p2_hand;

        onCardsChanged?.Invoke(DeckManager.GetHand(hand), response.open_card);
    }
    
    // 勝者表示
    public void Result(GameResponse response, int playerNum)
    {
        if (response.winner != -1)
        {
            if (response.winner == playerNum)
            {
                resultText.text = "YOU WIN !!";
            }
            else
            {
                resultText.text = "YOU LOSE...";
            }
        }
        else
        {
            resultText.text = "DRAW";
        }
    }

    // 次のバトルへのカウントダウン
    public IEnumerator NextBattleCountDown()
    {
        countDownText.enabled = true;
        countDownText.text = "Next JANKEN -> 3 seconds ago";
        yield return new WaitForSeconds(1);
        countDownText.text = "Next JANKEN -> 2 seconds ago";
        yield return new WaitForSeconds(1);
        countDownText.text = "Next JANKEN -> 1 seconds ago";
        yield return new WaitForSeconds(1);
        countDownText.enabled = false;
    }

    #region デバッグ用関数群
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
    #endregion
}