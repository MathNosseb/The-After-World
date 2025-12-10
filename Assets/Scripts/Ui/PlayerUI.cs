using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


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

    [Header("Parametres")]
    public Canvas canvas;

    //notifications
    [HideInInspector]
    public List<Notifications> notifications = new List<Notifications>();


    //variables d instances
    public dataHolder data;

    public Notifications SendNotification(float coordX, float coordY, float time, string text)
    {
        Notifications nouvelNotif = new Notifications(coordY, coordX, time, text);
        nouvelNotif.CreateNotification();
        GameObject notifObjet = nouvelNotif.GetGameObject();
        notifObjet.transform.SetParent(canvas.transform);
        notifications.Add(nouvelNotif);

        if (time > 0)
            StartCoroutine(DestroyNotification(nouvelNotif, time));

        return nouvelNotif;

    }

    public bool isNotificationAlive(Notifications notif)
    {
        if (notifications.Contains(notif))
            return true;
        return false;
    }

    public void DestroyNotificationNow(Notifications notif)
    {
        notifications.Remove(notif);
        Destroy(notif.GetGameObject());
    }

    IEnumerator DestroyNotification(Notifications notif, float time)
    {
        yield return new WaitForSeconds(time);//pause 
        notifications.Remove(notif);
        Destroy(notif.GetGameObject());
    }


    private void Update()
    {
        CelestialBody reference;
        float velocity;
        bool inSpaceShip = data.inSpaceShip;
        if (inSpaceShip)
        {
            velocity = data.spaceShipVelocity;
            reference = data.spaceShipReference;
        }
        else
        {
            velocity = data.playerVelocity;
            reference = data.playerReference;
        }

        Reference.text = reference ? reference.name : "null";

        GameObject groundreference = data.referenceGround;
        GroundReference.text = groundreference ? "land on " + groundreference.name : "not landed";

        float distanceBetweenRef = Vector3.Distance(transform.position, reference ? reference.transform.position : Vector3.zero);
        HeightText.text = (int)distanceBetweenRef + " m";

        
        Velocity.text = (int)velocity + " m/s";


        RelativeVelocity.text = reference ? reference.name + " Speed " + (int)reference.GetComponent<CelestialBody>().currentVelocity.magnitude + " m/s" : "null";

        movementType.text = data.spaceMovement ? "mouvement spatial" : "mouvement planetaire";

        
        FPS.text = (int)data.GetFps(Time.deltaTime) + " FPS";

    }

    
}
