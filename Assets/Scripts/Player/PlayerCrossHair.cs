using UnityEngine;


[RequireComponent(typeof(PlayerContainer))]
public class PlayerCrossHair : MonoBehaviour
{
    PlayerContainer playerContainer;
    [Header("visé de planetes")]
    
    public int selectedIndex;
    public int lastSelectedIndex;
    Notifications selectedPlanetNotif;

    void Start()
    {
        playerContainer = GetComponent<PlayerContainer>();
    }

    void Update()
    {
        //placement crosshair et information de vol
        CelestialBody planet = playerContainer.GlobalContainer.simulation.GetPlanetByIndex(selectedIndex);
        Vector3 position = planet.GetVector3Position();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

        bool isBehind = screenPos.z < 0;

        bool offScreen =
            screenPos.x < 0 || screenPos.x > Screen.width ||
            screenPos.y < 0 || screenPos.y > Screen.height;

        if (screenPos.z < 0)
        {
            screenPos *= -1;
        }
        float margin = 50f;
        screenPos.x = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
        screenPos.y = Mathf.Clamp(screenPos.y, margin, Screen.height - margin);     

        Vector2 uiPos;

        RectTransform canvasRect = playerContainer.PlayerUI.canvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            playerContainer.PlayerUI.canvas.worldCamera,
            out uiPos
        );

        playerContainer.PlayerUI.screenPosCrosshair = uiPos;
    
        if (lastSelectedIndex != selectedIndex)
        {
            if (selectedPlanetNotif != null)
            {
                playerContainer.PlayerUI.DestroyNotificationNow(selectedPlanetNotif);
            }
            selectedPlanetNotif = playerContainer.PlayerUI.SendNotification(1000f,500f, 3f, "vous sélectionnez " + planet.Name);
            lastSelectedIndex = selectedIndex;
        }
    }

    public void HandlePadUp()
    {
        selectedIndex += 1;
    }

    public void HandlePadDown()
    {
        if (selectedIndex <= 0) { return; }
        selectedIndex -= 1;
    }
}
