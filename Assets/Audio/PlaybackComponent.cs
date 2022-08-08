using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackComponent : MonoBehaviour
{

    

    [SerializeField, Range(0, 2)]
    float m_PlaybackSpeed = 1f;

    float currentspeed = 1f;

    [SerializeField]
    AudioClip[] Clips = null;

    [SerializeField]
    AudioClip[] Clips2 = null;

    public RecordingTrack m_CurrentTrack { get; private set; } = null;




    private void Awake()
    {
        AudioManager.Start();



        ////create a sub track for a thing to playback in
        //List<RecordingTrackEvent> Events1 = new List<RecordingTrackEvent>();
        //
        //RecordingTrackEvent event1 = new RecordingTrackEvent(.25f, ERecordTrackEventType.SoundStart, 0, 0f);
        //Events1.Add(event1);
        //
        //
        //RecordingTrackEvent event2 = new RecordingTrackEvent(.5f, ERecordTrackEventType.SoundStop, 0, 0f);
        //Events1.Add(event2);
        //
        //
        //RecordingTrackEvent event3 = new RecordingTrackEvent(.75f, ERecordTrackEventType.SoundStart, 1, 0f);
        //Events1.Add(event3);
        //
        //RecordingTrackEvent event4 = new RecordingTrackEvent(1.2f, ERecordTrackEventType.SoundStop, 1, 0f);
        //Events1.Add(event4);
        //
        //RecordingTrack[] subTrack = { new RecordingTrack(Clips2, new RecordingTrack[0], null, Events1.ToArray(), 1.2f) };
        //
        ////our main track
        //
        //List<RecordingTrackEvent> Events = new List<RecordingTrackEvent>();
        //
        //RecordingTrackEvent Event1 = new RecordingTrackEvent(.1f,ERecordTrackEventType.SoundStart,0,0f);
        //
        //Events.Add(Event1);
        //
        //RecordingTrackEvent Event2 = new RecordingTrackEvent(3f, ERecordTrackEventType.SoundStart, 1, 0f);
        //
        //Events.Add(Event2);
        //
        //RecordingTrackEvent Event3 = new RecordingTrackEvent(3.5f, ERecordTrackEventType.SoundStop, 1, 0f);
        //
        //Events.Add(Event3);
        //
        //RecordingTrackEvent Event4 = new RecordingTrackEvent(3.8f, ERecordTrackEventType.SoundStart, 0, 0f,true);
        //
        //Events.Add(Event4);
        //
        //m_CurrentTrack = new RecordingTrack(Clips,subTrack, null, Events.ToArray(),6f);





    }

	private void Start()
	{
        //initalize our track for playback
        //m_CurrentTrack.Init();
        //set our emission socket to this gameobject
        //m_CurrentTrack.SetEmissionSocket(transform);
        
	}

	private void Update()
	{
        //input temp for now
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    if (m_CurrentTrack != null && m_CurrentTrack.Playing == false)
        //    {
        //        Play();
        //    }
        //    else
        //    {
        //        Stop();
        //    }
        //}

        float value = 0f;
        if (Input.GetKey(KeyCode.K))
		{
            value -= 1f;
		}
        if (Input.GetKey(KeyCode.L))
        {
            value += 1f;
        }

        m_PlaybackSpeed = Mathf.Clamp(m_PlaybackSpeed + value * Time.deltaTime, 0, 2);
    }
	void FixedUpdate()
    {
        if (currentspeed != m_PlaybackSpeed)
        {
            currentspeed = m_PlaybackSpeed;
            m_CurrentTrack.UpdateParentSpeed(currentspeed);
        }

        
    }

    public void Play()
    {
        if (m_CurrentTrack != null)
        m_CurrentTrack.Play(0,null);
    }

    public void Stop()
    {
        if (m_CurrentTrack != null)
        m_CurrentTrack.Stop();
    }


    public void UpdateCurrentTrack(RecordingTrack newTrack)
	{

        if (m_CurrentTrack != null)
		{
            Stop();
        }

        m_CurrentTrack = newTrack;

        if (m_CurrentTrack != null)
        m_CurrentTrack.SetEmissionSocket(transform);


    }
}
