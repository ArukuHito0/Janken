using System;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField]
    private DeckManager deckManager;

    [SerializeField]
    private JankenCard[] myCards;

    [SerializeField]
    private JankenCard openCard;

    public void OnEnable()
    {
        deckManager.OnCardsChanged += UpdateCards;
    }

    public void OnDisable()
    {
        deckManager.OnCardsChanged -= UpdateCards;
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
