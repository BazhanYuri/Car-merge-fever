using System;
using UnityEngine;

public class CollisionTrigered : MonoBehaviour
{
    public event Action<Collider> OnCollisionIn;


    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionIn?.Invoke(collision.collider);
    }
    private void OnTriggerEnter(Collider other)
    {
        OnCollisionIn?.Invoke(other);
    }
}

