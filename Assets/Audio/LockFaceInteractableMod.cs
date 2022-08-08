using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockFaceInteractableMod : MonoBehaviour, IInteractableListener
{
    //interact listener Interface


    [SerializeField]
    float m_HoldRotateSpeed = 180f;

    //this interface just rotates the circuit the face the player, 

    public void OnInteract(Interactor interactor)
    { }

    public void UpdateHoldInteract(Interactor interactor)
    {
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(interactor.GetLookTransform().right), m_HoldRotateSpeed * Time.deltaTime);
        //we want the interaction to rotate the nearest flat side towards the player
        //first lets calculate the difference
        float diff = Mathf.DeltaAngle(interactor.GetLookTransform().eulerAngles.y, transform.eulerAngles.y - 90f);
        //save the sign of that so we can just work in postive space
        float sign = diff >= 0 ? 1f : -1f;
        float mag = Mathf.Abs(diff);
        //have our start angle
        float angle = 0f;
        while (angle < 180f)
        {
            //lets check if we are within range of one of the sides
            if (Mathf.Abs(mag - angle) < 45f)
            {
                break;
            }
            //if not, lets rotate the the next angle
            angle += 90f;
        }
        //inform the rotation of our circuit component based on the angle we just calculated
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(interactor.GetLookTransform().right) * Quaternion.AngleAxis(angle * sign, Vector3.up), m_HoldRotateSpeed * Time.deltaTime);
    }

    public void FixedUpdateHoldInteract(Interactor interactor)
    { }

    public void OnEndInteract(Interactor interactor)
    { }
}
