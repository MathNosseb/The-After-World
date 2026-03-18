using System;
using UnityEngine;

public enum BodyType { Planet, Sun, Moon, Object };

[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : MonoBehaviour
{
    [Header("Personalisation")]
    public string Name;
    public BodyType BodyType;

    [Header("Références")]
    GameObject sun;
    CelestialBody sunCelestial;
    
    private Rigidbody rb;
    constant constantValue;

    [Header("Paramètres liés à la physique")]
    public float surfaceGravity;
    public float diametre;
    public float density;
    public bool fix = false;
    public float distanceBeforeRotation;
    public float jitteringStrength;
    [HideInInspector] public float mass;
    

    [Header("Paramètres liés au mouvement")]
    public DoubleVector3 currentVelocity;
    public DoubleVector3 currentPosition;
    [HideInInspector] public DoubleVector3 startPosition;
    public DoubleVector3 initialVelocity;
    
    
    [Header("Atmosphère")]
    public bool useAtmosphere = false;
    public AtmosphereGenerator planetAtmosphere;

    
    [Header("Debug")]
    public Color colorPath;

    

    

    #if UNITY_EDITOR

    private void OnValidate()
    {
        constantValue = GameObject.Find("Universe").GetComponent<constant>();
        sun = GameObject.Find("Sun");

        //la masse de la planete est calculé au lancement du jeu
        mass = surfaceGravity * (diametre/2) * (diametre/2) / constantValue.GravityConstant;
        Debug.Log(Name + " " + mass + "kg");
    }
    #endif


    [ContextMenu("Mettre � jour la position")]
    public void AtmosphereUpdatePosition()
    {
        if (useAtmosphere) { planetAtmosphere.planetCentre = transform.position; }
    }


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        constantValue = GameObject.Find("Universe").GetComponent<constant>();
        sun = GameObject.Find("Sun");
        
        sunCelestial = sun.GetComponent<CelestialBody>();
        if (sun == null) {Debug.LogError("Le soleil est introuvable, assurez vous d avoir un gameObject sun");}
        if (sunCelestial == null) {Debug.LogError("Le Celestial soleil est introuvable, assurez vous d avoir un CelestialBody");}

        //la masse de la planete est calculé au lancement du jeu
        mass = surfaceGravity * (diametre/2) * (diametre/2) / constantValue.GravityConstant;

        //on initialise la vitesse de départ pour "lancer" la planete
        currentVelocity = initialVelocity;
        //on initialise la position de départ
        startPosition = new DoubleVector3(
            (double)transform.position.x,
            (double)transform.position.y,
            (double)transform.position.z
        );
        
        currentPosition = startPosition;
    }

    private void FixedUpdate()
    {


        //si la position de la planete doit etre fixe (on ignore la physique)
        if (fix) { 
            transform.position = new Vector3(
                (float)startPosition.x,
                (float)startPosition.y,
                (float)startPosition.z
            );
        }
        else
        {
            //sinon on modifie la position de la planete suivant la veritable position
            rb.position = GetVector3Position();
        }

        if (useAtmosphere) { planetAtmosphere.planetCentre = rb.position; }

        
       
    }

    private void Update()
    { 
        //modifie la light
        if (useAtmosphere) { planetAtmosphere.lightDir = (sunCelestial.GetDoubleVector3Position() - currentPosition).normalized.convert; }
    }

    public void UpdateVelocity(CelestialBody[] allBodies, float timeStep)
    {
        if (fix) { return; }
        foreach (var otherBody in allBodies)
        {
            //si le corp est lui même ou si c'est un corp que l on souhaite ignorer on applique pas sa gravité
            if (otherBody == this) continue;

            DoubleVector3 direction = otherBody.GetDoubleVector3Position() - currentPosition;
            double distanceSqr = direction.sqrMagnitude;
            //on evite la division par 0
            if (distanceSqr < 1e-6f) continue;

            DoubleVector3 forceDir = direction.normalized;
            DoubleVector3 acceleration = forceDir * (constantValue.GravityConstant * otherBody.mass / distanceSqr);
            currentVelocity += acceleration * timeStep;
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Sphère d'influence (zone où le joueur est attiré)
        Gizmos.color = new Color(colorPath.r, colorPath.g, colorPath.b, 0.15f);
        Gizmos.DrawSphere(transform.position, distanceBeforeRotation);

        // Contour de la sphère d'influence
        Gizmos.color = colorPath;
        Gizmos.DrawWireSphere(transform.position, distanceBeforeRotation);

        // Rayon de la planète
        Gizmos.color = new Color(1f, 1f, 1f, 0.05f);
        Gizmos.DrawSphere(transform.position, diametre/2);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, diametre/2);
    }
    #endif

    public void UpdatePosition(float timeStep)
    {
        if (fix) { return; }
        currentPosition += currentVelocity * timeStep;
        
    }

    public void ChangePosition(DoubleVector3 newPosition)
    {
        startPosition = newPosition;
        currentPosition = newPosition;
    }

    public Vector3 GetVector3Position()
    {
        return currentPosition.convert;
    }

    public DoubleVector3 GetDoubleVector3Position()
    {
        return currentPosition;
    }

    public DoubleVector3 GetDoubleVector3Velocity()
    {
        return currentVelocity;
    }

    [System.Serializable]
    public struct DoubleVector3
    {
        public double x;
        public double y;
        public double z;

        public DoubleVector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static DoubleVector3 operator +(DoubleVector3 a, DoubleVector3 b)
        {
            return new DoubleVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static DoubleVector3 operator -(DoubleVector3 a, DoubleVector3 b)
        {
            return new DoubleVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public double sqrMagnitude => x * x + y * y + z * z;
        
        public double magnitude => Math.Sqrt(x * x + y * y + z * z);

        public DoubleVector3 negative => new DoubleVector3(-x, -y, -z);

        public static DoubleVector3 zero => new DoubleVector3(0, 0, 0);

        public DoubleVector3 normalized
        {
            get
            {
                double mag = magnitude;       // utilise ta propriété magnitude
                if (mag > 1e-9)               // éviter la division par zéro
                    return new DoubleVector3(x / mag, y / mag, z / mag);
                else
                    return new DoubleVector3(0, 0, 0); // vecteur nul
            }
        }

        public Vector3 convert
        {
            get
            {
                return new Vector3((float)x, (float)y, (float)z);
            }
        }

        public static DoubleVector3 operator *(DoubleVector3 v, double scalar)
        {
            return new DoubleVector3(v.x * scalar, v.y * scalar, v.z * scalar);
        }

        public static DoubleVector3 operator /(DoubleVector3 v, double scalar)
        {
            return new DoubleVector3(v.x / scalar, v.y / scalar, v.z / scalar);
        }

    }

    
}
