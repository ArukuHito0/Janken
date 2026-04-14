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

    [SerializeField]
    private Canvas titleUI;
    [SerializeField]
    private Canvas matchingUI;
    [SerializeField]
    private TextMeshProUGUI playerNumText;

    public static int playerNum;
    public static int roomId;

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
        WWWForm form = new WWWForm();
        form.AddField(FormFields.userId, "匿名ユーザー");

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

                playerNumText.text = $"Room ID is {roomId}\nYour Player Number is {playerNum} !";
                Debug.Log($"プレイヤー番号: {playerNum}");
                Debug.Log($"ルームID: {roomId}");
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }

    private void OnDestroy()
    {
        
    }
}