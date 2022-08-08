/*
 * Author: Jacob Kaplan-Hall
 * Overview: system that manages effect mixer assets used for the audio circuit, sound detection system,
 * and general effects
 * ChangeLog:
 * 29/11/21: start
 * 2/12/22 sound rework complete
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

public class MixerAudioEffect : IControlCallback
{
    public AudioMixer EffectMixer { get; private set; } = null;
    public AudioMixerGroup EffectMixerGroup { get; private set; } = null;

    public EffectInfo Info { get; private set; } = null;

    public string EffectName { get; private set; } = "";

    public float EffectPercent { get; private set; } = 0;

    AudioMixerSnapshot[] m_Snapshots = null;
    float[] m_SnapshotPercents = null;
    float[] m_CurrentSnapshotRatio = null;


    public MixerAudioEffect(AudioMixer mixer)
    {
        if (mixer != null)
        {
            //reference the mixer of course!
            EffectMixer = mixer;

            //Get the name of the mixer and remove the number if it is not the first one

            EffectName = mixer.name.Substring(0, mixer.name.Length - 2);

            //get master group from the mixer
            AudioMixerGroup[] groups = mixer.FindMatchingGroups("Master");

            if (groups.Length > 0)
            {
                EffectMixerGroup = groups[0];
            }
            else
            {
                Debug.LogError("Couldnt get mixer group from audio effect mixer");
            }

            //we're gonna check resouces to load the extra data if there is some
            Info = Resources.Load<EffectInfo>("EffectMixers/" + EffectName + "/" + EffectName);
            
            //we will setup for default use
            if (Info == null)
            {


                m_Snapshots = new AudioMixerSnapshot[] { mixer.FindSnapshot("Min"), mixer.FindSnapshot("Max") };
                m_SnapshotPercents = new float[] { 0, 1 };
                m_CurrentSnapshotRatio = new float[] { 1, 0 };
            }
            else
			{
                //lets get a list of our snapshots by extracting them from the mixer, hope nobody messed up the effect info
                m_Snapshots = new AudioMixerSnapshot[Info.Snapshots.Length];
                for (int i = 0; i < m_Snapshots.Length; i++)
                {
                    m_Snapshots[i] = mixer.FindSnapshot(Info.Snapshots[i]);
                }


                m_SnapshotPercents = Info.SnapshotPercents;

                m_CurrentSnapshotRatio = new float[m_Snapshots.Length];

                //set all but the start to zero
                for (int a = 1; a <m_CurrentSnapshotRatio.Length; a++)
                    m_CurrentSnapshotRatio[a] = 0f;

                m_CurrentSnapshotRatio[0] = 1f;

			}



        }
        else
		{
            Debug.LogError("null mixer for effect");
		}
        
    }

    public void SetEffectPercent(float value)
    {
        float Value = Mathf.Clamp(value, 0, 1);

        //set our reference to the current percent of the mixers value
        EffectPercent = Value;
        //handle the control input from an interactable control to the mixer

        if (EffectMixer)
        {

            //Debug.Log("Mixer control update on " + EffectMixer.name + ": " + Value);

            //set the ratio to transition equally between the snapshots

            //first, lets find the two snapshots that we are between
                int i = 0;
            for (;i < m_SnapshotPercents.Length - 1; i++)
			{
                if (m_SnapshotPercents[i] <= Value && m_SnapshotPercents[i + 1] >= Value)
				{
                    break;
				}
			}


            //we now have the index of the low snapshot, so the high snapshot is just one above that


            //lets get the ratio we are between the current two snapshots
            float localValue = Value - m_SnapshotPercents[i];
            float Diff = (m_SnapshotPercents[i + 1] - m_SnapshotPercents[i]);

            //this is the value of interpolation between the two snapshots in question
            float InterpNormal = localValue / Diff;


            //quick reset of the values
            for (int s = 0; s < m_CurrentSnapshotRatio.Length;s++)
			{
                if (s == i || s == i + 1)
				{
                    continue;
				}

                m_CurrentSnapshotRatio[s] = 0f;
			}


            //now lets set our values for the snapshots we are interpolating

            m_CurrentSnapshotRatio[i] = 1 - InterpNormal;
            m_CurrentSnapshotRatio[i + 1] = InterpNormal;

            string text = "";
            for (int a = 0; a < m_CurrentSnapshotRatio.Length; a++)
                text += m_CurrentSnapshotRatio[a] + ", ";

            Debug.Log(text);



            //set transition between snapshots
            EffectMixer.TransitionToSnapshots(m_Snapshots, m_CurrentSnapshotRatio, 0);
        }
        else
        {
            Debug.LogError("Effect has no mixer assigned to its effect!");
        }
    }

    public float GetEffectPercent()
	{
        return EffectPercent;
	}
    public void OnControlUpdate(float value)
    {

        SetEffectPercent(Mathf.Clamp(value,0f,1f));
    }


}












static class MixerManager
{
    static public bool isInitalized { get; private set; } = false;

    static List<MixerAudioEffect> m_MixerEffects = null;
    static List<MixerAudioEffect> m_RentedMixerEffects = null;

    static public void Init()
    {

        if (!isInitalized)
        {

            //wow! very easy to load the mixers in that folder
            AudioMixer[] LoadedMixers = Resources.LoadAll<AudioMixer>("EffectMixers");
            m_MixerEffects = new List<MixerAudioEffect>();
            m_RentedMixerEffects = new List<MixerAudioEffect>();
            foreach (AudioMixer m in LoadedMixers)
            {
                
                
                MixerAudioEffect newEffect = new MixerAudioEffect(m);
                m_MixerEffects.Add(newEffect);
                
            }


            foreach (MixerAudioEffect m in m_MixerEffects)
            {
                Debug.Log(m.EffectMixer.name + ", " + m.EffectMixerGroup.name + ", " + m.EffectName);
            }



            //initalized!
            isInitalized = true;
        }
    }



    public static MixerAudioEffect RentMixer(string name)
	{
        if (!isInitalized)
            Init();

        foreach (MixerAudioEffect m in m_MixerEffects)
		{
            if (m.EffectName == name)
			{
                //move to the rented mixers list
                m_RentedMixerEffects.Add(m);
                m_MixerEffects.Remove(m);
                
                //quick reset there
                m.EffectMixer.outputAudioMixerGroup = null;
                return m;
			}
		}

        //if we didnt find an effect with a matching name
        return null;
	}

    public static void ReturnMixer(MixerAudioEffect effect)
	{
        if (effect != null)
        {
            foreach (MixerAudioEffect m in m_RentedMixerEffects)
            {
                //find the mixer in our list of rented mixers
                if (m.EffectMixer.name == effect.EffectMixer.name)
                {
                    //clear the output group
                    m.EffectMixer.outputAudioMixerGroup = null;

                    //move to the effects pool
                    m_MixerEffects.Add(m);
                    m_RentedMixerEffects.Remove(m);
                    //we're done
                    return;
                }
            }
        }
	}
}
