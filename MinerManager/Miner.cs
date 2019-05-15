// Thomas's Miner Manager Script
// =============================
// Date: 2019-05-14
//
// https://github.com/guitaristtom/SpaceEngineersScripts/tree/master/MinerManager

// =======================================================================================
//                                                                            --- Configuration ---
// =======================================================================================

// --- Essential Configuration ---
// =======================================================================================

// The group name for the cargo containers used for the input for the drills
string inputCargoGroupName = "Outpost - Input Storage";

// The group name for the cargo containers used for the input for the drills
string pistonBlockGroupName = "Outpost - Pistons";

// Keyword for LCDs on the same grid to output data to
string outputLcdKeyword = "!MinerManagerOutput";

// =======================================================================================
//                                                                      --- End of Configuration ---
//                                                        Don't change anything beyond this point!
// =======================================================================================

bool compileSuccess = false;

string heartBeat = "|";

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
public Program()
{
    Echo("Thomas's Miner Manager");
    Echo("----------------------------------");

    // Grab the group of cargo containers, and check that it exists. Set them up.
    IMyBlockGroup inputCargoGroup = GridTerminalSystem.GetBlockGroupWithName(inputCargoGroupName);
    if (inputCargoGroup == null)
    {
        Echo("Cargo group not found.\r\nPlease change the 'inputCargoGroupName' variable");
        return;
    }
    inputCargoGroup.GetBlocksOfType<IMyCargoContainer>(inputCargoBlocks);
    Echo($"Set up {inputCargoBlocks.Count} input cargo containers.");

    // Grab the group of pistons, and check that it exists. Then set them up.
    IMyBlockGroup pistonBlockGroup = GridTerminalSystem.GetBlockGroupWithName(pistonBlockGroupName);
    if (pistonBlockGroup == null)
    {
        Echo("Piston group not found.\r\nPlease change the 'pistonBlockGroupName' variable");
        return;
    }
    pistonBlockGroup.GetBlocksOfType<IMyPistonBase>(pistonBlocks);
    Echo($"Set up {pistonBlocks.Count} pistons.");

    // Set up a list for all batteries, and check if any batteries are available
    // on the same immediate grid
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteryBlocks, block => block.IsSameConstructAs(Me));
    if (batteryBlocks == null)
    {
        Echo("No batteries found.\r\nPlease add some batteries and recompile.");
        return;
    }
    Echo($"Found {batteryBlocks.Count} batteries.");

    // Set up a list for all drills
    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drillBlocks, block => block.IsSameConstructAs(Me));
    if (drillBlocks == null)
    {
        Echo("No drills found.\r\nPlease add some drills and recompile.");
        return;
    }
    Echo($"Found {drillBlocks.Count} drills.");

    // Set up the LCDs
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(outputLcds, block => block.CustomName.Contains(outputLcdKeyword));

    // Assume everything in here ran and set up correctly.
    compileSuccess = true;

    // Configure this program to run every 100 update ticks
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
private double BatteryPercentage(List<IMyBatteryBlock> batteries)
{
    // Iterate through each battery and get the info
    float currentPower = 0;
    float powerTotal = 0;
    foreach (var battery in batteries)
    {
        currentPower += battery.CurrentStoredPower;
        powerTotal += battery.MaxStoredPower;
    }

    // Calculate the percentage
    double percentage = Math.Round((currentPower / powerTotal) * 100, 2);

    return percentage;
}

/**
 * Change a character that is shown, just so the user knows the script hasn't
 * died.
 */
private void BeatHeart()
{
    // Check which heart beat character is currently stored, then change it
    if (heartBeat == "|") {
        heartBeat = "/"; return;
    };

    if (heartBeat == "/") {
        heartBeat = "-"; return;
    };

    if (heartBeat == "-") {
        heartBeat = "\\"; return;
    };

    if (heartBeat == "\\") {
        heartBeat = "|"; return;
    };

    return;
}

/**
 * Returns the percentage of how full the given cargo is
 */
private double CargoFullPercentage(List<IMyCargoContainer> cargoBlocks)
{
    // Iterate through each battery and get the info
    float currentUsedStorage = 0;
    float storageTotal = 0;
    foreach (var cargoBlock in cargoBlocks)
    {
        IMyInventory inventoryData = cargoBlock.GetInventory();

        currentUsedStorage += (float)inventoryData.CurrentVolume;
        storageTotal += (float)inventoryData.MaxVolume;
    }

    // Calculate the percentage
    double percentage = Math.Round((currentUsedStorage / storageTotal) * 100, 2);

    return percentage;
}

/**
 * Displays the standard data on the given LCD.
 */
private void DisplayOutput(IMyTextPanel lcd)
{
    // Turn the LCD on
    lcd.Enabled = true;

    // Set the LCD to `text and image` mode
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;

    // Title
    lcd.WriteText("Thomas's Miner Manager " + heartBeat);
    lcd.WriteText("\r\n----------------------------------", true);

    // Battery Info
    lcd.WriteText($"\r\nBatteries ("+ batteryBlocks.Count + "): " + BatteryPercentage(batteryBlocks) + "%", true);

    // Storage Info
    lcd.WriteText($"\r\nInput Storage ("+ inputCargoBlocks.Count + "): " +
        CargoFullPercentage(inputCargoBlocks) + "%", true);

    return;
}

/**
 * Echo's other specific info to the programmable block's internal "console".
 */
private void EchoOutput()
{
    // Title
    Echo("Thomas's Miner Manager " + heartBeat);
    Echo("----------------------------------");

    // Battery Info
    Echo($"Batteries ("+ batteryBlocks.Count + "): " + BatteryPercentage(batteryBlocks) + "%");

    // Storage Info
    Echo($"Input Storage ("+ inputCargoBlocks.Count + "): " + CargoFullPercentage(inputCargoBlocks) + "%");
}

/**
 * Does this need explaining? This is the main method for the entire program.
 */
public void Main(string arg)
{
    // Checks if all of the stuff in "Progam()" ran correctly.
    if (compileSuccess == false)
    {
        Echo("Compile was unsuccessful, please retry.");
        return;
    }

    // "Beat" the heart, so the user knows this hasn't died.
    BeatHeart();

    // Write info to the LCDs
    foreach (var outputLcd in outputLcds)
    {
        DisplayOutput(outputLcd);
    }

    // Echo some info as well.
    EchoOutput();
}
