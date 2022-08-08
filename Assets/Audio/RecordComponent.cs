/*
Author: Jacob Kaplan-Hall
Comment/Description: allows gameobjects to record and playback audio
ChangeLog: d/m/y
1/15/22 created document and linked interfaces
2/12/22 sound rework complete
*/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioDetector))]
public class RecordComponent : MonoBehaviour, IAudioDetectionCallback
{



    public bool b_IsRecording { get; private set; } = false;

    public RecordingTrack CurrentTrack { get; private set; } = null;

    //buffers to store data in while recording tracks
    List<List<ISound>> m_SoundBuffers = null;



    List<MixerAudioEffect[]> m_EffectBuffers = null;
    List<float[]> m_EffectValueBuffers = null;



    List<List<RecordingTrackEvent>> m_EventBuffers = null;

    //sounds being monitored for changes
    List<List<ISound>> m_MonitoredSounds = null;


    //current time we've been recording in seconds
    float m_RecTime = 0f;



    // Update is called once per frame
    void FixedUpdate()
    {
        //if we are recording, update our recording time
        if (b_IsRecording)
		{
            m_RecTime += Time.fixedDeltaTime;

            //TODO every frame check the status of our effects and write any changes to the corrisponding buffers
            //for (int i = 0; i < m_EffectBuffers.Count; i++)
			//{
            //    
            //    for (int a = 0; a < m_EffectBuffers[i].Length; a++)
			//	{
            //        if (m_EffectBuffers[i][a].GetEffectPercent() != m_EffectValueBuffers[i][a])
			//		{
            //            m_EffectValueBuffers[i][a] = m_EffectBuffers[i][a].GetEffectPercent();
            //            m_EventBuffers[i].Add(new RecordingTrackEvent(m_RecTime, ERecordTrackEventType.EffectUpdate, a, m_EffectBuffers[i][a].GetEffectPercent()));
			//		}
			//	}
            //
            //}

                //this loop goes over every sound buffer we have that we have heard so far and
                //will check which ones have stopped playing, either from the manager stopping them,
                //or from another source like a playback script

                for (int i = 0; i < m_MonitoredSounds.Count; i++)
			{
                for (int a = 0; a < m_MonitoredSounds[i].Count; a++)
				{
                    ISound s = m_MonitoredSounds[i][a];
                    
                    //if its not a blank spot (stopped playing)
                    if (s != null)
					{
                        if (s.IsPlaying() == false)
                        {

                            //add a stop event to our event buffer for that track
                            CreateInitalRecordingEvents(i, a, s, true);
                            //set the monitored sound spot to null
                            m_MonitoredSounds[i][a] = null;
                            a--;
                            //we're done with this one
                            continue;
                        }
                        else
						{
                            //we are storing the data from the frame before in the sound we rented out for later,
                            //reduce, reuse, recycle...
                            ISound b = m_SoundBuffers[i][a];

                            if (s.GetLocalVolume() != b.GetLocalVolume())
                            {
                                //record the change in the buffer and make an event
                                b.UpdateLocalVolume(s.GetLocalVolume());
                                m_EventBuffers[i].Add(new RecordingTrackEvent(m_RecTime, ERecordTrackEventType.VolumeUpdate, a, s.GetLocalVolume()));
                            }

                            if (s.GetLocalSpeed() != b.GetLocalSpeed())
                            {
                                //record the change in the buffer and make an event
                                b.UpdateLocalSpeed(s.GetLocalSpeed());
                                m_EventBuffers[i].Add(new RecordingTrackEvent(m_RecTime, ERecordTrackEventType.SpeedUpdate, a, s.GetLocalSpeed()));
                            }

                            if (s.IsLooping() != b.IsLooping())
                            {
                                //record the change in the buffer and make an event
                                b.SetIsLooping(s.IsLooping());
                                m_EventBuffers[i].Add(new RecordingTrackEvent(m_RecTime, ERecordTrackEventType.LoopingUpdate, a, s.IsLooping() ? 1 : 0));
                            }

                        }

                        





                    }


            

                }
			}


		}
    }

