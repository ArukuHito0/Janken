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
    private TextMeshProUGUI waitingText;
    [SerializeField]
    private TextMeshProUGUI userIdText;

    private bool isTransitioning = false;
    private Coroutine waitCoroutine;

    private void Start()
    {
        userId = GetOrCreateUserId();

        userIdText.text = $"UserID: {userId}";
    }

    public void OnClickPlay()
    {
        StartCoroutine(Matching());
    }

    public void OnClickRematch()
    {
        StartCoroutine(Rematch());
    }

    public void OnClickLeave()
    {
        StartCoroutine(ReturnToTitle());
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

                if(titleUI != null) titleUI.enabled = false;
                if (matchingUI != null) matchingUI.enabled = true;

                if (waitCoroutine != null)
                {
                    StopCoroutine(waitCoroutine);
                }
                waitCoroutine = StartCoroutine(WaitOtherPlayer());

                Debug.Log($"プレイヤー番号: {playerNum}");
                Debug.Log($"ルームID: {roomId}");
            }
        }
        yield break;
    }

    // ルーム退出
    private IEnumerator LeaveRoom()
    {
        bool success = false;
        while (!success)
        {
            WWWForm form = new WWWForm();
            form.AddField(FormFields.roomId, roomId);
            form.AddField(FormFields.userId, userId);

            using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("disconnect"), form))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    success = true;
                    RoomMatchManager.roomId = -1;
                    yield break;
                }
            }
            yield return null;
        }
        yield break;
    }

    // 再戦を希望
    private IEnumerator Rematch()
    {
        bool success = false;
        while (!success)
        {
            WWWForm form = new WWWForm();
            form.AddField(FormFields.roomId, RoomMatchManager.roomId);
            form.AddField(FormFields.playerNum, RoomMatchManager.playerNum);

            using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("rematch"), form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    success = true;
                    yield break;
                }
            }
            yield return null;
        }        
    }

    // 待機
    private IEnumerator WaitOtherPlayer()
    {
        while (!isTransitioning)
        {
            if (roomId != -1)
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

                                yield return OnPlayerMatched(_response, previousState, nextState);
                            }
                        }
                    }
                }
            }
            yield return null;
        }
    }

    // タイトルにもどる
    private IEnumerator ReturnToTitle()
    {
        yield return LeaveRoom();

        SceneManager.LoadScene("TitleScene");
    }

    // マッチ成功
    private IEnumerator OnPlayerMatched(GameResponse response, GameState previousState, GameState nextState)
    {
        if (previousState == GameState.waiting && nextState != GameState.waiting)
        {
            isTransitioning = true;
            waitingText.text = "Matched!!";
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene("MainScene");
            yield break;
        }
    }

    private string GetOrCreateUserId()
    {
        string saveId = PlayerPrefs.GetString("UserID", "");

        if (string.IsNullOrEmpty(saveId))
        {
            saveId = UnityEngine.Random.Range(1000, 9999).ToString() + 
                "-" + UnityEngine.Random.Range(1000, 9999).ToString() + 
                "-" + UnityEngine.Random.Range(1000, 9999).ToString();

            PlayerPrefs.SetString("UserID", saveId);
            PlayerPrefs.Save();
        }

        return saveId;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}