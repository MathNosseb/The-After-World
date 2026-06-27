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

            if (sound.playOneTime == true)
            {
                playOneTime(sound);
            }
            
        }
    }

    void playOneTime(SoundMaker sound)
    {
        AudioSource source = sound.source;
        if (source.isPlaying)
            return;
        source.volume = sound.volume;
        source.clip = sound.clip;
        source.Play();
        sound.playOneTime = false;
    }

    void playSound(SoundMaker sound)
    {
        //v�rifier si il est pas d�j� entrain d'�tre jou�
        AudioSource source = sound.source;
        if (source.isPlaying)
            return;
        source.volume = sound.volume;
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

    public void stopSmooth(SoundMaker sound)
    {
        
    }


}
