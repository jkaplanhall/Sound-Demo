/*
 * Author: Jacob Kaplan-Hall
 * Discription: source component that applies effects by default
 * ChangeLog: d/m/y
 * 3/10/22: created document
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class CEffectSource : CAudioCircuit, ISource
{
    //the current emission point of a circuit
    protected Transform m_CurrentEmissionSocket = null;

    [SerializeField]
    protected AudioClip m_ClipToPlay;


    [SerializeField]
    protected InteractableButton m_PlayButton = null;

    [SerializeField]
    protected InteractableButton m_StopButton = null;

    protected Sound m_SoundPlaying = null;

    protected Sound[][] m_EffectSoundsPlaying = null;


    [SerializeField]
    protected string[] m_EffectsToLoad;
    //list of effects the source has applied to the sound

    protected List<MixerAudioEffect> m_Effects = null;

    protected List<CAudioCircuit> m_CurrentComponents = null;

    // Start is called before the first frame update
    new protected void Awake()
    {
        base.Awake();
        CircuitType = EAudioCircuitType.Source;
        m_CurrentComponents = new List<CAudioCircuit>();




        //hook up our buttons
        if (m_PlayButton != null)
            m_PlayButton.OnPressed.AddListener(this.OnPlayButtonPressed);
        if (m_StopButton != null)
            m_StopButton.OnPressed.AddListener(this.OnStopButtonPressed);
    }

	public void Start()
	{
        base.Awake();
        MixerManager.Init();

        m_Effects = new List<MixerAudioEffect>();
        foreach(string s in m_EffectsToLoad)
		{
            MixerAudioEffect m = MixerManager.RentMixer(s);
            if (s != null)
			{
                m_Effects.Add(m);
			}
		}
        //we have a new mixer to link in the effects chain!
        AudioMixer LastMixer = null;
        foreach (MixerAudioEffect m in m_Effects)
		{
            if (LastMixer != null)
            {
                LastMixer.outputAudioMixerGroup = m.EffectMixerGroup;
            }
            //last set the last mixer to the current mixer
            LastMixer = m.EffectMixer;
        }



        //hook the output to the mixer to the SFX output
        if (LastMixer != null)
            LastMixer.outputAudioMixerGroup = AudioManager.GetMasterMixerGroup("SFX");

    }
	public void TryMakeCircuit()
    {

        //make a list
        List<CAudioCircuit> searchList = new List<CAudioCircuit>();
        //search forward in the circuit and gather everything that we find
        RecursiveGatherForward(searchList, this);

        //if the circuit at the end is a speaker then we can make a circuit
        CAudioCircuit speaker = searchList[searchList.Count - 1];
        if (speaker.CircuitType == EAudioCircuitType.Speaker)
        {
            MakeCircuit(searchList, speaker.transform);
        }


    }
    //effects must be given in the order they are to be applied, so usually in order from the source to the speaker
    public void MakeCircuit(List<CAudioCircuit> Components, Transform emissionSocket)
    {

        //check our inputs
        if (Components == null || emissionSocket == null)
        {
            Debug.LogError(name + " cant make circuit");
            return;
        }



        //set the location of output for audio emission
        m_CurrentEmissionSocket = emissionSocket;

        //set the reference to our components so we can release the control links to the effects when we disconnect the circuit
        m_CurrentComponents = Components;

        //go through and link the mixers in each circuit effect together

        foreach (CAudioCircuit circuit in Components)
        {

            if (circuit.IsModifier())
            {
                //grab a mixer effect from our mixer manager
                MixerAudioEffect m = null;
                foreach (MixerAudioEffect ef in m_Effects)
				{
                    if (ef.EffectName == circuit.GetEffectName())
					{
                        m = ef;
                        break;
					}
                }

                //if we got one
                if (m != null)
                {

                    //set the control callback of our effect to the control on the mixer
                    IControl control = circuit.GetControl();
                    if (control != null)
                    {
                        //set the control callback reference
                        control.SetControlCallback(m);
                        //set the mixer effect percent based on the value of the control when we connect it
                        m.SetEffectPercent(control.GetValue());
                    }





                }
            }


        }

    }



    public void BreakCircuit()
    {
        //if we have an emission point
        if (m_CurrentEmissionSocket != null)
        {
            //log stuff
            if (DebugLog)
                Debug.Log(name + " disconnected from speaker " + m_CurrentEmissionSocket.name);


            //stop the sound from playing
            Stop();

            
            foreach (CAudioCircuit c in m_CurrentComponents)
            {
                //clear control callbacks to our effects
                IControl control = c.GetControl();
                if (control != null)
                {
                    control.SetControlCallback(null);
                }
            }
            m_CurrentComponents.Clear();

            //reset the static effect values
            foreach (MixerAudioEffect m in m_Effects)
            {
                m.SetEffectPercent(0f);
            }

            //bye bye speaker
            m_CurrentEmissionSocket = null;



        }
        else
        {
            if (DebugLog)
                Debug.Log(name + " has no speaker to remove effects from");
        }

    }

    void Play()
    {
        if (m_CurrentEmissionSocket != null)
        {
            //List<MixerAudioEffect>
            //create the sound
            m_SoundPlaying = new Sound(m_ClipToPlay);

            //make our sounds for the effects
            
            m_EffectSoundsPlaying = new Sound[m_Effects.Count][];

            for (int i = 0; i < m_EffectSoundsPlaying.Length; i++)
			{
                //make an array full of null arrays for each effect
                
                m_EffectSoundsPlaying[i] = new Sound[m_Effects[i].Info.EffectClips.Length];

                //populate the arrays with lists of sounds for each effect
                for (int a = 0; a < m_EffectSoundsPlaying[i].Length; a++)
				{
                    m_EffectSoundsPlaying[i][a] = new Sound(m_Effects[i].Info.EffectClips[a]);
				}
			}

            //go through all of our sounds and connect them to their correct groups
            for (int i = 0; i < m_Effects.Count; i++)
            {

                for (int a = 0; a < m_EffectSoundsPlaying[i].Length; a++)
                {
                    AudioMixerGroup[] groups = m_Effects[i].EffectMixer.FindMatchingGroups("Effect");
                    if (groups.Length > 0)
                    {
                        m_EffectSoundsPlaying[i][a].SetEffectsInput(groups[a]);

                    }
                    else
                        Debug.LogError("Source " + name + "Could not find Channel for effect " + m_Effects[i].EffectMixer.name);
                }
            }





            //link up our sound if there are effects
            if (m_Effects.Count > 0)
            {
                m_SoundPlaying.SetEffectsInput(m_Effects[0].EffectMixerGroup);
            }
            else
            {
                m_SoundPlaying.SetEffectsInput(AudioManager.GetMasterMixerGroup("SFX"));
            }

            //set our output socket to play on our speaker
            m_SoundPlaying.SetEmissionSocket(m_CurrentEmissionSocket);
            m_SoundPlaying.SetIsLooping(true);
            m_SoundPlaying.Play(0, m_Effects.ToArray());


            //play our effect sounds
			for (int i = 0; i < m_EffectSoundsPlaying.Length; i++)
			{

				for (int a = 0; a < m_EffectSoundsPlaying[i].Length; a++)
				{
                    m_EffectSoundsPlaying[i][a].SetEmissionSocket(m_CurrentEmissionSocket);
                    m_EffectSoundsPlaying[i][a].SetIsLooping(true);
                    m_EffectSoundsPlaying[i][a].Play(0,null);

                }
			}


                    //set buttons

                    m_PlayButton.Pressed = true;
            m_StopButton.Pressed = false;
        }
    }

    void Stop()
    {
        if (m_SoundPlaying != null)
        {
            //stop sound and clear it
            m_SoundPlaying.Stop();
            m_SoundPlaying.SetEffectsInput(null);
            m_SoundPlaying = null;

            //blast em all
            for (int i = 0; i < m_EffectSoundsPlaying.Length; i++)
            {

                for (int a = 0; a < m_EffectSoundsPlaying[i].Length; a++)
                {
                    m_EffectSoundsPlaying[i][a].Stop();
                    m_EffectSoundsPlaying[i][a].SetEffectsInput(null);
                    m_EffectSoundsPlaying[i][a] = null;

                }
            }

            //set buttons
            m_StopButton.Pressed = true;
            m_PlayButton.Pressed = false;
        }
    }

    public void OnPlayButtonPressed()
    {
        if (m_SoundPlaying == null)
        {
            Play();

        }
    }

    public void OnStopButtonPressed()
    {
        if (m_SoundPlaying != null)
        {
            Stop();
        }
    }


}
