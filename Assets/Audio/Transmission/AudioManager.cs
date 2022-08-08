// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

// CHANGE LOG
// DATE FORMAT: DD/MM/YYYY
// DATE CREATED: 15/11/2021
/*
Author: Jacob Kaplan-Hall
Summary: Static manager class for entire audio system
Update Log:
17/11/21: created script
2/12/22 improved mouse over accuracy with small objects
*/
/*
Author: Aviraj Singh Virk
Comment/Description: AudioManager for the whole AudioSystem
ChangeLog:
15/11/2021: Added ChangeLog and function definition comments
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;

// Enum for?
public enum EAudioType
{
    None = 0,
    Player,
    SoundPuzzle,
    Physics,
    Special,
    Any,
}

public class AudioInstance
{

    public List<EAudioType> AudioTypeFlags;

    //source emitting the sound
    public AudioSource Source { get; private set; } = null;
    //the clip being played
    public AudioClip AClip { get; private set; } = null;

    //the time in game the clip started playing
    public float GameStartTime { get; private set; } = 0;

    //the time in seconds the clip has been playing
    public float TimePlaying { get; private set; } = 0;
    //current playback time of the clip in seconds adjusted for time change by pitch
    public float CurrentPlaybackPosition { get; private set; } = 0;

    //if the sound is expired or not
    public bool SoundExpired { get; private set; } = false;

    // function def
    public AudioInstance(AudioSource source, EAudioType[] flags)
    {
        //create and populate our list
        AudioTypeFlags = new List<EAudioType>();
        

        if (flags.Length != 0)
        {
            for (int i = 0; i < flags.Length; i++)
            {
                AudioTypeFlags.Add(flags[i]);
            }
        }
        else
        {
            AudioTypeFlags.Add(EAudioType.None);
        }



        Source = source;
        GameStartTime = Time.time;
    }

    //
    public void Update()
    {


        if (SoundExpired == false)
        {
            //adjust this guy for pitch and cap when we get to the end
            CurrentPlaybackPosition = Mathf.Min(CurrentPlaybackPosition + Source.pitch * Time.deltaTime, AClip.length);

            //check to see if the time has expired
            if (CurrentPlaybackPosition == AClip.length)
            {
                //oh dear
                SoundExpired = true;
            }
            else
            {
                //just add to time playing
                TimePlaying += Time.deltaTime;
            }
        }
    }

    // get the playback percent of the clip at its current playback position
    public float GetPlaybackPercent()
    {
        return CurrentPlaybackPosition / AClip.length;
    }

    // get the volume of the sound attenuated for the distance from a position
    public float GetVolumeAttenuated(Vector3 Pos)
    {
        //get distance from emitter
        float Dist = Vector3.Distance(Source.transform.position, Pos);

        //calculate volume based on distance based on audio source attenuation falloff to give to the detector

        //TODO add support to follow custom attenuation curve

        //logorithmic falloff, this is the default setting
        float Volume = Mathf.Clamp(Source.volume / Dist, 0, 1);

        return Volume;

    }

    // get the actual volume of the sound at its current playback point
    public float GetCurrentPlaybackdB()
    {
        Debug.Log("Not Implemented!");
        return 0f;

    }    
}


public interface ISound
{
    //update
    public void FixedUpdate();


    //playback interface
    public void Play(float time, MixerAudioEffect[] liveEffects);
    //public void Pause();
    //public void UnPause();
    //public bool IsPaused();
    public bool IsPlaying();
    public void Stop();
    public float GetPlaybackPos();


    //creates and sends a fresh copy of that sound
    public ISound GetCopy();

    public void SetIsRecorded(bool value);
    public bool IsRecorded();


    public void SetEmissionSocket(Transform socket);


    //playback speed interface
    public void UpdateParentSpeed(float newSpeed);
    public void UpdateLocalSpeed(float newSpeed);

    public float GetLocalSpeed();
    public float GetSpeed();

    //playback volume interface
    public void UpdateParentVolume(float newVolume);
    public void UpdateLocalVolume(float newVolume);

    public float GetLocalVolume();
    public float GetVolume();

    //looping interface
    public void SetIsLooping(bool value);
    public bool IsLooping();


    


}


//container to hold data about a sound playing in the scene
public class Sound : ISound
{
    public float LocalSpeed { get; private set; } = 1f;
    public float ParentSpeed { get; private set; } = 1f;
    public float LocalVolume { get; private set; } = 1f;
    public float ParentVolume { get; private set; } = 1f;

    public bool Recorded = false;

    public bool Looping { get; private set; } = false;

    //the current playback position in the clip
    public float PlaybackPos { get; private set; } = 0f;
    public Transform OutputSocket { get; private set; } = null;

    public AudioClip Clip { get; private set; } = null;

    //reference to source used during audio playback of a clip
    public AudioSource Source { get; private set; } = null;


    public AudioMixerGroup EffectsInput { get; private set; } = null;




   

    public Sound(AudioClip clip)
	{
        Clip = clip;
        

	}

    public ISound GetCopy()
	{
        Sound s = new Sound(Clip);

        s.UpdateLocalVolume(LocalVolume);
        s.UpdateLocalSpeed(LocalSpeed);
        s.SetIsLooping(Recorded);
        s.SetIsRecorded(true);
        return s;
    }
        
    
    public void SetIsRecorded(bool value)
	{
        Recorded = value;
	}
    public bool IsRecorded()
	{
        return Recorded;
	}
    public void SetEffectsInput(AudioMixerGroup input)
	{
        EffectsInput = input;

        if (Source != null)
		{
            Source.outputAudioMixerGroup = EffectsInput;
		}
	}
    public void SetEmissionSocket(Transform socket)
	{
        OutputSocket = socket;

        //if we currently have a source rented, update its position and parent to the new socket
        if (Source != null)
		{
            Source.transform.position = OutputSocket.transform.position;
            Source.transform.parent = OutputSocket;
		}

    }

    public float GetPlaybackPos()
	{
        return PlaybackPos;
	}
    public void FixedUpdate()
	{

        //sync the playback position, the reason that we have this is because the value stored in
        // the sound does not reset until we play again, allowing us to know if a sound ended after its right time
        PlaybackPos = Source.time;
	}
    public void Play(float time, MixerAudioEffect[] liveEffects)
	{
        
        if (Clip != null && OutputSocket != null)
		{

            //rent our source from the manager
            Source = AudioManager.RentSource();

            if (Source != null)
            {
                //reset our playback position
                PlaybackPos = 0f;


                //stick the rented audio source to our emitting object
                Source.transform.position = OutputSocket.position;
                Source.transform.parent = OutputSocket;


                //set the sources clip
                Source.clip = Clip;

                //set speed and volume
                Source.pitch = GetSpeed();
                Source.volume = GetVolume();

                //set is looping
                Source.loop = Looping;

                //set output mixer group
                Source.outputAudioMixerGroup = EffectsInput;
                //play the source
                Source.Play();
                //set the time to the start time
                Source.time = time;

                //notify audio manager of emission if we have 
                AudioManager.NewSoundPlayed(this,liveEffects);
                //add it to active sounds
                AudioManager.GetActiveSounds().Add(this);
            }
		}


	}

    public void Stop()
	{
        //clip or track
        if (Source != null)
        {
            //stop playing the clip
            Source.Stop();
            Source.pitch = 1f;
            Source.volume = 1f;
            //remove this sound from the active sounds
            AudioManager.GetActiveSounds().Remove(this);

            //return the rented source to the manager
            AudioManager.ReturnSource(Source);

            //stop referencing the source
            Source = null;
        }


    }

    //done
    public void UpdateParentSpeed(float newSpeed)
    {
        //set our parent speed
        ParentSpeed = newSpeed;

        if (Source != null)
            Source.pitch = GetSpeed();

    }

    //done
    public void UpdateLocalSpeed(float newSpeed)
    {
        //set our local speed
        LocalSpeed = newSpeed;

        if (Source != null)
            Source.pitch = GetSpeed();
    }
    
    //done
    public float GetSpeed()
    {
        return LocalSpeed * ParentSpeed;

    }

    public float GetLocalSpeed()
	{
        return LocalSpeed;
	}

    public void UpdateParentVolume(float newVolume)
    {
        //set our parent volume
        ParentVolume = newVolume;

        if (Source != null)
            Source.volume = GetVolume();

    }

    //done
    public void UpdateLocalVolume(float newVolume)
    {
        //set our local volume
        LocalVolume = newVolume;

        if (Source != null)
            Source.volume = GetVolume();

    }
    public float GetVolume()
    {
        return LocalVolume * ParentVolume;
    }

    public float GetLocalVolume()
	{
        return LocalVolume;
	}
    public bool IsPlaying()
	{
        if (Source == null || Source.isPlaying == false)
		{
            return false;
		}

        return true;
	}

    public void SetIsLooping(bool value)
	{
        Looping = value;

        if (Source != null)
		{
            Source.loop = value;
		}
	}

    public bool IsLooping()
	{
        return Looping;
	}
}










static class AudioManager
{
    //static List<AudioInstance> m_AudioInstances = null;

    static List<AudioDetector> m_AudioDetectors = null;

    //the updater for the static class
    static UpdateAudioManager m_UpdateInstance = null;

    //the list of audio source objects that can be used to emit sound
    static List<AudioSource> m_AudioSourcesPool = null;

    
    static AudioMixer m_MasterMixer = null;
    
    //new system
    static List<ISound> m_ActiveSounds = null;

    public static List<ISound> GetActiveSounds()
	{
        return m_ActiveSounds;
	}

    //number of sources in the pool by default
    static int m_NumStartingSources = 10;

    public static void Start()
	{
        if (m_UpdateInstance == null)
        {
            Debug.Log("Audio Manager Started");
            //create update instance for the manager
            GameObject UpdateInstanceObj = new GameObject("Audio Manager Behavior");
            if (UpdateInstanceObj)
                m_UpdateInstance = UpdateInstanceObj.AddComponent<UpdateAudioManager>();

            //m_AudioInstances = new List<AudioInstance>();

            m_AudioSourcesPool = new List<AudioSource>();
            for (int i = 0; i < m_NumStartingSources; i++)
            {
                m_AudioSourcesPool.Add(CreateNewSourceObject());

            }
            


            m_ActiveSounds = new List<ISound>();
            m_AudioDetectors = new List<AudioDetector>();

            m_MasterMixer = Resources.Load<AudioMixer>("S_MasterMixer");
            if (m_MasterMixer == null)
			{
                Debug.LogError("Audio manager couldn't load master mixer!");
			}
        }
    }

    public static AudioMixerGroup GetMasterMixerGroup(string name)
	{
        AudioMixerGroup[] groups = m_MasterMixer.FindMatchingGroups(name);

        if (groups.Length == 1)
		{
            return groups[0];
		}

        return null;
	}

    static AudioSource CreateNewSourceObject()
	{
        Debug.Log("created source object!");
        //create our gameobject and make it inactive
        GameObject newObj = new GameObject("rental audio source");

        //create audio source component on our new object
        AudioSource a = newObj.AddComponent<AudioSource>();

        //setup our object
        newObj.SetActive(false);
        a.playOnAwake = false;
        a.spatialBlend = 1f;
        //its done!
        return a;
    }
    //called by a helping hand from inside the game!!!
    public static void FixedUpdate()
	{

        //check to see if there are any expired sounds
        for (int i = 0; i < m_ActiveSounds.Count; i++)
		{
            //if we are not playing, stop the sound
            if (m_ActiveSounds[i].IsPlaying() == false)
            {
                m_ActiveSounds[i].Stop();
                //go back one
                i--;
            }
            else
			{
                //update our sound if it is still playing
                m_ActiveSounds[i].FixedUpdate();
			}
        }
    }
    public static void NewSoundPlayed(ISound newSound, MixerAudioEffect[] liveEffects)
    {
        //notify all of our detectors
        foreach (AudioDetector d in m_AudioDetectors)
        {
            d.HandleNewSoundPlayed(newSound, liveEffects);
        }
    }


    //depreicated function, interface to new method
    public static AudioInstance EmitSound(AudioSource source, AudioClip clip, EAudioType[] types = null)
	{
        //PlaySound(source.transform, clip);
        return null;
	}

    //for now it will return the audio source of the rental object and will handle the pooling internally


    public static Sound PlaySound(Transform emissionSocket, AudioClip clip, MixerAudioEffect[] effects = null, float speed = 1f, float volume = 1f, float clipStartPoint = 0f)
	{
        //create our sound object
        Sound newSound = new Sound(clip);
    
        //set our socket
        newSound.SetEmissionSocket(emissionSocket);
    
        //link our effects
        if (effects != null)
            newSound.SetEffectsInput(effects[0].EffectMixerGroup);
    
        //change our speed and volume to their starting values
        newSound.UpdateLocalSpeed(speed);
        newSound.UpdateLocalVolume(volume);
    
        //play the sound
        newSound.Play(clipStartPoint,effects);
    
    
    
    
        return newSound;
	}


    // add a detector to the list of active detectors in the scene
    public static void AddDetector(AudioDetector NewDetector)
    {


        m_AudioDetectors.Add(NewDetector);
    }

    // remove a detector from the list of active detectors in the scene
    public static void RemoveDetector(AudioDetector NewDetector)
    {
        m_AudioDetectors.Remove(NewDetector);
    }


    public static AudioSource RentSource()
	{
        //if we have any in our pool
        if (m_AudioSourcesPool.Count > 0)
		{
            //grab one
            AudioSource s = m_AudioSourcesPool[m_AudioSourcesPool.Count - 1];
            //remove it from the list
            m_AudioSourcesPool.RemoveAt(m_AudioSourcesPool.Count - 1);
            //activate it
            s.gameObject.SetActive(true);

            s.outputAudioMixerGroup = null;
            //send it return it
            return s;
		}

        //otherwise just 

        //if we dont find an unused source, make a new one and return that
        AudioSource newSource = CreateNewSourceObject();
        newSource.gameObject.SetActive(true);
        return newSource;


	}

    public static void ReturnSource(AudioSource source)
	{

        source.outputAudioMixerGroup = null;
        source.gameObject.SetActive(false);

        //put it at the start just to be safe
        m_AudioSourcesPool.Insert(0, source);

	}


}
