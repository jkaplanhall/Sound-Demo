 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartMicrophone : MonoBehaviour, IAudioDetectionCallback
{
    [SerializeField]
    MicCondition m_Condition;


    //visual gauges
    [SerializeField]
    NeedleGague[] m_Gagues;

    [SerializeField]
    float GagueSpeed = 4f;

    //turns on when it hears the sound it wants
    [SerializeField]
    IndicatorLight m_Indicator;


    Sound m_MonitoredSound = null;
    MixerAudioEffect[] m_MonitoredEffects = null;

    // Start is called before the first frame update
    void Start()
    {



    }

    // Update is called once per frame
    void Update()
    {
        //if we have a sound we are hearing
        if (m_MonitoredSound != null)
        {
            //and its playing
            if (m_MonitoredSound.IsPlaying())
            {
                for (int i = 0; i < m_MonitoredEffects.Length; i++)
				{
                    MixerAudioEffect e = m_MonitoredEffects[i];
                    if (e != null)
					{
                        NeedleGague G = m_Gagues[i];
                        G.EnableLight();
                        G.SetMainValue(Mathf.Lerp(G.GetMainValue(), m_MonitoredEffects[i].GetEffectPercent(), Time.deltaTime * GagueSpeed));
                    }
				}
                m_Indicator.Brightness = 2f;
            }
            else //if its not playing, delete the sound
            {
                m_MonitoredEffects = null;
                m_MonitoredSound = null;


                //turn off the lights
                foreach (NeedleGague g in m_Gagues)
                {
                    g.DisableLight();
                }
                m_Indicator.Brightness = 0f;
            }
    
        }
        else //if theres no sound being heard
		{
            //just have them travel to 0
            foreach (NeedleGague g in m_Gagues)
			{
                g.SetMainValue(Mathf.Lerp(g.GetMainValue(), 0f, Time.deltaTime * GagueSpeed));
            }
            
		}
        
    }

    public void OnNewSoundPlayed(ISound newSound, MixerAudioEffect[] liveEffects)
	{
        //if we are not currently monitoring a sound
        if (m_MonitoredSound == null)
        {
            //try to get the new sound
            m_MonitoredSound = newSound as Sound;
            if (m_MonitoredSound != null)
            {


                if (m_Condition.SoundCondition == m_MonitoredSound.Clip)
                {
                    //now we have to make sure that the sound has any required effects


                    //if we do that, clear the effects as well and make a new list
                    m_MonitoredEffects = new MixerAudioEffect[m_Condition.EffectConditions.Length];

                    for (int i = 0; i < m_Condition.EffectConditions.Length; i++)
					{
                        //go the live effects and place them in the list corrisponding to their place in the conditional array
                        for (int a = 0; i < liveEffects.Length; a++)
						{
                            if (liveEffects[a].EffectName == m_Condition.EffectConditions[i])
							{
                                m_MonitoredEffects[i] = liveEffects[a];
                                m_Gagues[i].SetAccuracyMargin(m_Condition.EffectAccuracyMargins[i]);
                                m_Gagues[i].SetTargetValue(m_Condition.EffectPercentConditions[i]);
                                break;
							}

						}

                    }






                }
                else
				{
                    m_MonitoredSound = null;
				}


            }
        }




	}
}
