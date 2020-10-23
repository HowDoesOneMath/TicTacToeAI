using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Flippable : MonoBehaviour
{
    Vector3 myOriginalPosition;
    Quaternion myOriginalRotation;

    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myOriginalPosition = transform.position;
        myOriginalRotation = transform.rotation;

        OpponentAnimationController.ToFlip.Add(this);
    }

    protected virtual void OnDestroy()
    {
        OpponentAnimationController.ToFlip.Remove(this);
    }

    public virtual void OnFlip()
    {
        rb.isKinematic = false;
    }

    public virtual void OnUnFlip()
    {
        rb.isKinematic = true;

        transform.position = myOriginalPosition;
        transform.rotation = myOriginalRotation;
    }
}
