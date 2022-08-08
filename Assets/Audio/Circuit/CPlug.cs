// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Author: Seth Grinstead
ChangeLog:
12/11/2021: Added change log and comments
*/

/*
 * Author: Jacob Kaplan-Hall
 * ChangeLog: d/m/y
 * 17/11/21: changed plug to use new interaction interface
 * 2/12/21: finalized for module
 */

public class CPlug : MonoBehaviour, IInteractableListener
{
	//current socket interacting with plug, null means unplugged
	private CSocket m_CurrentSocket = null;

	//partner connected plug, used to link circuit
	[SerializeField]
	private CPlug m_Partner;

	//distance plug will sit off of the stocket (kinda temporary)
	[SerializeField]
	float m_PlugDistanceOffset = 3.3f;

	//plugs interactable component
	[SerializeField]
	PhysicsInteractable m_PlugInteractable = null;

	//reference to joint used to attach the plug to the socket, null means not plugged in
	ConfigurableJoint m_SocketAttachementJoint = null;

	//max distance the plug can be from its partner before unplugging or dropping; All this to quell the physics
	public float m_MaxPartnerDist = 0;


	
	private Rigidbody m_RigidBody = null;


    private void Awake()
    {
		enabled = false;

		m_RigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
		//too taught!!
		if (m_CurrentSocket != null)
        {
			if (Vector3.Distance(transform.position, m_Partner.transform.position) > m_MaxPartnerDist + 1f)
			{
				UnPlug();
				return;
			}


			if (m_CurrentSocket != null)
			{

				//put the plug in the right place
				transform.position = Vector3.Lerp(transform.position,m_CurrentSocket.transform.position + m_CurrentSocket.transform.up * .15f,Time.deltaTime * 20);
				transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(-m_CurrentSocket.transform.up),Time.deltaTime * 6);
				//transform.rotation = Quaternion.LookRotation(-m_CurrentSocket.transform.up);
				m_SocketAttachementJoint.connectedAnchor = transform.position;
				
				//transform.position = Vector3.Lerp(transform.position, m_CurrentSocket.transform.position + m_CurrentSocket.transform.up * 1f,Time.time);
				//transform.rotation = Quaternion.LookRotation((m_CurrentSocket.transform.position - m_CurrentSocket.transform.up) - transform.position);
				//m_SocketAttachementJoint.transform.position = transform.position;
			}

		}
		
    }


	//use distance joint to keep the two audio components from going too far from each other when attached
    private void OnTriggerEnter(Collider NewCollider)
    {
		// m_CurrentSocket does not exist
		if (m_CurrentSocket == null)
		{
 			CSocket socket = NewCollider.gameObject.GetComponent<CSocket>();


			//if we got a socket connect to it
			//this would hard crash if we didnt do this check to make sure im not routing the cables in a circle;
			if (socket != null && socket.GetAudioCircuit() != this && socket.CurrentPlug == null)
			{

				//you cant plug inputs into outputs and vice versa
				if (m_Partner.GetCurrentSocket() == null || socket.IsInput != m_Partner.GetCurrentSocket().IsInput && m_Partner.GetCurrentSocket().GetAudioCircuit() != socket.GetAudioCircuit())
				{
					//if we get a valid socket
					////put the plug in the right place
					//transform.position = socket.transform.position + socket.transform.up * .15f;
					//transform.rotation = Quaternion.LookRotation(socket.transform.position - transform.position, transform.up);
					//
					//attach it to the circuit with a physics joint
					m_SocketAttachementJoint = gameObject.AddComponent<ConfigurableJoint>();

					m_SocketAttachementJoint.xMotion = ConfigurableJointMotion.Locked;
					m_SocketAttachementJoint.yMotion = ConfigurableJointMotion.Locked;
					m_SocketAttachementJoint.zMotion = ConfigurableJointMotion.Locked;
					m_SocketAttachementJoint.angularXMotion = ConfigurableJointMotion.Free;
					m_SocketAttachementJoint.angularYMotion = ConfigurableJointMotion.Free;
					m_SocketAttachementJoint.angularZMotion = ConfigurableJointMotion.Free;

					m_SocketAttachementJoint.connectedAnchor = transform.position;
					//yikes better null check this at some point, 
					m_SocketAttachementJoint.connectedBody = socket.transform.parent.gameObject.GetComponent<Rigidbody>();

					m_CurrentSocket = socket;
					m_CurrentSocket.HandleConnection(this);

					m_RigidBody.freezeRotation = true;

					//if the plug is being held, have it get dropped
					if (m_PlugInteractable != null)
					m_PlugInteractable.Drop();
				}
			}
		}


    }


	public void UnPlug()
    {
		if (m_CurrentSocket)
		{
			//break joint holding it in place
			if (m_SocketAttachementJoint)
			{
				Destroy(m_SocketAttachementJoint);
				m_SocketAttachementJoint = null;
			}

			m_RigidBody.freezeRotation = false;

			//disconnect
			m_CurrentSocket.HandleConnection(null);
		}
    }

	//getters / setters
	public CSocket GetCurrentSocket()
	{
		return m_CurrentSocket;
	}

	public void SetCurrentSocket(CSocket newSocket)
	{
		m_CurrentSocket = newSocket;
	}

	public CPlug GetPartner()
	{
		return m_Partner;
	}

	public void SetPartner(CPlug newPartner)
	{
		m_Partner = newPartner;
	}

	public CAudioCircuit GetConnectedCircuit()
	{
		// If m_Partner and m_Partner's socket exist
		if (m_Partner != null && m_Partner.GetCurrentSocket() != null)
		{
			//return the circuit (if any) connected to the other end of the plug
			return m_Partner.GetCurrentSocket().GetAudioCircuit();
		}

		return null;
	}








	//interaction interface


	public void OnInteract(Interactor interactor)
    {
		//when we pick this up, unplug it
		UnPlug();
    }

	public void UpdateHoldInteract(Interactor interactor)
	{
	}

	public void FixedUpdateHoldInteract(Interactor interactor)
    {
		//if we get into a situation where we are stretched too far, just disconnect, its not worth it man
		if (Vector3.Distance(transform.position, m_Partner.transform.position) > m_MaxPartnerDist)
		{
			interactor.StopInteracting();
		}


	}

	public void OnEndInteract(Interactor interactor)
    {
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;

		if (m_SocketAttachementJoint != null)
		Gizmos.DrawSphere(m_SocketAttachementJoint.transform.position, .02f);
	}
}
