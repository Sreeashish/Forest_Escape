using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CombatController : MonoBehaviour
{
    Camera cam;
    public float lightAttackDamage, heavyAttackDamage, lightAttackRechargeTime, heavyAttackRechargeTime, aimDistance;
    public RectTransform crosshairRect;
    public RunicElementAttributes runicElement;
    public List<Inventory> runicBullets;
    public int runicsLimit, runicAvailable;
    public Transform triggerPoint, runicsParent;
    public LayerMask groundLayer;

    public static CombatController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        cam = Camera.main;
        InitializeInventoryPool();
    }

    int CalculateNumberOfRunicBullets()
    {
        int count = 0;
        for (int i = 0; i < runicBullets.Count; i++)
        {
            if (!runicBullets[i].isFired)
            {
                count++;
            }
        }
        return count;
    }

    public void GetRunicBulletCount()
    {
        float currentrunic = runicAvailable;
        runicAvailable = CalculateNumberOfRunicBullets();
        UiController.instance.StartCoroutine(UiController.instance.FillFillbar(currentrunic, runicAvailable, false));
    }

    void InitializeInventoryPool()
    {
        runicBullets = new List<Inventory>();
        for (int i = 0; i < runicsLimit; i++)
        {
            Inventory inventoryItem = new Inventory();
            RunicElementAttributes runicObject = Instantiate(runicElement.gameObject, runicsParent).GetComponent<RunicElementAttributes>();
            runicObject.gameObject.SetActive(false);
            inventoryItem.runics = runicObject;
            runicBullets.Add(inventoryItem);
            GetRunicBulletCount();
        }
    }

    public void CrosshairRay()
    {
        Vector3 crosshairAimPoint = crosshairRect.position;
        Ray ray = cam.ScreenPointToRay(crosshairAimPoint);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, aimDistance))
        {
            Vector3 hitPoint = rayHit.point;
            bool hitOnNavMesh = NavMesh.SamplePosition(hitPoint, out NavMeshHit navHit, 0.1f, NavMesh.AllAreas);
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(FireRunic(hitPoint));
            }
            if (Physics.Raycast(ray, out rayHit, aimDistance, groundLayer) && hitOnNavMesh)
            {
                Debug.DrawLine(triggerPoint.position, hitPoint, Color.blue);
                if (Input.GetMouseButtonDown(2))
                {
                    PlayerController.instance.StartCoroutine(PlayerController.instance.TeleportPlayer(hitPoint));
                }
            }
            else
            {
                Debug.DrawLine(triggerPoint.position, hitPoint, Color.green);

            }
        }
        else
        {
            Vector3 endPoint = ray.origin + ray.direction * aimDistance;
            Debug.DrawLine(triggerPoint.position, endPoint, Color.red);
            print("TOO FAR OUT");
        }
    }

    IEnumerator FireRunic(Vector3 hitPoint)
    {
        Vector3 fixedHitPoint = hitPoint;
        if (runicAvailable > 0)
        {
            foreach (Inventory bullet in runicBullets)
            {
                if (!bullet.isFired)
                {
                    PlayerController.instance.SetPlayerMode(PlayerController.PlayerMode.MidAttack);
                    yield return CommonScript.GetDelay(0.75f);
                    bullet.runics.gameObject.SetActive(true);
                    bullet.runics.StartCoroutine(bullet.runics.RunicTrajectory(triggerPoint, fixedHitPoint, lightAttackDamage));
                    bullet.isFired = true;
                    GetRunicBulletCount();
                    yield return CommonScript.GetDelay(0.2f);
                    StartCoroutine(bullet.RechargeRunic(lightAttackRechargeTime));
                    PlayerController.instance.SetPlayerMode(PlayerController.PlayerMode.CombatReady);
                    break;
                }
            }
        }
        else
        {
            PlayerController.instance.SetPlayerMode(PlayerController.PlayerMode.CombatReady);
        }
    }
}

[System.Serializable]
public class Inventory
{
    public RunicElementAttributes runics;
    public bool isFired;

    public IEnumerator RechargeRunic(float rechargeTime)
    {
        float t = 0;
        while (t < rechargeTime)
        {
            yield return null;
            t += Time.deltaTime;
        }
        runics.damage = 0;
        isFired = false;
        CombatController.instance.GetRunicBulletCount();
    }
}
