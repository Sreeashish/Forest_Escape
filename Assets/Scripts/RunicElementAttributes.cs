using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunicElementAttributes : MonoBehaviour
{
    public float speed, damage;
    public ParticleSystem trial, collissionParticle;

    public IEnumerator RunicTrajectory(Transform from, Vector3 to, float damageAllocated)
    {
        damage = damageAllocated;
        transform.position = from.position;
        PlayParticles("Trial");
        while (Vector3.Distance(transform.position, to) >= 0.2f)
        {
            yield return null;
            transform.position = Vector3.MoveTowards(transform.position, to, speed * Time.deltaTime);
        }
        transform.LookAt(Camera.main.transform);
        PlayParticles("Collide");
        yield return CommonScript.GetDelay(2);
        gameObject.SetActive(false);
    }

    public void PlayParticles(string particleName)
    {
        switch(particleName)
        {
            case "Trial":
                trial.Play();
                break;
            case "Collide":
                collissionParticle.Play();
                break;
        }     
    }
}
