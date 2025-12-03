using UnityEngine;
using UnityEngine.UI;

public class Notifications
{
    float coordY;
    float coorX;
    float duration;
    float startTime;
    string text;

    GameObject notif;
    RectTransform rt;
    CanvasRenderer cr;
    Text txt;

    public Notifications(float coordY, float coorX, float duration, string text)
    {
        this.coordY = coordY;
        this.coorX = coorX;
        this.duration = duration;
        this.text = text;
    }

    public void CreateNotification()
    {
        notif = new GameObject("notification");
        cr = notif.AddComponent<CanvasRenderer>();
        txt = notif.AddComponent<Text>();
        rt = notif.GetComponent<RectTransform>();

        txt.text = this.text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rt.anchoredPosition = new Vector3(this.coorX, this.coordY);
    }
}