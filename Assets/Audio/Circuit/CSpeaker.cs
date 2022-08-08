/*
Author: Jacob Kaplan-Hall
Comment/Description: speaker component where audio is emitted in the circuit
ChangeLog: d/m/y
2/12/21: finalized for module
*/

/*
Author: Matthew Beaurivage
Comment/Description: Added comments
ChangeLog:
11/13/2021: Added comments and Updated the Change log
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public interface ISpeaker
//{
//	public AudioSource GetSpeakerAudioSource();
//}
public class CSpeaker : CAudioCircuit// , ISpeaker
{
	//location the speaker emits audio from
	[SerializeField]
	protected Transform m_EmissionSocket;

	new protected void Awake()
	{
		//call awake and set type to speaker
        base.Awake();
        CircuitType = EAudioCircuitType.Speaker;
	}

	public void Start()
	{


	}

	public void Update()
	{
		
		//if (m_SpeakerSource.isPlaying)
		//Debug.Log(name + " Source Playback: " + m_SpeakerSource.time);
	}
	//getter for audio source
	public Transform GetEmissionSocket()
    {
		return m_EmissionSocket;
    }


}
