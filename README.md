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
The application uses two external DLLs: DiffPlex 1.2.0 (https://github.com/mmanela/diffplex)
and InputSimulator 0.1.0.0 (https://inputsimulator.codeplex.com/)

You'll also want to edit the TeslaSplit.exe.config file and fill in the key variables:

SplitHotkey is the button this program will press upon detecting a split.
 The full set of hotkey options is listed in the config file's comments.
 This is set to NUMPAD1 by default since that's what LiveSplit uses.
 
SplitList is the sequence of split events you use for your run.
 The list of events is separated by commas.
 The full set of valid events are listed in the config file's comments.
 Also refer to TeslaSplit.txt for a sample set of events encountered in sequence during a 100% run.
 Each of the events is notated with the room or event it represents.
 This sample list is set in SplitList by default to show what it looks like.

Normal Usage:
Start the program before beginning your run. It runs in a small window which displays
its status as well as the saved game variables it's monitoring once it detects the
saved game slot you're playing on.

If you need to pause the file watcher, press the "Stop Watching" button.
If you need to restart your run, press the Reset button.
Otherwise, just play Teslagrad. It will display the split event it's watching for,
and press the split hotkey whenever that split is triggered.

When the sequence of splits has finished, it will not press any more buttons,
but it will still display updates to the save file.
Simply press the Reset button to start over when you do a new run.

Limitations:
Please note that the save file is not updated during three events we commonly use for splitting:
1) Starting a new game at the opening cutscene.
2) Killing the King.
3) Reaching the Ende.

You can either manually split those events, or use the following three things as a substitute:
1) The first game save is made at the end of the opening cutscene when the door opens. Briefly at this point, openBarriers is set to 0. SDA's rules say to start the timer when you gain control of the character, so this is the perfect event to use for their timing.
2) You can set a split point for reaching the Ende Moat scene after teleporting back to the tower in a 100% run.
3) Upon reaching the Ende, if you press Enter to return to the main menu, the gameComplete flag is set to 1. In an NG+ run, you can use defeatedBosses: 0 as a substitute.

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
