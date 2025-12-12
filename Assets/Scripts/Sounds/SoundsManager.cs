using System.Collections.Generic;
using UnityEngine;



public class SoundsManager : MonoBehaviour
{
    public List<SoundMaker> sounds = new List<SoundMaker>();

    private void Update()
    {
        foreach (SoundMaker sound in sounds)
        {
            if (sound.play == true)
            {
                playSound(sound);
            }else
                stopSound(sound);   
        }
    }

    void playSound(SoundMaker sound)
    {
        //vérifier si il est pas déjà entrain d'être joué
        AudioSource source = sound.source;
        if (source.isPlaying)
            return;

        source.clip = sound.clip;
        source.Play();
    }

    void stopSound(SoundMaker sound)
    {
        AudioSource source = sound.source;
        if (!source.isPlaying)
            return;
        source.Stop();
    }


}
