using UnityEngine;

public class FloatingPoint : MonoBehaviour
{
    public CelestialBody.DoubleVector3 originOffset = CelestialBody.DoubleVector3.zero; // Cumulative shift in doubles
    public float shiftThreshold = 1e6f; // Shift when player > this distance from local origin
    public PlayerContainer player; // Reference to player

    private void FixedUpdate()
    {
        // Use player's double position as reference (or strongest body/player average for multi-body)
        CelestialBody.DoubleVector3 playerDoublePos = new CelestialBody.DoubleVector3(
            player.PlayerRB.position.x,
            player.PlayerRB.position.y,
            player.PlayerRB.position.z
            ) - originOffset;
        Vector3 playerLocalPos = playerDoublePos.convert;

        if (playerLocalPos.magnitude > shiftThreshold)
        {
            // Shift origin by player's current local position (rounded to floats for stability)
            CelestialBody.DoubleVector3 shift = new CelestialBody.DoubleVector3(playerLocalPos.x, playerLocalPos.y, playerLocalPos.z);
            originOffset += shift;

            // Update all celestial bodies' double positions relative to new origin
            foreach (var body in FindObjectsByType<CelestialBody>(FindObjectsSortMode.None))
            {
                body.currentPosition -= shift;
                body.startPosition -= shift; // If needed for resets
            }

            // Update player's double position
            player.PlayerRB.position -= shift.convert;

            // Immediately update Unity transforms to new relative floats
            UpdateAllTransforms();
        }
    }

    public void UpdateAllTransforms()
    {
        // Sync all bodies to their relative float positions
        foreach (var body in FindObjectsByType<CelestialBody>(FindObjectsSortMode.None))
        {
            body.transform.position = (body.currentPosition).convert; // Relative to origin
        }
        // Sync player
        
    }

    public CelestialBody.DoubleVector3 GetAbsolutePosition(CelestialBody.DoubleVector3 relativePos)
    {
        return relativePos + originOffset; // For any absolute calc if needed
    }
}