using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class DeckManager : MonoBehaviour
{
    public event Action<int[], int> onCardsChanged;

    public void OnClickDeck()
    {
        StartCoroutine(Deck(1,1));
    }

    private IEnumerator Deck(int roomId, int playerNum)
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, roomId);
        form.AddField(FormFields.playerNum, playerNum);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("deck"), form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("PHPからのレスポンス: " + www.downloadHandler.text);
                GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);
                Debug.Log($"player 1の手札: {response.p1_hand} player2の手札: {response.p2_hand} 公開カード: {response.open_card}");

                onCardsChanged?.Invoke(playerNum == 1 ? GetHand(response.p1_hand) : GetHand(response.p2_hand), response.open_card);
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }

    public static int[] GetHand(string handstr)
    {
        string[] handStrings = handstr.Split(',');

        int[] result = new int[handStrings.Length];
        for (int i = 0; i < handStrings.Length; i++)
        {
            result[i] = int.Parse(handStrings[i]);
        }

        return result;
    }
}
