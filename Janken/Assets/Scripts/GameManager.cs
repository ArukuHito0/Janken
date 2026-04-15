using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Audio.ProcessorInstance;

public enum GameState
{
    waiting,    // プレイヤー待ち
    ready,      // 準備(盤面のリフレッシュ)
    selecting,  // 手を選択中
    battle,     // じゃんけん
    result,     // 結果表示
    end,        // 対戦終了
}

public class GameManager : MonoBehaviour
{
    private GameState currentState;

    [SerializeField]
    private Canvas jankenUI;
    [SerializeField]
    private Canvas matchUI;
    [SerializeField]
    private TextMeshProUGUI myPlayerStatusText;
    [SerializeField]
    private TextMeshProUGUI enemyPlayerStatusText;

    public void OnClickGameLoop()
    {
        StartCoroutine(GameObserverLoop());
    }

    private void Start()
    {
        StartCoroutine(GameObserverLoop());
    }

    private IEnumerator GameObserverLoop()
    {
        while (true)
        {
            WWWForm form = new WWWForm();
            form.AddField(FormFields.roomId, RoomMatchManager.roomId);
            form.AddField(FormFields.playerNum, RoomMatchManager.playerNum);

            // game_statusを取得し、ステートに応じて処理を行う
            // ※PHP側でゲームの進行処理は行うので、Unity側では表示や演出などの処理のみを行う
            using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("game_status_observer"), form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    //Debug.Log("受信したデータ: " + www.downloadHandler.text); // これを追加！
                    GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);

                    // 各プレイヤーが手をセットしたかどうかのテキストUIを更新
                    SetPlayersStatusText(response, RoomMatchManager.playerNum);

                    if (Enum.TryParse(response.game_status, true, out GameState nextState))
                    {
                        if (currentState != nextState)
                        {
                            Debug.Log(response.game_status);

                            GameState previousState = currentState;
                            currentState = nextState;

                           yield return OnStateChanged(response, previousState, nextState);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator OnStateChanged(GameResponse response,GameState previousState, GameState nextState)
    {
        switch (nextState)
        {
            case GameState.ready:
                ScoreManager.Instance.SetScoreText(RoomMatchManager.playerNum, response.p1_score, response.p2_score);

                break;
            case GameState.selecting:
                PlayerHand.Instance.EnableCards();
                BattleManager.Instance.Refresh(response, RoomMatchManager.playerNum);
                DeckManager.Instance.Deal(response, RoomMatchManager.playerNum);

                break;
            case GameState.battle:
                PlayerHand.Instance.DisableCards();
                BattleManager.Instance.LockSetButton();

                yield return BattleManager.Instance.Battle(response, RoomMatchManager.playerNum);

                break;
            case GameState.result:
                ScoreManager.Instance.SetScoreText(RoomMatchManager.playerNum, response.p1_score, response.p2_score);
                BattleManager.Instance.Result(response, RoomMatchManager.playerNum);

                if (response.p1_score < 3 && response.p2_score < 3)
                    yield return BattleManager.Instance.NextBattleCountDown();
                else
                    yield return new WaitForSeconds(1.0f);

                break;
            case GameState.end:
                PlayerHand.Instance.DisableCards();
                BattleManager.Instance.LockSetButton();
                BattleManager.Instance.Battle(response, RoomMatchManager.playerNum);
                ScoreManager.Instance.SetScoreText(RoomMatchManager.playerNum, response.p1_score, response.p2_score);

                jankenUI.GetComponent<Animator>().SetTrigger("EndJanken");
                yield return new WaitForSeconds(0.75f);
                matchUI.GetComponent<Animator>().SetTrigger("SlideUp");

                break;
            default:
                break;
        }
    }

    private void SetPlayersStatusText(GameResponse response, int playerNum)
    {
        // 自分のプレイヤー番号と同じ数字の選択した手を格納しているカラムに応じて状態の表示を変更
        if (playerNum == 1)
        {
            if (response.p1_select != 4)
                myPlayerStatusText.text = "set";
            else
                myPlayerStatusText.text = "think";
            if (response.p2_select != 4)
                enemyPlayerStatusText.text = "set";
            else
                enemyPlayerStatusText.text = "think";
        }
        else
        {
            if (response.p2_select != 4)
                myPlayerStatusText.text = "set";
            else
                myPlayerStatusText.text = "think";
            if (response.p1_select != 4)
                enemyPlayerStatusText.text = "set";
            else
                enemyPlayerStatusText.text = "think";
        }
    }
}