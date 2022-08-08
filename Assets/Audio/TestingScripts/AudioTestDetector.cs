// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

/*
Author: Jacob Kaplan-Hall
Comment/Description: Class that is a test audio detector
ChangeLog:
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

public class AudioTestDetector : MonoBehaviour
{
    AudioDetector m_AudioDetector;
    void Start()
    {
        m_AudioDetector = GetComponent<AudioDetector>();

        m_AudioDetector.OnAudioDetect.AddListener(HearAudio);
    }

    void Update()
    {
        
    }

    void HearAudio(AudioInstance info)
	{
        // Tells the dev that a sound was head and info about the sound that was heard
        Debug.Log("Wow! i heard " + info.Source.name + " play the sound " + info.AClip.name + " with a volume of " + info.GetVolumeAttenuated(transform.position));
	}

}
