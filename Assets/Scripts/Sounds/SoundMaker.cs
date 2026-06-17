using System.Runtime.CompilerServices;
using UnityEngine;

public enum SoundType {UI, VFX, AMBIENT};

[System.Serializable]
public class SoundMaker
{    
    public SoundType soundType;
    public AudioClip clip;
    public AudioSource source;
    public string soundName;
    [Range(0f,1f)]public float volume;
    public bool play;
}
 