    public void StartRecording()
	{
        Debug.Log("Start Recording");
        b_IsRecording = true;
        //reset rec time
        m_RecTime = 0f;
        //bye bye old track
        CurrentTrack = null;


        //clear our buffers
        m_SoundBuffers = new List<List<ISound>>();
        //TrackBuffer = new List<RecordingTrack>();
        m_EffectBuffers = new List<MixerAudioEffect[]>();
        m_EffectValueBuffers = new List<float[]>();
        m_EventBuffers = new List<List<RecordingTrackEvent>>();
        m_MonitoredSounds = new List<List<ISound>>();

        //we're gonna create our first buffer for the base track at index 0
        m_EffectBuffers.Add(null);
        m_EffectValueBuffers.Add(null);
        m_SoundBuffers.Add(new List<ISound>());
        m_EventBuffers.Add(new List<RecordingTrackEvent>());
        m_MonitoredSounds.Add(new List<ISound>());

        //TODO look at the audio managers currently palying sounds to get them
    }

    public void StopRecording()
	{
        Debug.Log("Stop Recording");
        b_IsRecording = false;

        //we need to create our new recording track. if there are sounds without any effects
        //put those on the main track, if there are any other effect stacks detected, put the sounds
        //played through those stacks in sub tracks within the main track

        //lets make the subtracks first and while we're at it, check to see if we havea 

        //make subtracks for all tracks with effects
        List<RecordingTrack> subTracks = new List<RecordingTrack>();



        //we will use these command buffers to indicate to the base track to play the subtracks
        List<RecordingTrackEvent> StartEvents = new List<RecordingTrackEvent>();

        for (int i = 0; i < m_EffectBuffers.Count; i++ )
		{

            if (m_EffectBuffers[i] == null)
                continue;

            //time to generate our list of effect names from our stack and create a subtrack

            //grab our effects list
            MixerAudioEffect[] effects = m_EffectBuffers[i];
            //make our names list
            string[] names = new string[effects.Length];


            for (int a = 0; a < effects.Length; a++)
			{
                //record the effect names into a list
                names[a] = effects[a].EffectName;
			}

            //create our subtrack
            subTracks.Add(new RecordingTrack(m_SoundBuffers[i].ToArray(), names, m_EventBuffers[i].ToArray(), m_RecTime));

            //add an event to make the track start at the start of the base clip
            StartEvents.Add(new RecordingTrackEvent(0f, ERecordTrackEventType.SoundStart, subTracks.Count - 1, 0));




		}

        //add our events to call the tracks to play to the start of the list of events
        RecordingTrackEvent[] events = StartEvents.ToArray().Concat(m_EventBuffers[0].ToArray()).ToArray();
        //once we've populated our subtracks and start event buffer, we are ready to add the subtracks
        RecordingTrack mainTrack = new RecordingTrack(m_SoundBuffers[0].ToArray(), null, events, m_RecTime);




        //we've now created our main track!!
        //next we have to initalize it

        mainTrack.Init();

        //we're all set

        CurrentTrack = mainTrack;

        //we are done! send it over to our playback component
        //PlaybackComp.UpdateCurrentTrack(CurrentTrack);
	}

