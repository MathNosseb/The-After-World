using System.Collections.Generic;
using UnityEngine;



public class SoundsManager : MonoBehaviour
{
    
    public AudioSource audioSource;
    public List<SoundMaker> soundMakers = new List<SoundMaker>();

    void OnGUI()
    {
        foreach (SoundMaker sound in soundMakers)
        {
            if (sound.play)
            {
                audioSource.clip = sound.clip;
                audioSource.Play();
            }
        }
    }

    void Start()
    {
        audioSource.clip = soundMakers[0].clip;
        audioSource.Play();
    }
}
