// Thomas's Miner Manager Script
// =============================
// Date: 2019-05-16
//
// https://github.com/guitaristtom/SpaceEngineersScripts/tree/master/MinerManager

// =======================================================================================
//                                                                            --- Configuration ---
// =======================================================================================

// --- Essential Configuration ---
// =======================================================================================

// The group name for the cargo containers used for the input for the drills.
string inputCargoGroupName = "Outpost - Input Storage";

// The name for the advanced rotor that is used for spinning the drills
string advancedRotorName = "Outpost - Advanced Rotor";

// The group name for the pistons that are used for pushing the drills.
string pistonBlockGroupName = "Outpost - Pistons";

// --- Non-Essential Configuration ---
// =======================================================================================

// How fast should the pistons extend / retract? (IT NEEDS the 'f' at the end)
float pistonExtendVelocity = 0.01f;
float pistonRetractVelocity = 0.50f;

// How long of a delay (in ~100 tick increments) should be waited before
// retracting the drills.
int pistonRetractDelay = 100;

// How far should the pistons go and retract? Between 0.00 and 10.00
// (In case you make it shorter for some reason)
double pistonMinLength = 0.00;
double pistonMaxLength = 10.00;

// Battery Percentages. Max and Min for flagging.
int batteryLowerLimit = 25;
int batteryUpperLimit = 75;

// Cargo Percentages. Max and Min for flagging.
int cargoLowerLimit = 25;
int cargoUpperLimit = 75;

// =======================================================================================
//                                                                      --- End of Configuration ---
//                                                        Don't change anything beyond this point!
// =======================================================================================

bool compileSuccess = false;

int delay = 0;

string heartBeat = "|";
string scriptState = "Starting";

IMyPistonBase currentPiston = null;
IMyMotorAdvancedRotor advancedRotor = null;

List<IMyBatteryBlock> batteryBlocks = new List<IMyBatteryBlock>();
List<IMyShipDrill> drillBlocks = new List<IMyShipDrill>();
List<IMyCargoContainer> inputCargoBlocks = new List<IMyCargoContainer>();
List<IMyTextPanel> outputLcds = new List<IMyTextPanel>();
List<IMyPistonBase> pistonBlocks = new List<IMyPistonBase>();

/**
 * SE automatically calls this when the program is compiled in game.
 *
 * Initializes the values that are needed for the script only one time, as to
 * not waste cycles later on.
 */
public Program() {
    Echo("Thomas's Miner Manager");
    Echo("----------------------------------");

    // Set up the advanced rotor.
    advancedRotor = GridTerminalSystem.GetBlockWithName(advancedRotorName) as IMyMotorAdvancedRotor;
    if (advancedRotor == null)
    {
        Echo("Advanced Rotor not found.\r\nPlease change the 'advancedRotorName' variable.");
        return;
    }

    // Grab the group of cargo containers, and check that it exists. Set them up.
    IMyBlockGroup inputCargoGroup = GridTerminalSystem.GetBlockGroupWithName(inputCargoGroupName);
    if (inputCargoGroup == null) {
        Echo("Cargo group not found.\r\nPlease change the 'inputCargoGroupName' variable");
        return;
    }
    inputCargoGroup.GetBlocksOfType<IMyCargoContainer>(inputCargoBlocks);
    Echo($"Set up {inputCargoBlocks.Count} input cargo containers.");

    // Grab the group of pistons, and check that it exists. Then set them up.
    IMyBlockGroup pistonBlockGroup = GridTerminalSystem.GetBlockGroupWithName(pistonBlockGroupName);
    if (pistonBlockGroup == null) {
        Echo("Piston group not found.\r\nPlease change the 'pistonBlockGroupName' variable");
        return;
    }
    pistonBlockGroup.GetBlocksOfType<IMyPistonBase>(pistonBlocks);
    Echo($"Set up {pistonBlocks.Count} pistons.");

    // Set up a list for all batteries, and check if any batteries are available
    // on the same immediate grid.
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteryBlocks, block => block.IsSameConstructAs(Me));
    if (batteryBlocks == null) {
        Echo("No batteries found.\r\nPlease add some batteries and recompile.");
        return;
    }
    Echo($"Found {batteryBlocks.Count} batteries.");

    // Set up a list for all drills.
    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drillBlocks, block => block.IsSameConstructAs(Me));
    if (drillBlocks == null) {
        Echo("No drills found.\r\nPlease add some drills and recompile.");
        return;
    }
    Echo($"Found {drillBlocks.Count} drills.");

    // Set up the LCDs.
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(outputLcds, block => block.CustomName.Contains("!MinerManagerOutput"));

    // Assume everything in here ran and set up correctly.
    compileSuccess = true;

    // Configure this program to run every 100 update ticks.
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    return;
}

