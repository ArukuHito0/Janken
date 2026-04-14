using UnityEngine;

[System.Serializable]
public class GameResponse
{
    public string p1_id;
    public string p2_id;
    public int open_card;
    public string p1_hand;
    public string p2_hand;
    public int p1_select;
    public int p2_select;
    public int p1_score;
    public int p2_score;
    public bool p1_ready;
    public bool p2_ready;
    public int winner;
    public string game_status;
}
