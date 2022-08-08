// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

// CHANGE LOG
// DATE FORMAT: DD/MM/YYYY
// DATE CREATED: 20/10/2021

/*
Author: Aviraj Singh Virk
Comment/Description: Audio Detecting System
ChangeLog:
15/11/2021: 15/11/2021: Added ChangeLog and function definition comments and null checks
*/

/*
Author: Seth Grinstead
Comment/Description: Class that control box sound behaviour with physics
ChangeLog:
12/11/2021: Added comments
18/11/2021: Updated collision to check if the player was involved in the collision (For monster detection)
18/11/2021: Updated collision so that monster does not trigger itself by walking into a box
*/

/*
Author: Jacob Kaplan-Hall
ChangeLog
14/11/21: changed script to work with new audio system
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoxSound : MonoBehaviour
{
    public AudioClip FirstAudioClip;           // Reference to an AudioClip in scene
    public AudioClip SecondAudioClip;          // Reference to a second AudioClip in scene

    public float MaxAudioRange = 40f;          // Max range audio will reach
    public float MaxForce = 10f;               // Maximum force that will affect sound output
    public float MinForce = .5f;               // Minumum force that will affect sound output
    public float VolumeScale = 1.6f;           // Multiplication factor for volume

    public bool WasHeldByPlayer = false;
    public bool WasHitByMonster = false;

    bool m_audioToggle = true;                 // Toggle between first and second AudioClip

    AudioSource m_Source;

    GameObject m_playerRef;                    // Reference to player in scene

    [Tooltip("Time on start where collisions are ignored")]
    public float AStartTimer = 1f;
    bool m_start;

    //it didnt work

    //[SerializeField]
    //float AccelerationTriggerSpeed = 1f;
    //[SerializeField]
    //int VelocityBufferSize = 3;
    //
    //
    //
    //List<float> VelocityBuffer;
    //int VelocityBufferPos = 0;

    // Start is called before the first frame update
    void Start()
    {
        //VelocityBuffer = new List<float>();
        //VelocityBuffer.Capacity = VelocityBufferSize;

        m_start = false;

        if (GetComponent<AudioSource>())
        {
            m_Source = GetComponent<AudioSource>();
        }

        m_playerRef = GameObject.Find("PR_Player");

        StartCoroutine(StartTimer());
    }

    // Called wheenever a collision happens with the GameObject
    void OnCollisionEnter(Collision collision)
    {
        // If applied force to object is greater than MinFOrce and m_start is true
        if (collision.relativeVelocity.magnitude >= MinForce && m_start)
        {
            float force = collision.relativeVelocity.magnitude;

            float volume = force / MaxForce;
            
            // If m_AudioToggle is true
            if (m_audioToggle) 
            {
                m_audioToggle = false;

                if (collision.gameObject == m_playerRef || WasHeldByPlayer) 
                {
                    // If other object is a box and player is holding this one
                    // I want to make sure the other box's sound is tagged as a player sound too
                    BoxSound sound = collision.gameObject.GetComponent<BoxSound>();
                    if (sound)
                        sound.WasHeldByPlayer = true;

                    AudioManager.EmitSound(m_Source, SecondAudioClip, new EAudioType[] { EAudioType.Player });
                    WasHeldByPlayer = false;
                }
                else if (WasHitByMonster)
                {
                    // If Monster bumps into a box, it won't trigger itself
                    AudioManager.EmitSound(m_Source, SecondAudioClip, new EAudioType[] { EAudioType.None });
                }
                else
                    AudioManager.EmitSound(m_Source, SecondAudioClip, new EAudioType[] { EAudioType.Physics });
            }
            else
            {
                m_audioToggle = true;

                if (collision.gameObject == m_playerRef || WasHeldByPlayer)
                {
                    // If other object is a box and player is holding this one
                    // I want to make sure the other box's sound is tagged as a player sound too
                    BoxSound sound = collision.gameObject.GetComponent<BoxSound>();
                    if (sound)
                        sound.WasHeldByPlayer = true;

                    AudioManager.EmitSound(m_Source, FirstAudioClip, new EAudioType[] { EAudioType.Player });
                    WasHeldByPlayer = false;
                }
                else if (WasHitByMonster)
                {
                    // If Monster bumps into a box, it won't trigger itself
                    AudioManager.EmitSound(m_Source, FirstAudioClip, new EAudioType[] { EAudioType.None });
                }
                else
                    AudioManager.EmitSound(m_Source, FirstAudioClip, new EAudioType[] { EAudioType.Physics });
            }
        }

    }

    // Enumerator function for a timer
    IEnumerator StartTimer()
    {
        // While AStartTime is greater than zero
        while (AStartTimer > 0f)
        {
            AStartTimer -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        m_start = true;

        yield return null;
    }
}