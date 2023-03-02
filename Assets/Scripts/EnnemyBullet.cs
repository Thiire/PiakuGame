using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(TrailRenderer))]
public class EnnemyBullet : MonoBehaviour
{
    private Rigidbody body;
    private TrailRenderer trail;
    private VisualEffect visualeffect;

    private Vector3 destination;
    private float damage = 0;
    private float maxTime = 0;
    private float elapsedTime = 0;
    private Vector3 velocity;
    private bool isEnable = false;
    void Awake()
    {
        body = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        visualeffect = GetComponent<VisualEffect>();
        visualeffect.Stop();
        trail.enabled = false;
        gameObject.SetActive(false);
    }

    public void startSpawn(Vector3 position, Vector3 direction, float strengh, float timetoFade, float olderDamage)
    {
        Debug.Log(direction);
        maxTime = timetoFade;
        damage = olderDamage;
        velocity = direction * strengh;
        transform.position = position;
        visualeffect.Play();
        trail.Clear();
        trail.enabled = true;
        isEnable = true;
        gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.SendMessage("TakeDamage", damage);
            stopSpawn();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("RedLayer") || other.gameObject.layer == LayerMask.NameToLayer("BlueLayer"))
        {
            stopSpawn();
        }
    }

    public void stopSpawn()
    {
        elapsedTime = 0;
        isEnable = false;
        visualeffect.Stop();
        trail.enabled = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (isEnable)
        {
            transform.position += velocity;
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= maxTime)
            {
                stopSpawn();
            }
        }
    }
}
