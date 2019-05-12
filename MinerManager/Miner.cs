// Thomas's Miner Manager Script
// =============================
// Date: 2019-05-11
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

// --- Debug Configuration. You shouldn't need to edit this. ---
// =======================================================================================
// Keyword for LCDs on the same grid to output debug data to
string debugLcdKeyword = "!MinerManagerDebug";

// =======================================================================================
//                                                                      --- End of Configuration ---
//                                                        Don't change anything beyond this point!
// =======================================================================================

List<IMyCargoContainer> inputCargoBlocks = new List<IMyCargoContainer>();
List<IMyPistonBase> pistonBlocks = new List<IMyPistonBase>();
List<IMyBatteryBlock> batteryBlocks = new List<IMyBatteryBlock>();
List<IMyShipDrill> drillBlocks = new List<IMyShipDrill>();
List<IMyTextPanel> debugLcds = new List<IMyTextPanel>();
List<IMyTextPanel> outputLcds = new List<IMyTextPanel>();

public Program()
{
    // Grab the group of cargo containers, and check that it exists. Set them up.
    IMyBlockGroup inputCargoGroup = GridTerminalSystem.GetBlockGroupWithName(inputCargoGroupName);
    if (inputCargoGroup == null)
    {
        Echo("Cargo group not found.\r\nPlease change the 'inputCargoGroupName' variable");
        return;
    }
    inputCargoGroup.GetBlocksOfType<IMyCargoContainer>(inputCargoBlocks);

    // Grab the group of pistons, and check that it exists. Then set them up.
    IMyBlockGroup pistonBlockGroup = GridTerminalSystem.GetBlockGroupWithName(pistonBlockGroupName);
    if (pistonBlockGroup == null)
    {
        Echo("Piston group not found.\r\nPlease change the 'pistonBlockGroupName' variable");
        return;
    }
    pistonBlockGroup.GetBlocksOfType<IMyPistonBase>(pistonBlocks);

    // Set up a list for all batteries, and check if any batteries are available
    // on the same immediate grid
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteryBlocks, block => block.IsSameConstructAs(Me));
    if (batteryBlocks == null)
    {
        Echo("No batteries found.\r\nPlease add some batteries and recompile.");
        return;
    }

    // Set up a list for all drills
    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drillBlocks, block => block.IsSameConstructAs(Me));
    if (drillBlocks == null)
    {
        Echo("No drills found.\r\nPlease add some drills and recompile.");
        return;
    }

    // Set up the LCDs
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(debugLcds, block => block.CustomName.Contains(debugLcdKeyword));
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(outputLcds, block => block.CustomName.Contains(outputLcdKeyword));

    return;
}

public void Save() {}

/**
 * Displays debug data on the given LCD
 */
public void DisplayDebug(IMyTextPanel lcd)
{
    // Turn the LCD on
    lcd.Enabled = true;

    // Set the LCD to `text and image` mode
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;

    // First line doesn't have true, so you erase everything
    lcd.WriteText("");

    // Everything else does, so it appends
    lcd.WriteText("This is the debug LCD", true);
    lcd.WriteText("\n\rDebug things~!", true);

    return;
}

/**
 * Returns info on the given list of batteries
 */
public decimal BatteryPercentage(List<IMyBatteryBlock> batteries)
{
    // Iterate through each battery and get the info
    float powerTotal = 0;
    float currentPower = 0;
    foreach (var battery in batteries)
    {
        powerTotal += battery.MaxStoredPower;
        currentPower += battery.CurrentStoredPower;
    }

    // Calculate the percentage
    decimal percentage = Math.Round((currentPower / powerTotal) * 100);

    return batteryInfo;
}

/**
 * Displays the standard data on the given LCD
 */
public void DisplayOutput(IMyTextPanel lcd)
{
    // Turn the LCD on
    lcd.Enabled = true;

    // Set the LCD to `text and image` mode
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;

    // Title
    lcd.WriteText("Thomas's Miner Manager");
    lcd.WriteText("\r\n----------------------------------", true);

    // Battery Info
    lcd.WriteText($"\r\nBatteries ("+ batteryBlocks.Count + "): " + BatteryPercentage(batteryBlocks) + "%", true);

    return;
}

// Echo's other specific info to the programmable block's internal "console"
public void EchoOutput()
{
    Echo("Thomas's Miner Manager");
    Echo("----------------------------------");
}


public void Main(string arg)
{
    // Write info to the LCDs
    foreach (var outputLcd in outputLcds)
    {
        DisplayOutput(outputLcd);
    }

    // Echo some info as well.
    EchoOutput();

    // List off the names of all the input cargo from the group
    Echo($"Input Cargo Blocks:");
    foreach (var cargoBlock in inputCargoBlocks)
    {
        Echo($"- {cargoBlock.CustomName}");
    }

    Echo("\r\n");

    // List off the names of the pistons from the given group
    Echo($"Piston Blocks:");
    foreach (var pistonBlock in pistonBlocks)
    {
        Echo($"- {pistonBlock.CustomName}");
    }

    Echo("\r\n");

    // List off the names of all the batteries on the immediate grid
    Echo($"Battery Blocks:");
    foreach (var batteryBlock in batteryBlocks)
    {
        Echo($"- {batteryBlock.CustomName}");
    }
}
