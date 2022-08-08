// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

/*
Author: Jacob Kaplan-Hall
Comment/Description: Class that is a test script
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
using UnityEngine.Audio;

public class AudioTestScript : MonoBehaviour
{
    Vector3 InitalPosition; // variable for initial position

    [SerializeField]
    AudioSource Source;     // Audio Source

    [SerializeField]
    AudioClip Clip1;        // Audio Clip

    [SerializeField]
    AudioClip Clip2;        // Audio Clip

    [SerializeField]
    AudioClip CompClip;     // Audio Clip

    [SerializeField]
    float DistTest = 0;
    // Start is called before the first frame update
    void Start()
    {
        // stores the initial position
        InitalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // checks if the audio source is playing
        if (Source.isPlaying)
		{
            transform.Rotate(new Vector3(0, 0, 360) * Time.deltaTime);

            transform.position = InitalPosition + Vector3.up * Source.time * 5;
		}

        // Checks input for the "1" key
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Source.Play();
        }

        // Checks input for the "2" key
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Source.PlayOneShot(Clip1);
        }

        // Checks input for the "3" key
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            
        }
    }
}
