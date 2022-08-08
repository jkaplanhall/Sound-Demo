// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.
/*
Author: Seth Grinstead
Comment/Description: Script handling volume output of TransmitterBox
ChangeLog:
20/10/2021: Created script
10/11/2021: Refined volume output and fixed rotation bugs. Added comments.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IControlCallback
{
    //sends a normalized value reperesenting the state of the control
    public void OnControlUpdate(float NewValue);
}
public interface IControl
{
    public void SetControlCallback(IControlCallback callback);
    public float GetValue();

    public void SetValue(float value);
}
public class TransmitterKnob : MonoBehaviour , IInteractable , IControl
{

    [SerializeField]
    bool Modulate = false;

    [SerializeField]
    float FrequencyRange = 100f;


    private float tValue = 0;
    private float AutoValue;


    [SerializeField]
    float m_rotateSpeed = 1000.0f;         // Speed that cylinder rotates
    float m_volume = 0.0f;                 // Total volume that cylinder rotation represents
    [SerializeField]
    float m_maxAngle = 180.0f;             // Max angle knob can rotate
    Quaternion LocalStartRotation;
    public IControlCallback KnobCallback = null;
    // Start is called before the first frame update
    void Awake()
    {
        LocalStartRotation = transform.localRotation;
    }
    public void SetControlCallback(IControlCallback callback)
    {
        KnobCallback = callback;
       
    }
    public float GetValue()
    {
        return m_volume;
    }

	private void FixedUpdate()
	{
        //if we are modulating our effect we will calculate the value based on the value of the knob, the time, and our frequency range
        if (Modulate)
        {
            tValue += m_volume * FrequencyRange * Time.fixedDeltaTime;
            AutoValue = (Mathf.Sin(tValue) + 1) * .5f;

            //notify our callback of the value.
            if (KnobCallback != null)
                KnobCallback.OnControlUpdate(AutoValue);
        }
	}
	public void SetValue(float value)
    {

        m_volume = Mathf.Clamp(value,0,1);

        transform.localRotation = Quaternion.Lerp(LocalStartRotation, LocalStartRotation * Quaternion.Euler(0, m_maxAngle, 0), m_volume);

        //notify our callback of the value.
        if (KnobCallback != null && !Modulate)
            KnobCallback.OnControlUpdate(m_volume);
    }
    //Function handling rotation based on mouse input
    public void Rotate(float input)
    {
        if (input != 0)
        {
            //Change volume based on local x and volumeFactor variable
            SetValue(Mathf.Clamp(m_volume + input * m_rotateSpeed, 0, 1));


        }
    }
    //Returns total volume
    public float GetVolume()
    {
        return m_volume;
    }
    //interactable interface
    float m_lastAngle = 0;
    public void OnInteract(Interactor interactor)
    {
        m_lastAngle = interactor.GetLookTransform().eulerAngles.y;
    }
    public void UpdateHoldInteract(Interactor interactor)
    {
        float InteractorAngle = interactor.GetLookTransform().eulerAngles.y;
        Rotate(Mathf.DeltaAngle(m_lastAngle, InteractorAngle));
        m_lastAngle = InteractorAngle;
    }
    public void FixedUpdateHoldInteract(Interactor interactor)
    {
    }
    public void OnEndInteract(Interactor interactor)
    {
    }
}