    //this function is resonsible for looking at an incomming sound, creating the nessisary start events for that point,
    //and create new sub track if that sound has unique effects, and if so record the inital events for those effects
    public void OnNewSoundPlayed(ISound newSound, MixerAudioEffect[] liveEffects)
	{
        
        //we only want to record sounds that are original, we will use a different method for handling recursive recording
        if (b_IsRecording)
		{

            //we want to capture and interperate these as recording commands, lets get to work i guess

            //first we need to check to see if we already have a track going for this set of effects

            for (int i = 0; i < m_EffectBuffers.Count; i++)
            {
                //we will compare the sounds effects buffer to all the ones we have so far
                if (liveEffects == m_EffectBuffers[i])
				{
                    //if this happens then it means that the sound we just heard can be added to an existing track


                    //lets create a new sound that is a copy of this sound and
                    //add the copy to our buffer
                    m_SoundBuffers[i].Add(newSound.GetCopy());
                    
                    m_MonitoredSounds[i].Add(newSound);

                    //create the recording events to update all playback properties to their inital states
                    CreateInitalRecordingEvents(i,m_SoundBuffers[i].Count - 1, newSound);

                    //were done after we record the event to the existing track
                    return;
				}
            }

            //we need to create a new "sub track" for this sound because we havent heard this effects stack yet
            List<ISound> newSoundBuffer = new List<ISound>();

            //add a copy of the sound we just heard to the sound buffer
            newSoundBuffer.Add(newSound.GetCopy());
            //add our new clip buffer to the buffer list
            m_SoundBuffers.Add(newSoundBuffer);

            //add our effects to the effects buffer list
            m_EffectBuffers.Add(liveEffects);
            float[] effectValues = new float[liveEffects.Length];
            m_EffectValueBuffers.Add(effectValues);

            //create a new event buffer
            List<RecordingTrackEvent> newEventBuffer = new List<RecordingTrackEvent>();

            //add our new event buffer 
            m_EventBuffers.Add(newEventBuffer);



            //record inital values for effects
            for (int i = 0; i < liveEffects.Length; i++)
			{
                MixerAudioEffect m = liveEffects[i];
                //load inital values
                effectValues[i] = m.EffectPercent;
                newEventBuffer.Add(new RecordingTrackEvent(m_RecTime, ERecordTrackEventType.EffectUpdate, i, m.EffectPercent));
			}





            //make a new buffer of monitored sounds
            List<ISound> newMonitoredSoundBuffer = new List<ISound>();
            //add the sound we heard to the buffer
            newMonitoredSoundBuffer.Add(newSound);
            //add our buffer to the list of buffers
            m_MonitoredSounds.Add(newMonitoredSoundBuffer);

            //create the recording events to update all modified playback properties to their inital states
            CreateInitalRecordingEvents(m_SoundBuffers.Count - 1,m_SoundBuffers[m_SoundBuffers.Count - 1].Count - 1, newSound);


        }

	}





    void CreateInitalRecordingEvents(int index, int soundIndex, ISound newSound, bool soundStop = false)
	{
        //grab the buffer we are working on
        List<RecordingTrackEvent> buffer = m_EventBuffers[index];

        //first, lets do pitch and volume


        //TODO HAZARD, we dont know how we are gonna read the volume for sure yet
        //oh boy, someone has the pitch changed right off the bat, thats terrible!

        //add the new event to our event buffer
        buffer.Add(new RecordingTrackEvent(m_RecTime, ERecordTrackEventType.VolumeUpdate, soundIndex, newSound.GetLocalVolume()));



        //add the new event to our event buffer
        buffer.Add(new RecordingTrackEvent(m_RecTime, ERecordTrackEventType.SpeedUpdate, soundIndex, newSound.GetLocalSpeed()));



        buffer.Add(new RecordingTrackEvent(m_RecTime, ERecordTrackEventType.LoopingUpdate, soundIndex, 1));



        //add a sound start event to the buffer
        Sound sound = newSound as Sound;
        if (sound != null)
        {
            buffer.Add(new RecordingTrackEvent(m_RecTime, soundStop ? ERecordTrackEventType.SoundStop : ERecordTrackEventType.SoundStart, soundIndex, newSound.GetPlaybackPos()));
        }
        else
        {
            RecordingTrack t = newSound as RecordingTrack;
            if (t != null)
            {
                int snapIndex = t.TakeSnapshot();

                buffer.Add(new RecordingTrackEvent(m_RecTime, soundStop ? ERecordTrackEventType.SoundStop : ERecordTrackEventType.SoundStart, soundIndex, (float)snapIndex));

            }
        }

        //what we are in the middle of figuring out is what need to be added by the inital events function
        //vs what is stored inside of a snapshot
        //i think the answer is: make the inital events function handle making the snapshot in the case of a subtrack
        //the information for THAT TRACKS playback propery updates is held in the parent track
    }

}






