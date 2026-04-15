using UnityEngine;

[System.Serializable]
public static class FormFields
{
    public static string GetFormURL(string fileName)
    {
        return "http://10.219.32.73/Janken-Backend/" + fileName + ".php";
    }

    public static readonly string roomId = "room_id";
    public static readonly string userId = "user_id";
    public static readonly string playerNum = "player_num";
    public static readonly string selectedHand = "selected_hand";
    public static readonly string playerReady = "player_ready";
}
