using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class JankenCard : MonoBehaviour
{
    public enum Hand
    {
        グー,
        チョキ,
        パー
    }

    private Image handIcon;

    [SerializeField]
    private Sprite[] handSprites;

    [SerializeField]
    private Hand hand;

    private void Awake()
    {
        handIcon = transform.Find("Icon").GetComponent<Image>();
    }

    public void SetHand(int hand)
    {
        this.hand = (Hand)hand;

        handIcon.sprite = handSprites[hand];
    }

    public void OnClickHand()
    {
        StartCoroutine(SelectMyHand(1, 1));
    }

    IEnumerator SelectMyHand(int roomId, int playerNum)
    {
        WWWForm form = new WWWForm();
        form.AddField(FormFields.roomId, roomId);
        form.AddField(FormFields.playerNum, playerNum);
        form.AddField(FormFields.selectedHand, (int)hand);

        using (UnityWebRequest www = UnityWebRequest.Post(FormFields.GetFormURL("select_card"), form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                GameResponse response = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);
                Debug.Log("あなたが出す手は [" + response.p1_select + "] です");
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }
}
