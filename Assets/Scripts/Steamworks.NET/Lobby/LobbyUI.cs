using UnityEngine;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI instance;

    public TMP_Text playersInZoneText;  // np. "0/2 w strefie"
    public TMP_Text countdownText;      // np. "Start za: 5"
    public TMP_Text infoText; // info o klasie / braku klasy

    void Awake() => instance = this;

    public static void ShowText(string msg)
    {
        if (instance && instance.infoText) instance.infoText.text = msg;
    }

    public void UpdateHud(int inside, int total, int sec)
    {
        if (playersInZoneText)
            playersInZoneText.text = $"{inside}/{total} w strefie";

        if (countdownText)
        {
            bool show = sec > 0;
            countdownText.gameObject.SetActive(show);
            if (show) countdownText.text = $"Start za: {sec}";
        }
    }
}
