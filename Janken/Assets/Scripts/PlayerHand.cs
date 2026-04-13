using System;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
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
        deckManager.onCardsChanged += UpdateCards;
        battleManager.onCardsChanged += UpdateCards;

    }

    public void OnDisable()
    {
        deckManager.onCardsChanged -= UpdateCards;
        battleManager.onCardsChanged -= UpdateCards;
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
