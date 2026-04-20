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

    public static int playerNum { get; private set; } = -1;
    public static int roomId { get; private set; } = -1;

    private static string userId;

    [SerializeField]
    private Canvas titleUI;
    [SerializeField]
    private Canvas matchingUI;
    [SerializeField]
    private TextMeshProUGUI playerNumText;
    [SerializeField]
    private TextMeshProUGUI waitingText;
    [SerializeField]
    private TextMeshProUGUI userIdText;

    public void OnClickPlay()
    {
        StartCoroutine(Matching());
    }

    public void OnClickRemain()
    {
        StartCoroutine(Remain());
    }

    public void OnClickRematch()
    {
        StartCoroutine(Rematch());
    }

    public void OnClickLeave()
    {
        StartCoroutine(LeaveRoom());
    }

    public void OnSetUserId(string userId)
    {
        RoomMatchManager.userId = userId;
    }

    // ルームに参加もしくはルーム作成
    private IEnumerator Matching()
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.userId, userId);

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

                StartCoroutine(WaitOtherPlayer());

                Debug.Log($"プレイヤー番号: {playerNum}");
                Debug.Log($"ルームID: {roomId}");
            }
        }
        yield break;
    }

    // ルーム退出
    private IEnumerator LeaveRoom()
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, roomId);
        form.AddField(FormFields.userId, userId);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("disconnect"), form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                SceneManager.LoadScene("TitleScene");
            }
            else
            {
                StartCoroutine(LeaveRoom());
            }
        }

        yield break;
    }

    // 別のルームを探す
    private IEnumerator Remain()
    {
        yield return LeaveRoom();
        StartCoroutine(WaitOtherPlayer());
        yield break;
    }

    // 再戦を希望
    private IEnumerator Rematch()
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, RoomMatchManager.roomId);
        form.AddField(FormFields.playerNum, RoomMatchManager.playerNum);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("rematch"), form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                StartCoroutine(Rematch());
            }
        }
    }

    // 待機
    private IEnumerator WaitOtherPlayer()
    {
        while (true)
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

    private IEnumerator OnStateChanged(GameResponse response, GameState previousState, GameState nextState)
    {
        if (nextState == GameState.waiting)
        {
            playerNumText.text = $"Room ID is {roomId}\nYour Player Number is {playerNum} !";
        }
        else
        {
            waitingText.text = "Matched!!";
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene("MainScene");
        }
    }

    private string GetOrCreateUserId()
    {
        string saveId = PlayerPrefs.GetString("SaveUserId", "");

        if (string.IsNullOrEmpty(saveId))
        {
            saveId = Guid.NewGuid().ToString();

            PlayerPrefs.SetString("SaveUserId", saveId);
            PlayerPrefs.Save();
        }
                
        return saveId;
    }

}