/*
Author: Jacob Kaplan-Hall
Comment/Description: gauge for visual feedback on certain values in audio levels
ChangeLog: d/m/y
<3 2/14/22 created document and linked interfaces

*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleGague : MonoBehaviour
{
    //[SerializeField, Range(0f, 1f)]
    //float Control = .5f;


    [SerializeField]
    float m_Range = 75f;
    

    //target value of the gauge
    float m_TargetValue = 0f;

    float m_AccuracyMargin = 0.03f;

    //needle that points at the current value represented by the gauge
    [SerializeField]
    GameObject m_ValueNeedle;

    [SerializeField]
    GameObject target;


    Material m_NeedleMat;

    float m_FlashValue = 0f;

    //current value of the gauge
    float m_Value = 0f;

	private void Awake()
	{
        Renderer r = m_ValueNeedle.GetComponentInChildren<Renderer>();

        if (r != null)
        {
            m_NeedleMat = r.material;
            if (m_NeedleMat == null)
                Debug.LogError("no material found!");
        }
        else
		{
            Debug.LogError("no renderer found!");
		}

        DisableLight();
	}


	public float GetMainValue()
	{
        return m_Value;
	}
    public void SetMainValue(float value)
	{
        m_Value = value;
        SetNeedleValue(value, m_ValueNeedle);

	}

    public float GetTargetValue()
    {
        return m_TargetValue;
    }
    public void SetTargetValue(float value)
    {
        m_TargetValue = value;
        SetNeedleValue(m_TargetValue, target);
    }

    public float GetAccuracyMargin()
	{
        return m_AccuracyMargin;
	}

    public void SetAccuracyMargin(float value)
	{
        m_AccuracyMargin = value;
	}

    private void Update()
	{

        float Difference = Mathf.Abs(m_Value - m_TargetValue);

        if (Difference > m_AccuracyMargin)
        {
            m_FlashValue += Time.deltaTime * ((1 - Difference) * 50f);

            //update the values of our material shader
            m_NeedleMat.SetFloat("_Brightness", (Mathf.Sin(m_FlashValue) + 1f) * .5f);


            m_NeedleMat.SetFloat("_Diff", Mathf.Clamp(Difference * 10f,0f,1f));
        }
        else
        {
            m_NeedleMat.SetFloat("_Brightness", 1f);


            m_NeedleMat.SetFloat("_Diff", 0f);
        }
    }

    public void EnableLight()
	{
        enabled = true;
	}
    
    public void DisableLight()
	{
        m_NeedleMat.SetFloat("_Brightness", 0f);
        enabled = false;

	}



    void SetNeedleValue(float value, GameObject needle)
	{
        needle.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(-m_Range, transform.right), Quaternion.AngleAxis(m_Range, transform.right), value);


	}
}
