using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MixerEffectStack : MonoBehaviour
{
    List<MixerAudioEffect> m_CurrentEffects;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddEffect(MixerAudioEffect effect)
    {
        m_CurrentEffects.Add(effect);
    }

    public void ClearEffects()
    {
        m_CurrentEffects.Clear();
    }
}
