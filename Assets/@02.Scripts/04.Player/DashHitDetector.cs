using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashHitDetector : MonoBehaviour, IObservable<GameObject>
{
    public bool mbIsCrash { get; private set; }
    private List<IObserver<GameObject>> mObservers = new List<IObserver<GameObject>>();
    private HashSet<GameObject> mDetectedEnemies = new HashSet<GameObject>();

    private float mCrashCheckDistance = 1.0f;
    private LayerMask mCrashLayer;
    
    private void OnEnable()
    {
        mbIsCrash = false;
    }

    private void FixedUpdate()
    {
        if (!mbIsCrash)
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 rayDirection = transform.forward;

            if (Physics.Raycast(origin, rayDirection, mCrashCheckDistance))
            {
                mbIsCrash = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        mbIsCrash = true;
        
        if (other.CompareTag("Enemy"))
        {
            mDetectedEnemies.Add(other.gameObject);
        }
    }

    public void Subscribe(IObserver<GameObject> observer)
    {
        if (!mObservers.Contains(observer))
        {
            mObservers.Add(observer);
        }
    }

    public void Unsubscribe(IObserver<GameObject> observer)
    {
        if (mObservers.Contains(observer))
        {
            mObservers.Remove(observer);
        }
    }

    public void Notify(GameObject value)
    {
        foreach (var observer in mObservers)
        {
            observer.OnNext(value);
        }
    }

    private void OnDisable()
    {
        var capyObservers = new List<IObserver<GameObject>>(mObservers);
        foreach (var capyObserver in capyObservers)
        {
            capyObserver.OnCompleted();
        }
        mObservers.Clear();
    }
}