using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    Camera cam;
    public float lightAttackDamage, heavyAttackDamage;
    public float lightAttackRechargeTime, heavyAttackRechargeTime;
    public RectTransform crosshairRect;
    public RunicElementAttributes runicElement;
    public List<Inventory> runicBullets;
    public int runicsLimit, runicAvailable;
    public Transform triggerPoint, runicsParent;

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

    public IEnumerator CrosshairRay()
    {
        Vector3 crosshairAimPoint = crosshairRect.position;
        Ray ray = cam.ScreenPointToRay(crosshairAimPoint);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit))
        {
            Vector3 hitPoint = rayHit.point;
            Debug.DrawLine(triggerPoint.position, hitPoint, Color.green);
            if (Input.GetMouseButtonDown(0))
            {
                PlayerController.instance.SetPlayerMode(PlayerController.PlayerMode.MidAttack);
                yield return CommonScript.GetDelay(1);
                StartCoroutine(FireRunic(hitPoint));
            }
        }
    }

    IEnumerator FireRunic(Vector3 hitPoint)
    {
        Vector3 fixedHitPoint = hitPoint;
        if (runicBullets.Count > 0)
        {
            foreach (Inventory bullet in runicBullets)
            {
                if (!bullet.isFired)
                {
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
