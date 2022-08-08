using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioInit : MonoBehaviour
{

    void Awake()
    {
        AudioManager.Start();
        MixerManager.Init();
    }


}
