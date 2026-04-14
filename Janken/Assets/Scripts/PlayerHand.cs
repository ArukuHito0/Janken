using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHand : MonoBehaviour
{
    public static PlayerHand Instance { get; private set; }

    [SerializeField]
    private DeckManager deckManager;
    [SerializeField]
    private BattleManager battleManager;

    [SerializeField]
    private JankenCard[] myCards;

    [SerializeField]
    private JankenCard openCard;

    public void OnEnable()
    {
        Instance = this;

        deckManager.onCardsChanged += UpdateCards;
        battleManager.onCardsChanged += UpdateCards;

    }

    public void OnDisable()
    {
        Instance = null;

        deckManager.onCardsChanged -= UpdateCards;
        battleManager.onCardsChanged -= UpdateCards;
    }

    public void EnableCards()
    {
        for (int i = 0; i < myCards.Length; i++)
        {
            myCards[i].GetComponent<Button>().enabled = true;
        }
    }

    public void DisableCards()
    {
        for (int i = 0; i < myCards.Length; i++)
        {
            myCards[i].GetComponent<Button>().enabled = false;
        }
    }

    private void UpdateCards(int[] hand, int openCard)
    {
        for (int i = 0; i < hand.Length; i++)
        {
            myCards[i].SetHand(hand[i]);
        }

        this.openCard.SetHand(openCard);
    }
}
