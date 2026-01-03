using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(constant))]
[RequireComponent(typeof(NbodySimulation))]
[RequireComponent(typeof(InputManager))]
public class Container : MonoBehaviour
{
    public constant Constant { get; private set; }
    NbodySimulation simulation;
    public InputManager inputManager { get; private set; }
    float fps;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        Constant = GetComponent<constant>();
        simulation = GetComponent<NbodySimulation>();
        

    }

    public Vector3 GetGravityAcceleration(Vector3 point, out CelestialBody strongestGravitationaBody, CelestialBody ignoreBody= null)
    {
        Vector3 acceleration = simulation.CalculateAcceleration(point, Constant.GravityConstant, out strongestGravitationaBody, ignoreBody);
        return acceleration;
    }

    public Vector3 GetBodyAcceleration(CelestialBody body, Vector3 point)
    {
        return simulation.GetBodyAcceleration(body, point, Constant.GravityConstant);
    }

    public bool influenceByBody(Transform self, CelestialBody referenceBody)
    {
        return simulation.influenceByBody(self, referenceBody);
    }

    public float GetFps(float deltaTime)
    {
        float rawFps = 1 / deltaTime;
        fps = Mathf.Lerp(fps, rawFps, .1f);

        return fps;
    }

}
