/*
Author: Jacob Kaplan-Hall
Summary: base interactor script for interface interaction
Update Log:
17/11/21: created script
2/12/22 improved mouse over accuracy with small objects
*/

/*
Author: Bryson Bertuzzi
Comment/Description: base interactor script for interface interaction
ChangeLog:
08/12/2021: Fixed issue with script not working in scene loading
19/01/2021: Fixed naming issue with player
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class Interactor : MonoBehaviour
{
    [SerializeField]
    Transform m_LookTransform = null;
    [SerializeField]
    GameObject m_HoverImage = null;
    [SerializeField]
    float m_Range = 3f;
    [SerializeField]
    float m_Radius = .5f;
    IInteractable m_CurrentInteractable = null;
    IInteractable m_CurrentHighlightedInteractable = null;


    [SerializeField]
    bool debug = false;

    void Start()
    {
        //give that guy a little poke
        AudioManager.Start();
        MixerManager.Init();

        m_LookTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        m_HoverImage = GameObject.Find("HandGrab");


        if (!m_LookTransform)
        {
            //dont run if we dont have a look transform
            if (debug) { Debug.LogError("Interactor on " + gameObject.name + " has no look transform!"); }
            enabled = false;
        }
        else if (!m_HoverImage)
        {
            Debug.LogWarning("Interactor " + name + " has no hover image (just so you know)");
        }
        else
        {
            //disable hover image at the start
            m_HoverImage.SetActive(false);
        }

    }
    // Update is called once per frame
    void Update()
    {
        //if we are not interacting at the moment, highlight interactables
        if (m_CurrentInteractable == null)
        {
            RaycastHit HitInfo;
            Physics.Raycast(m_LookTransform.position, m_LookTransform.forward, out HitInfo, m_Range);
            IInteractable NewInteractable = null;


            if (HitInfo.collider != null)
            {
                NewInteractable = HitInfo.collider.gameObject.GetComponent<IInteractable>();
            }



            if (NewInteractable == null)
			{
                RaycastHit HitInfo2;
                Physics.SphereCast(m_LookTransform.position, m_Radius, m_LookTransform.forward, out HitInfo2, m_Range);
                if (HitInfo2.collider != null)
                {
                    NewInteractable = HitInfo2.collider.gameObject.GetComponent<IInteractable>();

                }
            }
            


            //if we got one in our sights
            if (NewInteractable != null)
            {
                //set the highlighted interactable
                m_CurrentHighlightedInteractable = NewInteractable;
                if (debug)
                {
                    Debug.Log("Interactor " + name + " started hovering over " + m_CurrentHighlightedInteractable);
                }
                //enable hover image
                if (m_HoverImage)
                    m_HoverImage.SetActive(true);
            }
            else
            {
                //if we already are null just ignore it
                if (m_CurrentHighlightedInteractable != null)
                {
                    if (debug)
                    {
                        Debug.Log("Interactor " + name + " stopped hovering over Interactable");
                    }
                    //if we didnt get anything
                    m_CurrentHighlightedInteractable = null;
                    //disable hover image
                    if (m_HoverImage)
                        m_HoverImage.SetActive(false);
                }
            }
            
        }
        else
        {
            //if we are interacting at the moment
            if (debug)
            {
                Debug.Log("Interactor " + name + " interacting with " + m_CurrentInteractable);
            }
            m_CurrentInteractable.UpdateHoldInteract(this);
        }
        //take input
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //if we have an interactable highlighted
            if (m_CurrentHighlightedInteractable != null)
            {
                //set the interactable we are interacting with
                m_CurrentInteractable = m_CurrentHighlightedInteractable;
                //start interaction with interactable
                m_CurrentInteractable.OnInteract(this);
                //clear it from highlighted
                m_CurrentHighlightedInteractable = null;
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StopInteracting();
        }
    }
    private void FixedUpdate()
    {
        if (m_CurrentInteractable != null)
            m_CurrentInteractable.FixedUpdateHoldInteract(this);
    }
    public Transform GetLookTransform()
    {
        return m_LookTransform;
    }

    public void StopInteracting()
    {
        //end interaction with interactable if we have one
        if (m_CurrentInteractable != null)
        {
            m_CurrentInteractable.OnEndInteract(this);
            m_CurrentInteractable = null;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(m_LookTransform.position, m_LookTransform.position + m_LookTransform.forward * m_Range);
        //Gizmos.DrawSphere(m_LookTransform.position + m_LookTransform.forward * m_Range,m_Radius);
    }
}