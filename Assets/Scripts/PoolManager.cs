using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Header("PoolInstances")]
    public static PoolManager SharedInstance;

    [Header("FloatingText Ref")]
    public GameObject floatingText;
    public int floatingTextAmount;
    private List<GameObject> pooledFloatingText;

    [Header("EnnemyBullet")]
    public GameObject ennemyBullet;
    public int ennemyBulletAmount;
    private List<GameObject> pooledEnnemyBullet;

    [Header("NormalHit VFX")]
    public GameObject normalVFX;
    public int normalVFXAmount;
    private List<GameObject> pooledNormalVFX;

    [Header("CriticalHit VFX")]
    public GameObject criticalVFX;
    public int criticalVFXAmount;
    private List<GameObject> pooledCriticalVFX;

    void Awake()
    {
        SharedInstance = this;
    }

    void Start()
    {
        pooledFloatingText = new List<GameObject>();
        pooledEnnemyBullet = new List<GameObject>();
        pooledNormalVFX = new List<GameObject>();
        pooledCriticalVFX = new List<GameObject>();

        GameObject tmp;
        for (int i = 0; i < floatingTextAmount; i++)
        {
            tmp = Instantiate(floatingText);
            pooledFloatingText.Add(tmp);
        }
        for (int i = 0; i < ennemyBulletAmount; i++)
        {
            tmp = Instantiate(ennemyBullet);
            pooledEnnemyBullet.Add(tmp);
        }
        for (int i = 0; i < normalVFXAmount; i++)
        {
            tmp = Instantiate(normalVFX);
            tmp.SetActive(false);
            pooledNormalVFX.Add(tmp);
        }
        for (int i = 0; i < criticalVFXAmount; i++)
        {
            tmp = Instantiate(criticalVFX);
            tmp.SetActive(false);
            pooledCriticalVFX.Add(tmp);
        }
    }

    IEnumerator DisableObject(GameObject gameObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public GameObject GetPooledEnnemyBullet()
    {
        for (int i = 0; i < ennemyBulletAmount; i++)
        {
            if (!pooledEnnemyBullet[i].activeInHierarchy)
            {
                return pooledEnnemyBullet[i];
            }
        }
        return null;
    }
    public GameObject GetPooledFloatingText()
    {
        for (int i = 0; i < floatingTextAmount; i++)
        {
            if (!pooledFloatingText[i].activeInHierarchy)
            {
                return pooledFloatingText[i];
            }
        }
        return null;
    }

    public GameObject GetPooledNormalVFX()
    {
        for (int i = 0; i < normalVFXAmount; i++)
        {
            if (!pooledNormalVFX[i].activeInHierarchy)
            {
                StartCoroutine(DisableObject(pooledNormalVFX[i], 1f));
                return pooledNormalVFX[i];
            }
        }
        return null;
    }

    public GameObject GetPooledCriticalVFX()
    {
        for (int i = 0; i < criticalVFXAmount; i++)
        {
            if (!pooledCriticalVFX[i].activeInHierarchy)
            {
                StartCoroutine(DisableObject(pooledCriticalVFX[i], 1f));
                return pooledCriticalVFX[i];
            }
        }
        return null;
    }

    void Update()
    {

    }
}