/**
 * SE calls this whenever the game is saved or just before it's recompiled.
 */
public void Save() {}

/**
 * Returns the percentage of how full the batteries are.
 */
private double BatteryPercentage(List<IMyBatteryBlock> batteries) {
    // Iterate through each battery and get the info.
    float currentPower = 0;
    float powerTotal = 0;
    foreach (var battery in batteries) {
        currentPower += battery.CurrentStoredPower;
        powerTotal += battery.MaxStoredPower;
    }

    // Calculate the percentage.
    double percentage = Math.Round((currentPower / powerTotal) * 100, 2);

    return percentage;
}

/**
 * Change a character that is shown, just so the user knows the script hasn't
 * died.
 */
private void BeatHeart() {
    if (heartBeat == "|") {
        heartBeat = "/";
        return;
    };

    if (heartBeat == "/") {
        heartBeat = "-";
        return;
    };

    if (heartBeat == "-") {
        heartBeat = "\\";
        return;
    };

    if (heartBeat == "\\") {
        heartBeat = "|";
        return;
    };

    // Just in case...
    return;
}

/**
 * Returns the percentage of how full the given cargo is.
 */
private double CargoFullPercentage(List<IMyCargoContainer> cargoBlocks) {
    // Iterate through each cargo container and get the info from them.
    float currentUsedStorage = 0;
    float storageTotal = 0;
    foreach (var cargoBlock in cargoBlocks) {
        IMyInventory inventoryData = cargoBlock.GetInventory();

        currentUsedStorage += (float)inventoryData.CurrentVolume;
        storageTotal += (float)inventoryData.MaxVolume;
    }

    // Calculate the percentage.
    double percentage = Math.Round((currentUsedStorage / storageTotal) * 100, 2);

    return percentage;
}

/**
 * Displays the standard data on the given LCD.
 */
private void DisplayOutput(IMyTextPanel lcd) {
    // Turn the LCD on.
    lcd.Enabled = true;

    // Set the LCD to `text and image` mode.
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;

    // Title
    lcd.WriteText("Thomas's Miner Manager " + heartBeat);
    lcd.WriteText("\r\n------------------------------------", true);

    // Current Script State
    lcd.WriteText("\r\nScript State: " + scriptState, true);

    // Battery Info
    lcd.WriteText($"\r\nStored Battery ("+ batteryBlocks.Count + ") Power: " + BatteryPercentage(batteryBlocks) + "%", true);

    // Storage Info
    lcd.WriteText($"\r\nInput Cargo ("+ inputCargoBlocks.Count + ") Fill Level: " +
        CargoFullPercentage(inputCargoBlocks) + "%", true);

    return;
}

/**
 * Echo's other specific info to the programmable block's internal "console".
 */
private void EchoOutput() {
    // Title
    Echo("Thomas's Miner Manager " + heartBeat);
    Echo("------------------------------------");

    // Current Script State
    Echo("Script State: " + scriptState);

    // Battery Info
    Echo($"Stored Battery ("+ batteryBlocks.Count + ") Power: " + BatteryPercentage(batteryBlocks) + "%");

    // Storage Info
    Echo($"Input Cargo ("+ inputCargoBlocks.Count + ") Fill Level: " + CargoFullPercentage(inputCargoBlocks) + "%");

    // Advanced Rotor
    Echo($"Rotor: {advancedRotor.Velocity}");

    // Current Piston
    if (currentPiston != null && scriptState == "Extending") {
        Echo("");
        Echo($"Currently extending {currentPiston.CustomName} at {currentPiston.Velocity}m/s");
        Echo($"{currentPiston.CustomName} Position: {Math.Round(currentPiston.CurrentPosition, 2)}m");
    } else if (currentPiston != null && scriptState == "Retracting") {
        Echo("");
        Echo($"Currently retracting {currentPiston.CustomName} at {currentPiston.Velocity * -1}m/s");
        Echo($"{currentPiston.CustomName} Position: {Math.Round(currentPiston.CurrentPosition, 2)}m");
    }

    return;
}

/**
 * Set the given piston to the supplied velocity for it to extend.
 */
private void ExtendPiston(IMyPistonBase piston) {
    // Turn the piston on
    if (piston.Enabled == false) {
        piston.Enabled = true;
    }

    // If the piston is set to go faster then it's allowed, set it to the max
    // it can go.
    if (piston.Velocity <= piston.MaxVelocity) {
        piston.Velocity = pistonExtendVelocity;
    } else {
        piston.Velocity = piston.MaxVelocity;
    }

    return;
}

