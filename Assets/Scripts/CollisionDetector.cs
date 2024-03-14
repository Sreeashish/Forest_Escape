using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Water"))
        {
            if (PlayerController.instance.DamageCoroutine != null)
            {
                PlayerController.instance.StopCoroutine(PlayerController.instance.DamageCoroutine);
                PlayerController.instance.FreeMovement();
            }
            PlayerController.instance.DamageCoroutine = PlayerController.instance.StartCoroutine(PlayerController.instance.GettingDamagePerSecond(25));
        }
    }
}
