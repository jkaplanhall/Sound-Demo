/*
 *Author: Jacob Kaplan-Hall
 *Summary: base interactable interface
 *Update Log:
 *17/11/21: created script
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IInteractable
{
    public void OnInteract(Interactor interactor);
    public void UpdateHoldInteract(Interactor interactor);
    public void FixedUpdateHoldInteract(Interactor interactor);
    public void OnEndInteract(Interactor interactor);
}
public interface IInteractableListener
{
    public void OnInteract(Interactor interactor);
    public void UpdateHoldInteract(Interactor interactor);
    public void FixedUpdateHoldInteract(Interactor interactor);
    public void OnEndInteract(Interactor interactor);
}