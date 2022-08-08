// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

/*
Author: Jacob Kaplan-Hall
Comment/Description: Class that is a test script
ChangeLog:
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

public class AutoDrifter : MonoBehaviour
{
    [SerializeField]
    Vector3 Movement;   // Vector3 for movment

    void Start()
    {
        
    }

    void Update()
    {
        // Moves object
        transform.position += Movement * Time.deltaTime;
    }
}
