using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

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

    public void OnClickGameLoop()
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

            using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("game_status_observer"), form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    //Debug.Log("受信したデータ: " + www.downloadHandler.text); // これを追加！
                    GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);

                    if(Enum.TryParse(response.game_status, true, out GameState nextState))
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
            case GameState.waiting:
                break;
            case GameState.ready:
                PlayerHand.Instance.EnableCards();
                ScoreManager.Instance.SetScoreText(response.p1_score, response.p2_score);
                BattleManager.Instance.Refresh(response, RoomMatchManager.playerNum);
                break;
            case GameState.selecting:
                DeckManager.Instance.Deal(response, RoomMatchManager.playerNum);
                break;
            case GameState.battle:
                yield return new WaitForSeconds(2.0f);

                PlayerHand.Instance.DisableCards();
                BattleManager.Instance.LockSetButton();
                BattleManager.Instance.Battle(response);
                break;
            case GameState.result:
                yield return new WaitForSeconds(2.0f);
                ScoreManager.Instance.SetScoreText(response.p1_score, response.p2_score);
                break;
            case GameState.end:
                break;
            default:
                break;
        }
    }
}