/**
 * Grabs the current piston who isn't extended and is working.
 */
private IMyPistonBase GetCurrentPiston(List<IMyPistonBase> pistons, string state) {
    IMyPistonBase newPiston = null;
    foreach (var piston in pistons) {
        if ($"{piston.Status}" != state && piston.IsFunctional == true) {
            newPiston = piston;
            break;
        }
    }

    return newPiston;
}

/**
 * Set the given piston to the supplied velocity (and multiply by -1) for it to
 * retract.
 */
private void RetractPiston(IMyPistonBase piston) {
    piston.Velocity = pistonRetractVelocity * -1;
}

/**
 * Sets up the values for the pistons to a proper, default, good, known state.
 */
private void SetUpPistons(List<IMyPistonBase> pistons) {
    foreach (var piston in pistons) {
        // Turn off all of the pistons, just in case.
        piston.Enabled = false;

        // Set their max and min, in case they were changed somehow.
        piston.MinLimit = (float)pistonMinLength;
        piston.MaxLimit = (float)pistonMaxLength;

        // Set the Velocity to zero.
        piston.Velocity = 0;
    }
}

/**
 * Turn off the given piston, and set its velocity to zero.
 */
private void StopPiston(IMyPistonBase piston) {
    // Turn the piston on
    piston.Enabled = false;

    // Set its velocity to zero
    piston.Velocity = 0;
}

/**
 * Does this need explaining? This is the main method for the entire program.
 */
public void Main(string arg) {
    // Checks if all of the stuff in "Progam()" ran correctly.
    if (compileSuccess == false) {
        Echo("Compile was unsuccessful, please retry.");
        return;
    }

    // "Beat" the heart, so the user knows this hasn't died.
    BeatHeart();

    // Write info to the LCDs.
    foreach (var outputLcd in outputLcds) {
        DisplayOutput(outputLcd);
    }

    // Echo some info as well.
    EchoOutput();

    // Check the script state
    // - "Starting": Running some start up commands and setting up some values.
    // - "Extending": Pistons are going, rotor is spinning, drills are on.
    // - "LowPower": The state that gets set when power is below a certain
    // percentage, and will stay until it gets above a certain percentage. Pauses
    // mining and turns off the refineries (if there is any) to save some power.
    // - "FullStorage": The state that gets set when the input storage cargo is
    // too full. Pauses mining.
    // - "Retracting:Delay": Timer for after the pistons are done extending that
    // the system waits before retracting the pistons.
    // - "Retracting": Currently retracting the pistons.
    // - "Completed": The state that the script goes in to once it's completely
    // done running.
    switch (scriptState) {
        case "Starting":
            SetUpPistons(pistonBlocks);
            scriptState = "Extending";
            break;

        case "Extending":
            currentPiston = GetCurrentPiston(pistonBlocks, "Extended");
            if (currentPiston == null) {
                scriptState = "Retracting:Delay";
            } else {
                ExtendPiston(currentPiston);
            }

            // Check if the battery is too low
            if (BatteryPercentage(batteryBlocks) <= batteryLowerLimit) {
                scriptState = "LowPower";
            }

            // Check if the cargo is too full
            if (CargoFullPercentage(inputCargoBlocks) >= cargoUpperLimit) {
                scriptState = "FullStorage";
            }
            break;

        case "LowPower":
            // WIP. Need to shut off refineries and pause drilling
            // Stop the current piston from extending.
            StopPiston(currentPiston);

            // If it is above the current limit, set it back to extending.
            if (BatteryPercentage(batteryBlocks) >= batteryUpperLimit) {
                scriptState = "Extending";
            }
            break;

        case "FullStorage":
            // WIP. Need to pause drilling
            // Stop the current piston from extending.
            StopPiston(currentPiston);

            // If it is above the current limit, set it back to extending.
            if (CargoFullPercentage(inputCargoBlocks) <= cargoLowerLimit) {
                scriptState = "Extending";
            }
            break;

        case "Retracting:Delay":
            if (delay >= pistonRetractDelay) {
                scriptState = "Retracting";
            } else {
                delay++;
                Echo($"Delay set {delay} of {pistonRetractDelay}");
            }
            break;

        case "Retracting":
            currentPiston = GetCurrentPiston(pistonBlocks, "Retracted");
            if (currentPiston == null) {
                scriptState = "Completed";
            } else {
                RetractPiston(currentPiston);
            }
            break;

        case "Completed":
            break;

        default:
            Echo("");
            Echo($"Unknown state given. Got: {scriptState}");
            return;
    }
}
