using UnityEngine;


[RequireComponent(typeof(SpaceShipContainer))]
public class SpaceShipSoundSystem : MonoBehaviour
{
    SpaceShipContainer spaceShipContainer;

    [Header("FX")]
    SoundMaker burningSound;
    SoundMaker switchSound;

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

            if (sound.soundName == "hyperspeed_switch")
            {
                switchSound = sound;
            }
        }
    }

    public void HandleBurnSound(bool burning)
    {
        if (!spaceShipContainer.playerInSpaceShip) return;
        //appel� par l event
        if (burningSound == null)
        {
            Debug.LogError("burning sound non trouv�");
            return;
        }
        if (burning) burningSound.play = true;
        else burningSound.play = false;
    }

    public void HandleSwitchSound()
    {
        Debug.Log("sound");
        if (!spaceShipContainer.playerInSpaceShip) return;
        if (switchSound == null)
        {
            Debug.LogError("switch sound non trouv�");
            return;
        }
        switchSound.playOneTime = true;

    }
}
