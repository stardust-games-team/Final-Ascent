using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour, IDamageable
{
    [SerializeField] private FracturedAsteroid _fracturedAsteroidPrefab;
    [SerializeField] private Detonator _explosionPrefab;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        FractureAsteroid(hitPosition);
    }

    private void FractureAsteroid(Vector3 hitPosition)
    {
        if (_fracturedAsteroidPrefab != null)
        {
           FracturedAsteroid _fracturedAsteroid = Instantiate(_fracturedAsteroidPrefab, _transform.position, _transform.rotation);
                   _fracturedAsteroid.transform.localScale = _transform.localScale;
        }


        if (_explosionPrefab != null)
        {
            Instantiate(_explosionPrefab, transform.position/*hitPosition*/, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
}