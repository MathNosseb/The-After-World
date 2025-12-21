using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


[RequireComponent(typeof(PlayerContainer))]
public class PlayerUI : MonoBehaviour
{
    [Header("infos")]//tous les parametres debug lié à la simulation
    public Text HeightText;
    public Text Reference;
    public Text GroundReference;
    public Text Velocity;
    public Text RelativeVelocity; //speed on the reference object
    public Text movementType;
    public Text FPS;

    [Header("Canvas")]
    public Canvas canvas;

    [Header("References")]
    PlayerContainer playerContainer;


    private void Awake()
    {
        playerContainer = GetComponent<PlayerContainer>();
    }

    public Notifications SendNotification(float coordX, float coordY, float time, string text)
    {
        Notifications nouvelNotif = new Notifications(coordY, coordX, time, text);
        nouvelNotif.CreateNotification();
        GameObject notifObjet = nouvelNotif.GetGameObject();
        notifObjet.transform.SetParent(canvas.transform);
        playerContainer.notifications.Add(nouvelNotif);

        if (time > 0)
            StartCoroutine(DestroyNotification(nouvelNotif, time));

        return nouvelNotif;

    }

    public bool isNotificationAlive(Notifications notif)
    {
        if (playerContainer.notifications.Contains(notif))
            return true;
        return false;
    }

    public void DestroyNotificationNow(Notifications notif)
    {
        playerContainer.notifications.Remove(notif);
        Destroy(notif.GetGameObject());
    }

    IEnumerator DestroyNotification(Notifications notif, float time)
    {
        yield return new WaitForSeconds(time);//pause 
        playerContainer.notifications.Remove(notif);
        Destroy(notif.GetGameObject());
    }


    private void Update()
    {
        CelestialBody reference = playerContainer.reference;
        float velocity = playerContainer.PlayerRB.linearVelocity.magnitude;

        Reference.text = reference ? reference.name : "null";

        GameObject groundreference = playerContainer.groundRefGameObject;
        GroundReference.text = groundreference ? "land on " + groundreference.name : "not landed";

        float distanceBetweenRef = Vector3.Distance(transform.position, reference ? reference.transform.position : Vector3.zero);
        HeightText.text = (int)distanceBetweenRef + " m";

        
        Velocity.text = (int)velocity + " m/s";


        RelativeVelocity.text = reference ? reference.name + " Speed " + (int)reference.GetComponent<CelestialBody>().currentVelocity.magnitude + " m/s" : "null";

        movementType.text = playerContainer.influenceByBody ? "mouvement spatial" : "mouvement planetaire";

        
        FPS.text = (int)playerContainer.GetFps(Time.deltaTime) + " FPS";

    }

    
}
