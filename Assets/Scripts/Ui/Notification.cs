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

    public GameObject GetGameObject()
    {
        return this.notif;
    }

    public void CreateNotification()
    {
        notif = new GameObject("notification");
        cr = notif.AddComponent<CanvasRenderer>();
        txt = notif.AddComponent<Text>();
        rt = notif.GetComponent<RectTransform>();

        txt.text = this.text;
        txt.fontSize = 30;
        rt.sizeDelta = new Vector2(500f, 50f);
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rt.anchoredPosition = new Vector3(this.coorX, this.coordY);
    }
}