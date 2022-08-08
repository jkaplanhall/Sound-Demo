// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

/*
Author: Seth Grinstead
ChangeLog:
12/11/2021: Added comments and change log
*/

/*
 * Author: Jacob Kaplan-Hall
 * ChangeLog: d/m/y
 * 2/12/21: finalized for module
 */

public class CSocket : MonoBehaviour
{

    //reference to circuit
    [SerializeField]
    CAudioCircuit m_Circuit;

    //current plug if one is plugged in, no plug is null
    public CPlug CurrentPlug { get; private set; } = null;

    
    //weather or not we are an input or an output
    public bool IsInput { get; private set; } = false;


    public void HandleConnection(CPlug NewPlug)
	{

        // Enable / Disable plug
        if (NewPlug == null && CurrentPlug != null)
        {
            CurrentPlug.enabled = false;
            CurrentPlug.SetCurrentSocket(null);
        }
        else
        {
            NewPlug.enabled = true;
            NewPlug.SetCurrentSocket(this);
        }

        //save the old plug so we can use it
        CPlug OldPlug = CurrentPlug;
        // Set currentplug
        CurrentPlug = NewPlug;

        //this is to allow the audio component to know the state before and after disconnect
        //useful for when you need sent a signal down a line that has just been unplugged

        m_Circuit.SocketUpdate(NewPlug, OldPlug,this);

    }

    //getters / setters
    public CAudioCircuit GetAudioCircuit()
	{
        return m_Circuit;
	}

    public void SetIsInput(bool value)
    {
        IsInput = value;
    }


    public CAudioCircuit GetConnectedCircuit()
    {
        if (CurrentPlug != null)
        {
            return CurrentPlug.GetConnectedCircuit();
        }

        return null;
    }

}
