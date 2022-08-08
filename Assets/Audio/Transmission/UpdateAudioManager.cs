// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

// CHANGE LOG
// DATE FORMAT: DD/MM/YYYY
// DATE CREATED: 15/11/2021

/*
Author: Aviraj Singh Virk
Comment/Description: Audio Detecting System
ChangeLog:
15/11/2021: 15/11/2021: Added ChangeLog and function definition comments
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class UpdateAudioManager : MonoBehaviour
{
	// Update is called once per frame

	private void FixedUpdate()
	{
        AudioManager.FixedUpdate();
	}
}
