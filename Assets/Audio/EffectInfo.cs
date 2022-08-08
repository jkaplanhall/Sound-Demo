using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EffectInfo", menuName = "Audio/Effect Info")]
public class EffectInfo : ScriptableObject
{
	[SerializeField]
	public string Name = null;
	[SerializeField]
	public string[] Snapshots = null;
	[SerializeField]
	public float[] SnapshotPercents = null;

	[SerializeField]
	public AudioClip[] EffectClips = null;
}
