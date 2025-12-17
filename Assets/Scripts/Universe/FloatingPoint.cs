using System.Collections.Generic;
using UnityEngine;

public class FloatingPoint : MonoBehaviour
{
    public dataHolder data;
    public List<Transform> Transforms;
    GameObject player;
    Rigidbody playerRb;

    public void Awake()
    {
        player = data.player;
        playerRb = data.playerRb;
        if (player == null)
            Debug.LogError("player non assigné dans le dataHolder");
    }

    void FixedUpdate()
    {
        Vector3 offset = -player.transform.position;
        if (playerRb.position.magnitude > data.distanceBeforeFloatingPointRepare)
        {
            foreach (var v in Transforms)
            {
                v.transform.position += offset;
            }
            playerRb.position = Vector3.zero;
        }
    }



}