public struct TrackSnapshot
{

    public TrackSnapshot(float time,int index, RecordingTrackEvent[] events)
	{
        Time = time;
        Index = index;
        Events = events;
	}
    
    public float Time;
    public int Index;
    public RecordingTrackEvent[] Events;



}








public class RecordingTrack : ISound
{

    //Data held in the recording track

    //all clips that play in the track
    //AudioClip[] Clips = null;
    
    
    //RecordingTrack[] SubTracks = null;

    ISound[] Sounds = null;

    //effects in the track
    string[] EffectsList = null;

    //recording events in the track
    RecordingTrackEvent[] Events = null;

    //snapshots used to start clip playback at a specified point
    List<TrackSnapshot> Snapshots = null;

    //lenght in seconds of the track
    float Lenght = 0f;

    bool Recorded = false;



    //Playback stuff
    //playback position in timeline
    public float PlaybackPos { get; private set; } = 0f;

    //index of playback event we are on
    public int PlaybackIndex { get; private set; } = 0;
    //local playback speed of the clip
    public float LocalSpeed { get; private set; } = 1f;

    //the actual playback speed of the clip within a recorded context
    public float ParentSpeed { get; private set; } = 1f;


    public float LocalVolume { get; private set; } = 1f;

    public float ParentVolume { get; private set; } = 1f;

    public Transform EmissionSocket { get; private set; } = null;

    public bool Playing { get; private set; } = false;

    //array referencing the sounds playing from this clip during playback
    //public Sound[] CurrentSounds { get; private set; } = null;

    //array referencing all effects stacked onto audio on this clip
    public MixerAudioEffect[] StackedEffects { get; private set; } = null;
    //array referencing the effects loaded for this track layer during playback
    public MixerAudioEffect[] LocalEffects { get; private set; } = null;
    //the "input" for our audio sources to be put into the effects circuit
    public MixerAudioEffect InputMixer = null;


    public MixerAudioEffect[] ParentEffects { get; private set; } = null;

    private MixerAudioEffect[] LiveEffects = null;
    public RecordingTrack(ISound[] sounds, string[] effectslist, RecordingTrackEvent[] events, float lenght)
	{
        Sounds = sounds;

        //Clips = clips;
        //SubTracks = subTracks;
        EffectsList = effectslist;
        Events = events;
        Lenght = lenght;

        //rent out an array for our sounds the size of the number of clips in our recording
        //CurrentSounds = new Sound[Clips.Length];

        //create our snapshots array
        Snapshots = new List<TrackSnapshot>();
        //add the snapshots that represent the start
        Snapshots.Add(new TrackSnapshot(0,0, new RecordingTrackEvent[0]));
    }

    public ISound GetCopy()
	{
        //create a list of fresh sounds
        ISound[] newSounds = new ISound[Sounds.Length];

        for (int i = 0; i < Sounds.Length; i++)
		{
            newSounds[i] = Sounds[i].GetCopy();
		}

        //create our sounds list

        string[] newEffects = new string[LocalEffects.Length];

        for (int i = 0; i < LocalEffects.Length; i++)
        {
            newEffects[i] = LocalEffects[i].EffectName;
        }

        RecordingTrack t = new RecordingTrack(newSounds, newEffects, Events, Lenght);
        t.SetIsRecorded(true);

        return t;
	}

    public bool IsRecorded()
	{
        return Recorded;
	}

    public void SetIsRecorded(bool value)
	{
        Recorded = value;
	}

    public float GetPlaybackPos()
	{
        return PlaybackPos;
	}

