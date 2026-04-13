using UnityEngine;
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
    private HandSetButton setButton;
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

    public void OnClickSelect()
    {
        setButton.SetHand((int)hand);
    }
}
