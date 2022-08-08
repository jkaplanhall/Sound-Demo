// Copyright (c) DeepSilentStudio Ltd. 2021. All Rights Reserved.

// CHANGE LOG
// DATE FORMAT: DD/MM/YYYY
// DATE CREATED: 20/10/2021

/*
Author: Aviraj Singh Virk
Comment/Description: Interface for all Inputs
ChangeLog:
11/11/2021: Refined and added 2 new methods
01/12/2021: Added LeanRight and LeanLeft declarations
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interface for all Inputs
public interface IControllerInterface
{
	Vector3 GetMoveInput();			// Gets movement input

    Vector3 GetLookInput();			// Gets camera look input

    bool IsJumping();				// Gets jump input

    bool IsPickingUp();				// Gets interact/pick input

    bool IsSprinting();				// Gets sprint input
    
    bool ToggleCrouch();			// Gets crouch input

    bool ToggleCrawl();				// Gets crawl input

    bool QuitGame();				// Gets quit/pause input

    bool CrankFlashlight();			// Gets crank/recharge flashlight input

    bool ToggleFlashlight();		// Gets toggle flashlight input

    bool SwitchToWalkieTalkie();	// Gets toggle walkie talkie input

    bool ToggleInventory();			// Gets opening/closing inventory input

    public bool LeanLeft();         // Gets input for leaning left

    public bool LeanRight();        // Gets the input for leaning right
}