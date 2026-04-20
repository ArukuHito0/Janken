using UnityEngine;

[System.Serializable]
public class GameResponse
{
    public string game_status;
    public int winner;
    public int open_card;
    public string p1_id;
    public string p2_id;
    public bool p1_connect;
    public bool p2_connect;
    public string p1_hand;
    public string p2_hand;
    public int p1_select;
    public int p2_select;
    public int p1_score;
    public int p2_score;
    public string p1_status;
    public string p2_status;
}
