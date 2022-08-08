using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class IndicatorLight : MonoBehaviour, IControlCallback
{

    Material m_LightMat;

    public float Brightness = 0f;

    float m_CurrentBrightness = 0f;

    [SerializeField]
    float m_Speed = 2f;

    // Start is called before the first frame update
    void Awake()
    {

        m_LightMat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        m_CurrentBrightness = Mathf.Lerp(m_CurrentBrightness, Brightness, Time.deltaTime * m_Speed);
        m_LightMat.SetFloat("_Brightness", m_CurrentBrightness);
    }

    public void OnControlUpdate(float value)
	{

	}
}
