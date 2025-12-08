using System.Runtime.CompilerServices;
using UnityEngine;

public enum SoundType {UI, VFX};

[System.Serializable]
public class SoundMaker
{    
    public SoundType soundType;
    public AudioClip clip;
    public string soundName;
    public int volume;
    public bool play = false;
}
 