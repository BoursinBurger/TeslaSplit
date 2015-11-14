# TeslaSplit
Speedrunning tools for Teslagrad

Purpose:
This program watches the Teslagrad save game files looking for updates.
New saves are made upon entering a room, killing a boss, collecting an item, etc.
Upon encountering a key event you're anticipating, it will press the split hotkey
for your timesplitting program. This allows hands-free and consistently accurate
split times.

System Requirements:
This program runs on Windows and requires .NET Framework version 4 installed.
It also requires the Windows Media Player wmp.dll located in \Windows\System32.

Setup:
The application uses an external DLL: InputSimulator 0.1.0.0 (https://inputsimulator.codeplex.com/)

You may also want to edit the TeslaSplit.exe.config file and configure the SplitHotkey:
SplitHotkey is the button this program will press upon detecting a split.
 The full set of hotkey options is listed in the config file's comments.
 This is set to NUMPAD1 by default since that's what LiveSplit uses.

Normal Usage:
Start the program and it will display a configuration screen. Here you can customize
your lists of split events. Press the buttons under the Help section for more info.

Select the split list you want to use and press the Start Watching button.

The window now displays its status as well as the saved game variables it's monitoring
once it detects the saved game slot you're playing on.

If you need to pause the file watcher, press the "Stop Watching" button.

If you need to restart your run, you may press the Reset button, however the program
has been written to detect when a new game has started and automatically reset for you.

Otherwise, just play Teslagrad. It will display the split event it's watching for,
and press the split hotkey whenever that split is triggered.

When the sequence of splits has finished, it will not press any more buttons,
but it will still display updates to the save file.

You may press the Reset button to start over when you do a new run, or let the auto-reset detect it.

Limitations:
Please note that the save file is not updated during three events we commonly use for splitting:
1) Immediately starting a new game.
2) Defeating The King.
3) Reaching the Ende.

You can either manually split those events, or use the following three things as a substitute:

1) The first game saves are made at the end of the opening cutscene when the door opens.
We have set the auto-reset detector to watch for these saves and reset the splits. 
Shortly afterward, the game saves the event "openBarriers: 262144"
SDA's rules say to start the timer when you gain control of the character, so this openBarriers event
is the perfect event to use for their timing.

2) You can set a split point for reaching the King's room and another for the Ende Moat scene
after teleporting back to the tower in a 100% run.

3) Press your split hotkey when the character stops moving in either ending.

Q&A:
Why a file watcher and not an AutoSplit program within LiveSplit?
The save game file is small, simple, and human-readable. Teslagrad's memory footprint is around 700MB.
In the future it may certainly be more feasible to use memory addresses and an ASL program, but I'm
not going to be the one brave enough to crawl through that much memory hunting for single bits.

Why is Windows Media Player required?
OK, since you read to the end of this file, I will reveal an Easter Egg included in the program.
If you place a file named "hamsterdance.mp3" in the program directory, it will be played during a
specific part of the game, for your added entertainment. Pressing either button or completing that
area of the game will stop the music. To disable this feature, remove the MP3 from the program directory.
(Obviously I cannot distribute this song due to copyright concerns.)
