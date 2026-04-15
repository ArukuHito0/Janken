using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class RoomMatchManager : MonoBehaviour
{
    // JSON形式のデータを受け取る為のクラス
    private class MatchingResponse
    {
        public int player_num;
        public int room_id;
    }

    private GameState currentState;

    public static int playerNum = -1;
    public static int roomId = -1;

    [SerializeField]
    private Canvas titleUI;
    [SerializeField]
    private Canvas matchingUI;
    [SerializeField]
    private TextMeshProUGUI playerNumText;
    [SerializeField]
    private TextMeshProUGUI waitingText;

    public void OnClickPlay()
    {
        StartCoroutine(Matching());
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private IEnumerator Matching()
    {
        Debug.Log(roomId);

        while (true)
        {
            if (roomId == -1)
            {
                WWWForm form = new WWWForm();
                form.AddField(FormFields.userId, "匿名ユーザー");

                // マッチングのAPIを叩く
                using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("matching"), form))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("受信したデータ: " + www.downloadHandler.text); // これを追加！
                        MatchingResponse response = JsonUtility.FromJson<MatchingResponse>(www.downloadHandler.text);
                        roomId = response.room_id;
                        playerNum = response.player_num;

                        titleUI.enabled = false;
                        matchingUI.enabled = true;

                        Debug.Log($"プレイヤー番号: {playerNum}");
                        Debug.Log($"ルームID: {roomId}");
                    }
                }
            }
            else
            {
                // game_status_observerに送信する為のフォーム
                WWWForm form = new WWWForm();
                form.AddField(FormFields.roomId, roomId);
                form.AddField(FormFields.playerNum, playerNum);

                // ゲームの状態を監視するAPIを叩く
                using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("game_status_observer"), form))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        GameResponse _response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);

                        if (Enum.TryParse(_response.game_status, true, out GameState nextState))
                        {
                            if (currentState != nextState)
                            {
                                GameState previousState = currentState;
                                currentState = nextState;

                                yield return OnStateChanged(_response, previousState, nextState);
                            }
                        }
                    }
                }
            }
        }        
    }

    private IEnumerator OnStateChanged(GameResponse response, GameState previousState, GameState nextState)
    {
        if (nextState == GameState.waiting)
        {
            playerNumText.text = $"Room ID is {roomId}\nYour Player Number is {playerNum} !";
        }
        else
        {
            waitingText.text = "Matched!!";
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("MainScene");
        }
    }
}