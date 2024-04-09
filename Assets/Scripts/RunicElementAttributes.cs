using System.Collections;
using UnityEngine;

public class RunicElementAttributes : MonoBehaviour
{
    public float speed, damage, collisionRadius;
    public ParticleSystem trial, collissionParticle;

    public IEnumerator RunicTrajectory(Transform from, Vector3 to, float damageAllocated)
    {
        damage = damageAllocated;
        transform.position = from.position;
        PlayParticles("Trial");

        bool hitEnemy = false;

        while (Vector3.Distance(transform.position, to) >= 0.2f && !hitEnemy)
        {
            yield return null;
            transform.position = Vector3.MoveTowards(transform.position, to, speed * Time.deltaTime);

            Collider[] colliders = Physics.OverlapSphere(transform.position, collisionRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    hitEnemy = true;
                    EnemyBehaviour enemy = collider.GetComponent<EnemyBehaviour>();
                    enemy.GetHit(damage);
                    break;
                }
            }
        }
        transform.LookAt(Camera.main.transform);
        PlayParticles("Collide");
        yield return CommonScript.GetDelay(2);
        gameObject.SetActive(false);
    }


    public void PlayParticles(string particleName)
    {
        switch (particleName)
        {
            case "Trial":
                collissionParticle.Stop();
                trial.Play();
                break;
            case "Collide":
                trial.Stop();
                collissionParticle.Play();
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; 
        Gizmos.DrawWireSphere(transform.position, collisionRadius); 
    }
}
