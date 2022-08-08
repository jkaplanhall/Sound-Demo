/*
Author: Jacob Kaplan-Hall
Comment/Description: source used in an audio circuit to control the sound played through a connected speaker
ChangeLog: d/m/y
2/12/21: finalized for module
2/12/22 sound rework complete
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

//interfaces are fast!!
public interface ISource
{
    public void MakeCircuit(List<CAudioCircuit> Components, Transform emissionSocket);

    public void TryMakeCircuit();
    public void BreakCircuit();
}
public class CSource : CAudioCircuit , ISource
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

    //list of effects currently applied to the sources output
    protected List<MixerAudioEffect> m_CurrentEffectStack = null;

    protected List<CAudioCircuit> m_CurrentComponents = null;

    // Start is called before the first frame update
    new protected void Awake()
    {
        base.Awake();
        CircuitType = EAudioCircuitType.Source;
        m_CurrentEffectStack = new List<MixerAudioEffect>();
        m_CurrentComponents = new List<CAudioCircuit>();


        //hook up our buttons
        if (m_PlayButton != null)
            m_PlayButton.OnPressed.AddListener(this.OnPlayButtonPressed);
        if (m_StopButton != null)
            m_StopButton.OnPressed.AddListener(this.OnStopButtonPressed);
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
            AudioMixer LastMixer = null;
            foreach (CAudioCircuit circuit in Components)
            {

                //grab a mixer effect from our mixer manager
                MixerAudioEffect m = MixerManager.RentMixer(circuit.GetEffectName());
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
                    


                   
                    
                    //add the effect to our effects stack
                    m_CurrentEffectStack.Add(m);

                    //we have a new mixer to link in the effects chain!
                    
                    if (LastMixer != null)
                    {
                        LastMixer.outputAudioMixerGroup = m.EffectMixerGroup;
                    }
                    //last set the last mixer to the current mixer
                    LastMixer = m.EffectMixer;
                    
                }


            }

        //hook the output to the mixer to the SFX output
        if (LastMixer != null)
        LastMixer.outputAudioMixerGroup = AudioManager.GetMasterMixerGroup("SFX");
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

            //
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
            
            //undo the effects chain
            foreach (MixerAudioEffect m in m_CurrentEffectStack)
            {
                //disband the mixer link chain
                m.EffectMixer.outputAudioMixerGroup = AudioManager.GetMasterMixerGroup("SFX");
                //send the mixer back to our manager
                MixerManager.ReturnMixer(m);
            }
            //clear the effects list
            m_CurrentEffectStack.Clear();

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

            //link up our sound if there are effects
            if (m_CurrentEffectStack.Count > 0)
			{
                m_SoundPlaying.SetEffectsInput(m_CurrentEffectStack[0].EffectMixerGroup);
			}
            else
			{
                m_SoundPlaying.SetEffectsInput(AudioManager.GetMasterMixerGroup("SFX"));
			}

            //set our output socket to play on our speaker
            m_SoundPlaying.SetEmissionSocket(m_CurrentEmissionSocket);
            m_SoundPlaying.SetIsLooping(true);
            m_SoundPlaying.Play(0,m_CurrentEffectStack.ToArray());

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
