using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PlayerCrossHair))]
public class PlayerCrossHairEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlayerCrossHair playerCrossHair = (PlayerCrossHair)target;

        if (GUILayout.Button("Teleport To Selected Planet"))
        {
            if (Application.isPlaying)
                playerCrossHair.TeleportToSelectedPlanet();
            else
                GUI.Label(new Rect(10, 10, 300, 20), "Lancer avant d'utiliser");
        }
    }
}
