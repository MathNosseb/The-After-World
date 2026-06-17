using UnityEngine;

public class PlanetSounds : MonoBehaviour
{
    SoundsManager soundsManager;
    public float[] radiusBeforePlaying;
    public Color[] colors;

    Transform playerPosition;

    void Start()
    {
        soundsManager = GetComponent<SoundsManager>();  
        playerPosition = Camera.main.transform;
    }

    void Update()
    {
        if (soundsManager.sounds.Count != radiusBeforePlaying.Length || soundsManager.sounds.Count != colors.Length)
        {
            Debug.Log("[INFORMATION] Les listes ne sont pas équivalentes sur les sons des plantes");
            return;
        }

        float dst = Vector3.Distance(transform.position, playerPosition.position);
        for (int i = 0; i < soundsManager.sounds.Count; i++)
        {
            if (dst <= radiusBeforePlaying[i])
                soundsManager.sounds[i].play = true;
            else
                soundsManager.sounds[i].play = false;
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < radiusBeforePlaying.Length; i++)
        {
            Gizmos.color = new Color(colors[i].r, colors[i].g, colors[i].b, .15f);
            Gizmos.DrawSphere(transform.position, radiusBeforePlaying[i]);
        }
    }
    #endif
}
