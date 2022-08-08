using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "micConditions", menuName = "Audio/Microphone Condition")]
public class MicCondition : ScriptableObject
{
    [SerializeField]
    public AudioClip SoundCondition;

    [SerializeField]
    public string[] EffectConditions;

    [SerializeField]
    public float[] EffectPercentConditions;

    [SerializeField]
    public float[] EffectAccuracyMargins;

}
