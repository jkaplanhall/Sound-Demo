using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsInteractable : MonoBehaviour , IInteractable
{
	//Interaction Interface
	[SerializeField]
	float m_holdDist = 2f;
	[SerializeField]
	float m_HoldSpeed = 5f;
	Interactor m_CurrentInteractor = null;

	//another interactable that this can send updates to
	IInteractableListener m_Listener = null;

	Rigidbody m_MyRigidBody = null;


    private void Start()
    {
		m_MyRigidBody = GetComponent<Rigidbody>();
		m_Listener = GetComponent<IInteractableListener>();
	}
    public void OnInteract(Interactor interactor)
	{
		if (m_Listener != null)
			m_Listener.OnInteract(interactor);

		m_CurrentInteractor = interactor;



	}

	public void UpdateHoldInteract(Interactor interactor)
	{
		if (m_Listener != null)
			m_Listener.UpdateHoldInteract(interactor);
	}

	public void FixedUpdateHoldInteract(Interactor interactor)
	{
		if (m_Listener != null)
			m_Listener.FixedUpdateHoldInteract(interactor);

		m_MyRigidBody.velocity = Vector3.ClampMagnitude(m_MyRigidBody.velocity, 10);
		m_MyRigidBody.angularVelocity = Vector3.zero;
		Vector3 TargetPos = interactor.GetLookTransform().position + interactor.GetLookTransform().forward * m_holdDist;
		m_MyRigidBody.AddForce((TargetPos - transform.position) * 5000f);

	}

	public void OnEndInteract(Interactor interactor)
	{
		if (m_Listener != null)
			m_Listener.OnEndInteract(interactor);

		m_CurrentInteractor = null;
	}

	public void Drop()
    {
		if (m_CurrentInteractor != null)
        {
			m_CurrentInteractor.StopInteracting();
        }
    }
}
