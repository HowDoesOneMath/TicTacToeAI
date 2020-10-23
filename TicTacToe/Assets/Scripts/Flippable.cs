using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all objects that can be thrown or moved
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

        //Adds itself to a static list, so that the animation controller can easily tell them to move
        OpponentAnimationController.ToFlip.Add(this);
    }

    protected virtual void OnDestroy()
    {
        //Removes itself after destruction
        OpponentAnimationController.ToFlip.Remove(this);
    }

    //flip
    public virtual void OnFlip()
    {
        //While the rigidbody is kinematic, it won't be affected by force
        //We set this to false here so that it can get shoved around
        rb.isKinematic = false;
    }

    //put back into original location
    public virtual void OnUnFlip()
    {
        rb.isKinematic = true;

        transform.position = myOriginalPosition;
        transform.rotation = myOriginalRotation;
    }
}
