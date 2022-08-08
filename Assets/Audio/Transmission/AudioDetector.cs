// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

// CHANGE LOG
// DATE FORMAT: DD/MM/YYYY
// DATE CREATED: 20/10/2021

/*
Author: Aviraj Singh Virk
Comment/Description: Audio Detecting System
ChangeLog:
15/11/2021: 15/11/2021: Added ChangeLog and function definition comments
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



interface IAudioDetectionCallback
{
    public void OnNewSoundPlayed(ISound newSound, MixerAudioEffect[] liveEffects);
}


// class def
public class AudioDetector : MonoBehaviour
{
	[SerializeField]
	public List<EAudioType> AudioTypeFlags;

	[SerializeField]
	public float MinVolume = 0.05f;

    [HideInInspector]
    public UnityEvent<AudioInstance> OnAudioDetect;

    IAudioDetectionCallback[] m_iaudiocallbacks = null;

    // Called when the Scene loads up, before Start is called
    void Awake()
    {
        //make sure the manager is started
        AudioManager.Start();
		//register detector
		AudioManager.AddDetector(this);

        //get it if we have it
		 m_iaudiocallbacks = GetComponents<IAudioDetectionCallback>();

    }

    //handle an audio detection event
    public void HandleDetection(AudioInstance info)
	{
		//sends audio info to custom delegate
        OnAudioDetect.Invoke(info);
       
	}

    public void HandleNewSoundPlayed(ISound newSound, MixerAudioEffect[] liveEffects)
	{
        //interface method, new and imporved
        if (m_iaudiocallbacks != null)
		{
            foreach(IAudioDetectionCallback c in m_iaudiocallbacks)
            c.OnNewSoundPlayed(newSound,liveEffects);
		}
	}

    private void OnDestroy()
    {
        //remove detector
        AudioManager.RemoveDetector(this);
    }
}