    //takes a snapshot and re
    public int TakeSnapshot()
	{
        
        List<RecordingTrackEvent> snapshotEvents = new List<RecordingTrackEvent>();

        for (int i = 0; i < Sounds.Length; i++)
		{
            ISound sound = Sounds[i];

            if (sound.IsPlaying())
			{
                //this sound is playing which means we need to record its properties at this point for the snapshot
                snapshotEvents.Add(new RecordingTrackEvent(PlaybackPos, ERecordTrackEventType.LoopingUpdate, i, sound.IsLooping() ? 1 : 0));
                snapshotEvents.Add(new RecordingTrackEvent(PlaybackPos, ERecordTrackEventType.SpeedUpdate, i, sound.GetLocalSpeed()));
                snapshotEvents.Add(new RecordingTrackEvent(PlaybackPos, ERecordTrackEventType.VolumeUpdate, i, sound.GetLocalVolume()));

                
                Sound s = sound as Sound;
                if (sound != null)
                {
                    //if its a normal sound, just tell it to play at the position it is right now
                    snapshotEvents.Add(new RecordingTrackEvent(PlaybackPos, ERecordTrackEventType.SoundStart, i, s.PlaybackPos));
                }
                else
                {
                    RecordingTrack t = sound as RecordingTrack;
                    if (t != null)
                    {
                        //if its a sub track, we have to take a snapshot and use the index of that snapshot as the input
                        snapshotEvents.Add(new RecordingTrackEvent(PlaybackPos, ERecordTrackEventType.SoundStart, i, (float)t.TakeSnapshot()));
                    }
                }


            }
		}

        //record all effect states
        for (int i = 0; i < LocalEffects.Length; i++)
		{
            if (LocalEffects[i] != null)
                snapshotEvents.Add(new RecordingTrackEvent(PlaybackPos, ERecordTrackEventType.EffectUpdate, i, LocalEffects[i].GetEffectPercent()));
		}

       
        TrackSnapshot snap = new TrackSnapshot(PlaybackPos,PlaybackIndex, snapshotEvents.ToArray());

        Snapshots.Add(snap);
        return Snapshots.Count - 1;
	}
    public void Init()
	{
        //rent and link up effects in mixers
        InitMixerEffects();
        
        //once mixers are set, build effect stacks for sound transmission
        SetupEffectStacks(new MixerAudioEffect[0]);


        //link the sounds in our track to the input for that track
        LinkSounds();
        //create our sound instances within the track
        //CreateSounds();
    }

    public void LinkSounds()
	{
        //go through all of our sounds
        //if its a subtrack: call this function
        //if its a sound: link the sounds output to the tracks input
        foreach (ISound s in Sounds)
        {
            Sound sound = s as Sound;
            if (sound != null)
            {

                sound.SetEffectsInput(InputMixer.EffectMixerGroup);
            }
            else
			{
                RecordingTrack track = s as RecordingTrack;
                if (track != null)
				{
                    track.LinkSounds();
				}
			}

        }
    }
    //public void CreateSounds()
	//{
    //    foreach (RecordingTrack t in SubTracks)
    //    {
    //        //have all sub tracks do the same
    //        t.CreateSounds();
    //    }
    //
    //    for (int i = 0; i < Clips.Length; i++)
	//	{
    //        //use the clips from our array to make the sound, and feed it the stacked effects so detectors have access to them
    //        CurrentSounds[i] = new Sound(Clips[i], true, StackedEffects);
    //        //set our sound to use the mixers we have so kindly set up for it
    //        CurrentSounds[i].SetEffectsInput(InputMixer.EffectMixerGroup);
	//	}
    //
    //
	//}
    public void SetEmissionSocket(Transform socket)
	{
        EmissionSocket = socket;
        foreach (ISound s in Sounds)
		{
            s.SetEmissionSocket(EmissionSocket);
		}
	}
    //done
    public void InitMixerEffects()
	{
        
        foreach (ISound s in Sounds)
        {
            RecordingTrack t = s as RecordingTrack;
            if (t != null)
            {
                //init mixers in our sub tracks
                t.InitMixerEffects();
            }

        }

        //rent out all the effects
        if (EffectsList != null)
        {
            LocalEffects = new MixerAudioEffect[EffectsList.Length];

            for (int i = 0; i < EffectsList.Length; i++)
            {
                //wow it really was that easy wasn't it
                //populate the array of effects
                LocalEffects[i] = MixerManager.RentMixer(EffectsList[i]);

                if (LocalEffects[i] == null)
                {
                    Debug.LogError("couldnt rent mixer");
                    return;
                }
            }
        }
        else
		{
            //just create an empty array
            LocalEffects = new MixerAudioEffect[0];
		}



        //rent our input mixer
        InputMixer = MixerManager.RentMixer("Empty");

        if (InputMixer == null)
        {
            Debug.LogError("couldnt rent mixer");
            return;
        }


        //make our last effect and set it to the InputMixer first
        MixerAudioEffect lastEffect = InputMixer;

        //go thourgh our list and link each effect
        
        
        for (int i = 0; i < LocalEffects.Length; i++)
        {


            //link our last effect to the next
            lastEffect.EffectMixer.outputAudioMixerGroup = LocalEffects[i].EffectMixerGroup;

            //set this effect to the last one
            lastEffect = LocalEffects[i];


        }

        //we need to link the mixers from our sub tracks to the input of this track
        foreach (ISound s in Sounds)
        {
            RecordingTrack t = s as RecordingTrack;
            if (t != null)
            {
                //link effects to this input mixer
                t.LinkEffects(InputMixer);
            }

        }
        

    }

