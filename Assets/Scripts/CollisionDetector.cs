using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Water"))
        {
            if (PlayerController.instance.playerState != PlayerController.PlayerState.Dead)
            {
                PlayerController.instance.DealDamage(25, true);
            }
        }
    }
}
