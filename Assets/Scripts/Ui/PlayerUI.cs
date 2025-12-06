using System.Collections;
using System.Collections.Generic;
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
    FirstPersonController firstPersonController;
    Rigidbody rb;

    Vector3 lastPosition;
    float speed;
    float fps;

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


    private void Start()
    {
        firstPersonController = GetComponent<FirstPersonController>(); 
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
    }

    float GetVelocity(Vector3 lastPosition, Vector3 actualPosition)
    {
        float distance = Vector3.Distance(actualPosition, lastPosition);

        // Vitesse = distance / temps écoulé
        float rawSpeed = distance / Time.fixedDeltaTime; 
 
        // Lissage
        speed = Mathf.Lerp(speed, rawSpeed, .5f);

        return speed;
        //return GetComponent<Rigidbody>().linearVelocity.magnitude;
    }


    float GetFps(float deltaTime)
    {
        float rawFps = 1 / deltaTime;
        fps = Mathf.Lerp(fps, rawFps, .1f);

        return fps;
    }

    private void FixedUpdate()
    {
        Velocity.text = (int)GetVelocity(lastPosition, transform.position) + " m/s";

    }


    private void Update()
    {
        CelestialBody reference = firstPersonController.reference;
        Reference.text = reference ? reference.name : "null";

        GameObject groundreference = firstPersonController.referenceGround;
        GroundReference.text = groundreference ? "land on " + groundreference.name : "not landed";

        float distanceBetweenRef = Vector3.Distance(transform.position, reference ? reference.transform.position : Vector3.zero);
        HeightText.text = (int)distanceBetweenRef + " m";


        RelativeVelocity.text = reference ? reference.name + " Speed " + (int)reference.GetComponent<CelestialBody>().currentVelocity.magnitude + " m/s" : "null";

        movementType.text = firstPersonController.SpaceMovement ? "mouvement spatial" : "mouvement planetaire";

        
        FPS.text = (int)GetFps(Time.deltaTime) + " FPS";

        lastPosition = transform.position;

    }

    
}