    //done
    public void SetupEffectStacks(MixerAudioEffect[] parentMixerStack)
	{
        //add our stacked effects together with our local effects
        StackedEffects = LocalEffects.Concat(parentMixerStack).ToArray();


        //record our parent effects stack
        ParentEffects = parentMixerStack;

        //now that our effects stack is set up, set up children
        foreach (ISound s in Sounds)
        {
            RecordingTrack t = s as RecordingTrack;
            if (t != null)
            {
                //give it our effects stack to apply to its children
                t.SetupEffectStacks(StackedEffects);
            }
        }
    }
    public void Play(float time, MixerAudioEffect[] liveEffects)
	{
        //make sure we're initalized
        if (InputMixer != null)
        {
            Debug.Log("Track Playback Started");
            //read our track snapshot from the index
            TrackSnapshot snap = Snapshots[(int)time];

            //execute all the snapshot events
            foreach (RecordingTrackEvent e in snap.Events)
			{
                ProcessEvent(e);
			}


            //set the playback pos and index to the correct values
            PlaybackPos = snap.Time;
            PlaybackIndex = snap.Index;




            Playing = true;


            //set our live effects reference
            LiveEffects = liveEffects;

            //notify audio manager of emission if we have 
            AudioManager.NewSoundPlayed(this, liveEffects);
            //add it to active sounds
            AudioManager.GetActiveSounds().Add(this);


        }
	}

    public void FixedUpdate()
	{
        //if we are playing
        if (Playing)
		{

            //if we have not gone through all our events and
            //look at the next event in the list and see if its time to trigger yet
            while (PlaybackIndex < Events.Length && Events[PlaybackIndex].EventTime < PlaybackPos)
			{
                //this is where we have to execute track event commands
                RecordingTrackEvent E = Events[PlaybackIndex];

                //execute the event
                ProcessEvent(E);


                Debug.Log(E.EventType + ", " + PlaybackIndex);
                //playback index goes up
                PlaybackIndex++;
			}
            


            //tick time after we process all the events for this time tick
            PlaybackPos = Mathf.Min(PlaybackPos + Time.fixedDeltaTime * GetSpeed(), Lenght);

            if (PlaybackPos >= Lenght)
			{
                //stop ourselves if we are done
                Stop();
			}
		}
	}

    public void ProcessEvent(RecordingTrackEvent E)
	{
        switch (E.EventType)
        {
            case ERecordTrackEventType.SoundStart:
                    Sounds[E.ItemIndex].Play(E.Value,LiveEffects);
                break;
            case ERecordTrackEventType.SoundStop:
                    Sounds[E.ItemIndex].Stop();
                break;
            case ERecordTrackEventType.SpeedUpdate:
                    Sounds[E.ItemIndex].UpdateLocalSpeed(E.Value);
                break;
            case ERecordTrackEventType.VolumeUpdate:
                    Sounds[E.ItemIndex].UpdateLocalVolume(E.Value);
                break;
            case ERecordTrackEventType.LoopingUpdate:
                Sounds[E.ItemIndex].SetIsLooping(E.Value == 1 ? true : false);
                break;
            case ERecordTrackEventType.EffectUpdate:
                LocalEffects[E.ItemIndex].SetEffectPercent(E.Value);
                break;



        }
    }
    public void Stop()
	{
        //not playing
        Playing = false;
        Debug.Log("Track Playback Stopped");



        //clear our references to the effects from the last play event
        LiveEffects = null;

        AudioManager.GetActiveSounds().Remove(this);
        //stop all the sounds playing in our sub tracks
        foreach (ISound s in Sounds)
		{
            s.Stop();
		}


	}

