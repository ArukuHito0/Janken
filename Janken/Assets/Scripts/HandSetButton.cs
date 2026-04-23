using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class HandSetButton : MonoBehaviour
{
    public enum Hand
    {
        グー,
        チョキ,
        パー,
        None,
    }

    private Button setButton;
    private Image handIcon;
    private Image miscIcon;

    [SerializeField]
    private Sprite[] handSprites;
    [SerializeField]
    private Hand hand = Hand.None;

    private bool isSet = false;

    private void Awake()
    {
        handIcon = transform.Find("Icon").GetComponent<Image>();
        miscIcon = transform.Find("MiscIcon").GetComponent<Image>();
        setButton = GetComponent<Button>();
    }

    private void Start()
    {
        hand = Hand.None;

        isSet = false;

        miscIcon.enabled = true;
        handIcon.enabled = false;

        if(setButton != null)
            setButton.interactable = false;
    }

    public void SetHand(int hand)
    {
        this.hand = (Hand)hand;
        handIcon.sprite = handSprites[hand];

        isSet = true;

        miscIcon.enabled = false;
        handIcon.enabled = true;

        if (setButton != null)
            setButton.interactable = true;
    }

    public void ResetHand()
    {
        this.hand = Hand.None;

        isSet = false;

        miscIcon.enabled = true;
        handIcon.enabled = false;

        if (setButton != null)
            setButton.interactable = false;
    }

    public void OnClickSet()
    {
        if (!isSet) return;

        setButton.interactable = false;
        StartCoroutine(SetMyHand(RoomMatchManager.roomId, RoomMatchManager.playerNum));
    }

    private IEnumerator SetMyHand(int roomId, int playerNum)
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
                PlayerHand.Instance.DisableCards();
            }
            else
            {
                Debug.LogError("通信に失敗しました。" + www.error);
            }
        }
    }
}
