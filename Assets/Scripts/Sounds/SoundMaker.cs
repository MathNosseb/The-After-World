using System.Runtime.CompilerServices;
using UnityEngine;

public enum SoundType {UI, VFX};

[System.Serializable]
public class SoundMaker
{    
    public SoundType soundType;
    public AudioClip clip;
    public AudioSource source;
    public string soundName;
    public float volume;
     public bool play;
}
 