    public void LinkEffects(MixerAudioEffect input)
	{
        //basically if we have effects it will link the last effect to input
        //and if we dont it will just link its input to the next

        //if we have effects link the last effect in the chain
        if (LocalEffects.Length > 0)
		{
            //grab our last effect
            MixerAudioEffect ToLink = LocalEffects[LocalEffects.Length - 1];
            if (ToLink != null)
			{
                //and link its output to the next clip input
                ToLink.EffectMixer.outputAudioMixerGroup = input.EffectMixerGroup;
			}
		}
        else if (InputMixer != null)
		{
            //if we dont have any effects just link the input's output to the new input
            InputMixer.EffectMixer.outputAudioMixerGroup = input.EffectMixerGroup;
		}
	}
    public void UpdateParentSpeed(float newSpeed)
	{
        //set our parent speed
        ParentSpeed = newSpeed;

        //refresh our speed to sounds and subclips
        RefreshSpeedValue();
    }

    public void UpdateLocalSpeed(float newSpeed)
	{
        //set our local speed
        LocalSpeed = newSpeed;

        //refresh our speed to sounds and subclips
        RefreshSpeedValue();

    }
    void RefreshSpeedValue()
	{
        float Speed = GetSpeed();

        //update the speed for all the sounds in this clip
        foreach (ISound s in Sounds)
        {
            s.UpdateParentSpeed(Speed);
        }

    }
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

        //refresh our speed to sounds and subclips
        RefreshVolumeValue();
    }

    //done
    public void UpdateLocalVolume(float newVolume)
    {
        //set our local volume
        LocalVolume = newVolume;

        //refresh our volume to sounds and subclips
        RefreshVolumeValue();

    }
    //done
    void RefreshVolumeValue()
    {
        float Volume = GetVolume();

        //update the volume for all the sounds in this clip
        foreach (ISound s in Sounds)
        {
            s.UpdateParentVolume(Volume);
        }

    }
    //done
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
        return Playing;
	}

    public void SetIsLooping(bool value)
	{

	}

    public bool IsLooping()
	{
        //we cannot!
        return false;
	}

}










public enum ERecordTrackEventType
{
    None = 0, //darn, what have you done? you've not initalized!
    SoundStart, //value is the start offset in seconds, or the index of the snapshot in the case of a track
    SoundStop, //value is the end offset in seconds (time the clip ends at, if the clip was not cut short, this number will be the lenght of the clip in seconds)
    SpeedUpdate, //value is the new playback speed
    VolumeUpdate, //value is the new volume
    LoopingUpdate,// value is 1 for true and 0 for false
    EffectUpdate, //value is the new value for the effect
}

public class RecordingTrackEvent
{


    //time in the recording the event takes place
    public float EventTime { get; private set; } = 0f;

    //type of event taking place
    public ERecordTrackEventType EventType { get; private set; } = ERecordTrackEventType.None;

    //array index of the sound, track, or effect we are referencing
    public int ItemIndex { get; private set; } = 0;

    //the value of the event, see type declearation above to know what this means for each event type
    public float Value { get; private set; } = 1f;

    public RecordingTrackEvent(float time, ERecordTrackEventType eventType, int index, float value)
    {
        //time the event takes place in the recording track
        EventTime = time;
        //event type
        EventType = eventType;
        //item index
        ItemIndex = index;

        //the value of the event
        Value = value;

    }
}
