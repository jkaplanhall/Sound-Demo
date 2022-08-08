using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RecordComponent),typeof(PlaybackComponent))]
public class TapeRecorder : MonoBehaviour
{
    RecordComponent RComp;
    PlaybackComponent PComp;


    [SerializeField]
    private InteractableButton m_PlayButton;

    [SerializeField]
    private InteractableButton m_StopButton;

    [SerializeField]
    private InteractableButton m_RecButton;


    void Awake()
    {
        RComp = GetComponent<RecordComponent>();
        PComp = GetComponent<PlaybackComponent>();

        if (m_PlayButton != null)
        {
            m_PlayButton.OnPressed.AddListener(this.OnPlayPressed);
        }

        if (m_StopButton != null)
        {
            m_StopButton.OnPressed.AddListener(this.OnStopPressed);
        }

        if (m_RecButton != null)
        {
            m_RecButton.OnPressed.AddListener(this.OnRecPressed);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if we are playing and the track stops
        if (RComp.b_IsRecording == false && m_PlayButton.Pressed == true && PComp.m_CurrentTrack != null && PComp.m_CurrentTrack.Playing == false)
		{
            m_PlayButton.Pressed = false;
            m_StopButton.Pressed = true;
		}
    }


    public void OnPlayPressed()
    {
        if (m_PlayButton.Pressed == false)
        {

            //if the recording button is pressed, start recording
            if (m_RecButton.Pressed == true)
            {
                RComp.StartRecording();
            }
            //otherwise playback the recording
            else
            {
                PComp.Play();
            }

            //set buttons
            m_PlayButton.Pressed = true;
            m_StopButton.Pressed = false;
        }
    }
    public void OnStopPressed()
    {
        if (m_StopButton.Pressed == false)
		{
            //if the recording button is pressed, stop recording
            if (m_RecButton.Pressed == true)
			{
                RComp.StopRecording();

                //release the record button
                m_RecButton.Pressed = false;
                //pass the clip from the record component to the playback component
                PComp.UpdateCurrentTrack(RComp.CurrentTrack);

			}
            //otherwise stop the playback
            else
			{
                PComp.Stop();
			}

            //set buttons
            m_StopButton.Pressed = true;
            m_PlayButton.Pressed = false;


		}
    }

    public void OnRecPressed()
    {
        if (m_StopButton.Pressed == true)
        m_RecButton.Pressed = !m_RecButton.Pressed;
    }


}
