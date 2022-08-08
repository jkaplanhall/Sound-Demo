using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/*
Author: Seth Grinstead
ChangeLog:
12/11/2021: Added comments
*/

/*
 * Author: Jacob Kaplan-Hall
 * Discription: base circuit component 
 * ChangeLog: d/m/y
 * 2/12/21: finalized for module
 * 3/10/22: added support for effect sources
 */

//type of circuit
public enum EAudioCircuitType
{
    None = 0,
    Speaker = 1,
    Source = 2,
    //TODO More to come
}


public class CAudioCircuit : MonoBehaviour
{

    //sockets
    [SerializeField]
    protected List<CSocket> m_InputSockets = null;
    [SerializeField]
    protected CSocket m_OutputSocket = null;




    //circuit type
    public EAudioCircuitType CircuitType { get; protected set; } = 0;

    //control
    [SerializeField]
    protected GameObject m_Control = null;
    protected IControl m_IControl = null;
    [SerializeField, Range(0,1)]
    protected float m_StartControlValue = 0f;

    //mixer effects
    [SerializeField]
    protected string EffectName = "";


    //this works with the effect source system, it allows the effect to modify an effect that is always applied and stored in the source
    [SerializeField]
    protected bool b_IsEffectModifier = false;
    public bool IsModifier() { return b_IsEffectModifier; }

    //debug
    [SerializeField]
    protected static bool DebugLog = true;



    protected void Awake()
    {



        //set the input bool for the different sockets on the circuit
        if (m_InputSockets != null)
        {
            foreach (CSocket s in m_InputSockets)
			{
                s.SetIsInput(true);
			}
        }
        if (m_OutputSocket)
            m_OutputSocket.SetIsInput(false);









    }

	public void Start()
	{
        //if we have a control, interface it with the control callback for this script
        if (m_Control != null)
        {
            IControl control = m_Control.GetComponent<IControl>();
            if (control != null)
            {
                m_IControl = control;

                m_IControl.SetValue(m_StartControlValue);


            }
        }
    }

	//goes in a direction and gathers components in the circuit and stores them in a list
	//the direction the gather goes in is decied by the GoesToInput bool
	public void RecursiveGatherForward(in List<CAudioCircuit> componentList, CAudioCircuit start)
    {
        //add myself to the list!
        componentList.Add(this);

        //check to make sure there is another circuit to search
        if (m_OutputSocket != null)
        {
            //search to the next output
            CAudioCircuit circuit = m_OutputSocket.GetConnectedCircuit();
        
            //make sure that we are not in an infinite loop or at the end
            if (circuit != null && circuit != start)
                circuit.RecursiveGatherForward(componentList, start);
        }


    }

    public void PingSources(bool ShouldConnect, CAudioCircuit start)
	{
        if (CircuitType == EAudioCircuitType.Source)
		{
            //wow! we're a source! time to ping ourselves!
            ISource source = this as ISource;

            if (source != null)
            {
                if (ShouldConnect)
				{
                    source.TryMakeCircuit();
				}
                else
				{
                    source.BreakCircuit();
				}
            }
            else
			{
                Debug.LogError("Invalid Source without interface!");
			}
		}

        foreach(CSocket s in m_InputSockets)
		{
            CAudioCircuit next = s.GetConnectedCircuit();
            if (next != null && next != start)
			{
                next.PingSources(ShouldConnect,start);
			}
		}
	}
    public void SocketUpdate(CPlug newPlug, CPlug oldPlug, CSocket socket)
    {

        //if its not null...
        if (newPlug != null)
        {

            //we need to now check if we just completed a circuit
            CAudioCircuit CurrentCircuit = this;

            while (true)
            {
                //if we have no output, we're done
                if (CurrentCircuit.m_OutputSocket == null)
                    break;

                //get the next circuit
                CAudioCircuit NextCircuit = CurrentCircuit.m_OutputSocket.GetConnectedCircuit();

                //if we get nothing, or we're going in a loop, we're done
                if (NextCircuit == null || NextCircuit == this)
                    break;

                //otherwise iterate to the next circuit
                CurrentCircuit = NextCircuit; 

			}
            
            //is the last one a speaker?? i sure hope so!
            if (CurrentCircuit.CircuitType == EAudioCircuitType.Speaker)
			{
                //we got a speaker, now we ping our sources and tell them to set up a circuit
                PingSources(true,this);
			}


        }        // If null then unplug event
        else
        {
            //if it was our input socket, we're gonna have to do a little jump
            if (socket.IsInput)
            {
                CAudioCircuit PrevCircuit = oldPlug.GetConnectedCircuit();
                if (PrevCircuit)
				{
                    PrevCircuit.PingSources(false,this);
				}
            }
            else
            {
                PingSources(false,this);
            }


        }
    }

    //getters for sockets
    public List<CSocket> GetInputSockets()
    {
        return m_InputSockets;
    }
    public CSocket GetOutputSocket()
    {
        return m_OutputSocket;
    }





    public string GetEffectName()
    {
        return EffectName;
    }

    public IControl GetControl()
	{
        return m_IControl;
	}





    
}
