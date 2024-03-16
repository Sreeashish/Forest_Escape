using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Water"))
        {
            PlayerController.instance.DealDamage(25);
        }
        //if(collision.collider.CompareTag("Lethal"))
        //{
        //    collision.collider.enabled = false;
        //    PlayerController.instance.StartCoroutine(PlayerController.instance.Death());
        //}
    }
}
