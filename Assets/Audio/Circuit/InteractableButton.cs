using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableButton : MonoBehaviour, IInteractable
{
    public bool Pressed = false;

    [SerializeField]
    float m_PressDistance = .04f;
    public UnityEvent OnPressed { get; private set; } = new UnityEvent();

    private Vector3 localStartPos;
    void Awake()
	{
        localStartPos = transform.localPosition;
	}

    // Update is called once per frame
    void Update()
    {
        
        if (!Pressed)
		{
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, localStartPos, Time.deltaTime);
		}
        else
		{
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, localStartPos - Vector3.right * m_PressDistance, Time.deltaTime);
        }

    }

    public void OnInteract(Interactor interactor)
	{
        OnPressed.Invoke();
        interactor.StopInteracting();
	}
    public void UpdateHoldInteract(Interactor interactor)
	{

	}
    public void FixedUpdateHoldInteract(Interactor interactor)
	{

	}
    public void OnEndInteract(Interactor interactor)
	{

	}
}
