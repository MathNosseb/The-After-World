using UnityEngine;


[RequireComponent(typeof(SpaceShipContainer))]
public class SpaceShipSoundSystem : MonoBehaviour
{
    SpaceShipContainer spaceShipContainer;

    [Header("FX")]
    SoundMaker burningSound;

    private void Awake()
    {
        spaceShipContainer = GetComponent<SpaceShipContainer>();
    }

    private void Start()
    {
        //recuperer les sons
        foreach (SoundMaker sound in spaceShipContainer.soundsManager.sounds)
        {
            if (sound.soundName == "rocket_burning")
            {
                burningSound = sound;
            }
        }
    }

    public void HandleBurnSound(bool burning)
    {
        //appelé par l event
        if (burningSound == null)
        {
            Debug.LogError("burning sound non trouvé");
            return;
        }
        if (burning) burningSound.play = true;
        else burningSound.play = false;
    }
}
