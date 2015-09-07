using System;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Win32;
using WMPLib;
using WindowsInput;
using System.Text;

namespace TeslaSplit
{
    public partial class FormMain : Form
    {
        private FileSystemWatcher fswLegacy;
        private FileSystemWatcher fswCloud;
        private string previousText;
        private string configLegacySavePath;
        private string configCloudSavePath;
        private string configSplitHotkey;
        private string configResetEvent;
        private string configMapHotkey;
        private string configSplitList;
        private string configItemAnimationSkipGlove;
        private string configItemAnimationSkipBoots;
        private string configItemAnimationSkipCloak;
        private string configItemAnimationSkipStaff;
        private string configSelectedSplitsIndex;
        private string activeSavePath;
        private bool splitFlag;
        private string[] splits;
        private int splitCounter;
        private short scrollCounter;
        private string[] scenes;
        private List<string> pickups;
        private WindowsMediaPlayer HamsterDance;
        private StringBuilder sbText;
        private List<SplitList> ListOfSplitLists;
        private int selectedSplitsIndex;
        private Configuration config;

        public FormMain()
        {
            InitializeComponent();
            configLegacySavePath = "";
            configSplitHotkey = "";
            configSplitList = "";
            ControlsDelegate.SetText(this, tbTitle, "");
            ControlsDelegate.SetText(this, labelNextSplit, "");
            ControlsDelegate.SetText(this, labelScene, "");
            ControlsDelegate.SetText(this, labelCheckpoint, "");
            ControlsDelegate.SetText(this, labelGlove, "");
            ControlsDelegate.SetText(this, labelBlink, "");
            ControlsDelegate.SetText(this, labelSuit, "");
            ControlsDelegate.SetText(this, labelStaff, "");
            ControlsDelegate.SetText(this, labelBarriers, "");
            ControlsDelegate.SetText(this, labelOrbs, "");
            ControlsDelegate.SetText(this, labelScrollCount, "");
            ControlsDelegate.SetText(this, labelBosses, "");
            ControlsDelegate.SetText(this, labelComplete, "");
            scenes = new[] { "Home", "Scales", "Rooftops", "Broken Bridge", "Chimneys", "Balconies", "Stave Church", "Moat", "Courtyard", "Classroom", "Levitation", "Pistons", "Chapel", "Barrier", "Trials", "Well", "Thunderbolt", "Iron Lice", "Magic Carpet", "Snakeway", "Fernus", "Maglev", "Hidey Hole", "Fernus", "Cooling Room", "Cages", "Magnetflies", "Grues", "Waterworks", "Waterworks", "Roots", "Act One", "Heartwood", "Maze", "Mural", "Ventilation", "Faradeus", "Shrine", "Wintergarden", "Wintergarden", "Wintergarden", "Wintergarden", "Wintergarden", "Wintergarden", "Storage", "Act Two", "Scrapyard", "Crusher", "Smelter", "Molten Pool", "Fireproof", "Forge", "Licemover", "Pipes", "Chokepoint", "Brazen Bull", "Magnetbridge", "Guardian", "Electromagnets", "Clerestory", "Oleg", "Act Three", "Control Room", "Wheeltrack", "Acrobatics", "Race", "Feast hall", "Happy Ending", "Magnetic Lift", "Magnetic Ball", "Fatal Attraction", "Surprise", "Alternation", "Grand Design", "Solomon Tesla", "Pinnacle", "Guerickes Orb", "Dodge This", "Deep Down", "Hidden Library", "Tower", "Tanngrisne", "Tanngnjost", "Bridge", "Room", "Stormfront", "Palace Stairs", "Downfall", "Passage", "Scrolls", "Dungeon", "Scrolls", "Secret Passage", "Grand Hall", "The King", "Cooler", "Assembler", "Forge", "Homage", "Crown Space", "Home", "Scales", "Rooftops", "Broken Bridge", "Chimneys", "Balconies", "Stave Church", "Moat", "Tower", "Tower", "Tower", "Tower", "Tower", "Tower" };
            HamsterDance = new WindowsMediaPlayer();
            sbText = new StringBuilder();
            pickups = new List<string>();

            // Only used for debugging
            //CheckForIllegalCrossThreadCalls = false;

            try
            {
                // Read the app config settings
                string appPath =
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string configFile = System.IO.Path.Combine(appPath, "TeslaSplit.exe.config");
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap {ExeConfigFilename = configFile};
                config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                configLegacySavePath = config.AppSettings.Settings["LegacySavePath"].Value;
                configCloudSavePath = config.AppSettings.Settings["CloudSavePath"].Value;
                configSplitHotkey = config.AppSettings.Settings["SplitHotkey"].Value;
                configResetEvent = config.AppSettings.Settings["ResetEvent"].Value;
                configMapHotkey = config.AppSettings.Settings["mapHotkey"].Value;
                configSplitList = config.AppSettings.Settings["SplitList"].Value;
                configSelectedSplitsIndex = config.AppSettings.Settings["SelectedSplitsIndex"].Value;
                configItemAnimationSkipGlove = config.AppSettings.Settings["ItemAnimationSkipGlove"].Value;
                configItemAnimationSkipBoots = config.AppSettings.Settings["ItemAnimationSkipBoots"].Value;
                configItemAnimationSkipCloak = config.AppSettings.Settings["ItemAnimationSkipCloak"].Value;
                configItemAnimationSkipStaff = config.AppSettings.Settings["ItemAnimationSkipStaff"].Value;
                
                // Set up the file watchers
                fswLegacy = new FileSystemWatcher
                {
                        Path =
                                CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                             configLegacySavePath),
                        NotifyFilter = NotifyFilters.LastWrite,
                        Filter = "SavedGame.asset",
                        IncludeSubdirectories = true
                };
                fswLegacy.Changed += FileWatcherTrigger;

                RegistryKey regKey = Registry.CurrentUser;
                regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

                if (regKey != null)
                {
                    string steamPath = regKey.GetValue("SteamPath").ToString();
                    
                    fswCloud = new FileSystemWatcher
                    {
                            Path = CombinePaths(steamPath, configCloudSavePath),
                            NotifyFilter = NotifyFilters.LastWrite,
                            Filter = "SavedGame.asset",
                            IncludeSubdirectories = true
                    };
                    fswCloud.Changed += FileWatcherTrigger;
                }

                ListOfSplitLists = new List<SplitList>();

                // If the app config has issues, report them.
                if (configLegacySavePath == "")
                {
                    buttonStopStart.Enabled = false;
                    MessageBox.Show("LegacySavePath is missing from app config.");
                }
                else if (configSplitHotkey == "")
                {
                    buttonStopStart.Enabled = false;
                    MessageBox.Show("SplitHotkey is missing from app config.");
                }
                else if (configMapHotkey == "")
                {
                    buttonStopStart.Enabled = false;
                    MessageBox.Show("mapHotkey is missing from app config.");
                }
                else if (configSplitList == "")
                {
                    buttonStopStart.Enabled = false;
                    MessageBox.Show("SplitList is missing from app config.");
                }
                else
                {
                    // Set up the config window
                    List<string> configSplitListLines = configSplitList.Split('[').Select(s => s.Trim()).ToList();
                    foreach (string splitListLine in configSplitListLines.Where(s => s.Length > 0))
                    {
                        ListOfSplitLists.Add(new SplitList
                        {
                            Name = splitListLine.Substring(0, splitListLine.IndexOf(']')),
                            Splits = splitListLine.Substring(splitListLine.IndexOf(']') + 1).Split(',').Select(a => a.Trim()).ToArray()
                        });

                    }

                    splitCounter = 0;
                    scrollCounter = 0;
                    if (!Int32.TryParse(configSelectedSplitsIndex, out selectedSplitsIndex))
                        selectedSplitsIndex = 0;

                    checkBoxGlove.Checked = configItemAnimationSkipGlove == "true";
                    checkBoxBoots.Checked = configItemAnimationSkipBoots == "true";
                    checkBoxCloak.Checked = configItemAnimationSkipCloak == "true";
                    checkBoxStaff.Checked = configItemAnimationSkipStaff == "true";

                    previousText = "";
                    fswLegacy.EnableRaisingEvents = false;
                    groupUrn.Visible = false;
                    ControlsDelegate.SetText(this, buttonStopStart, "Start Watching");

                    LoadSplits();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("{0}: {1}", e.GetType(), e.Message));
                Application.Exit();
            }
        }

        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("null paths");
            }
            return paths.Aggregate(Path.Combine);
        }


        private void FileWatcherTrigger(object source, FileSystemEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.FullPath != activeSavePath)
                {
                    // A new saved game has been opened
                    activeSavePath = eventArgs.FullPath;
                    DoReset("New save file detected.\r\nSplits have been reset!");
                }

                string currentText = "";

                // Read the new save file
                bool content = false;
                while (!content)
                {
                    using (FileStream fs = new FileStream(eventArgs.FullPath, FileMode.Open, FileAccess.Read,
                                                          FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs))
                        currentText = sr.ReadToEnd();

                    // If my stream has data, then escape the while loop.
                    // Otherwise, wait a little bit for file I/O and try again.
                    if (currentText.Length > 100)
                        content = true;
                    else
                        Thread.Sleep(50);

                }

                // Before parsing currentText for split events, scan it for evidence of a new run.
                // Possible triggers: 
                // openBarriers: 0
                // openBarriers: 262144
                if (currentText.Contains(configResetEvent))
                {
                    DoReset(String.Format("Reset event [{0}] detected!\r\nSplits have been reset!", configResetEvent));
                }

                if (previousText == "")
                {
                    // Set up a new run
                    sbText.PrependLine(String.Format("Loaded {0}\r\n", eventArgs.FullPath));
                    ControlsDelegate.SetText(this, labelNextSplit,
                                             String.Format("First Split:\r\n{0}", splits[splitCounter]));
                    ControlsDelegate.SetText(this, labelScene, String.Format("{0} - {1}", "sceneIndex: 0", scenes[0]));
                }


                // If all splits are done, quit analysis
                if (splitCounter >= splits.Count())
                    return;

                // Otherwise, continue an ongoing run
                string currentSplit = splits[splitCounter];
                splitFlag = false;

                // Get differences between previousText and currentText
                Differ d = new Differ();
                InlineDiffBuilder idb = new InlineDiffBuilder(d);
                var result = idb.BuildDiffModel(previousText, currentText);

                // Now analyze for splits
                foreach (
                        string lineText in
                                result.Lines.Where(t => t.Type == ChangeType.Inserted).Select(line => line.Text.Trim()))
                {
                    // Log the event
                    sbText.PrependLine(String.Format("[{0}] {1}", DateTime.Now.ToString("s"), lineText));

                    // Check for opt-in pickup skip events and perform the skip if they occur
                    if (pickups.Contains(lineText))
                        SendPickupSkip();

                    // If the split event is an exact text match on the line, split
                    if (currentSplit.Trim() == lineText)
                        SendSplit();

                            // CollectScroll and ScrollCount split events
                    else if (lineText.Contains("orbsFound:") && lineText != "orbsFound:")
                    {
                        scrollCounter++;
                        ControlsDelegate.SetText(this, labelScrollCount,
                                                 String.Format("Scrolls Collected: {0}", scrollCounter));
                        if (currentSplit == "CollectScroll" || currentSplit == "ScrollCount: " + scrollCounter)
                            SendSplit();
                    }

                            // BarrierChange split event
                    else if (lineText.Contains("openBarriers:") && currentSplit == "BarrierChange")
                        SendSplit();

                            // DefeatedBoss split event
                    else if (lineText.Contains("defeatedBosses:") && currentSplit == "DefeatedBoss")
                        SendSplit();

                            // ItemPickup split event
                    else if ((lineText.Contains("glove: 1")
                              || lineText.Contains("blink: 1")
                              || lineText.Contains("suit: 1")
                              || lineText.Contains("staff: 1"))
                             && currentSplit == "ItemPickup")
                        SendSplit();

                    // Hamster Dance Easter Egg!
                    // Play the mp3 during scenes 63, 64, 65, and stop upon reaching scene 98
                    if (lineText.Contains("sceneIndex: 63") && File.Exists("hamsterdance.mp3"))
                    {
                        sbText.PrependLine(String.Format("[{0}] {1}", DateTime.Now.ToString("s"), "Hamster Dance!"));
                        HamsterDance.URL = "hamsterdance.mp3";
                        HamsterDance.controls.play();
                    }
                    if (lineText.Contains("sceneIndex: 98") && HamsterDance.playState == WMPPlayState.wmppsPlaying)
                    {
                        HamsterDance.controls.stop();
                    }
                }

                // Scene + Checkpoint split event. This has to be checked after all individual lines have been processed.
                if (currentSplit.Contains("Scene") && currentSplit.Contains("CheckPoint"))
                {
                    string[] sceneCheckSplit = Regex.Split(currentSplit, @"\W+");
                    string[] sceneSplit = Regex.Split(labelScene.Text, @"\W+");
                    string[] checkSplit = Regex.Split(labelCheckpoint.Text, @"\W+");

                    if (sceneCheckSplit.Count() == 4
                        && sceneSplit.Count() == 2
                        && checkSplit.Count() == 2
                        && sceneCheckSplit[1] == sceneSplit[1]
                        && sceneCheckSplit[3] == checkSplit[1])
                        SendSplit();
                }

                // If a split occurred during this FileWatcherTrigger, then update the split counter and display next split
                if (splitFlag)
                {
                    ControlsDelegate.SetText(this, labelNextSplit, (splitCounter < splits.Count()
                                                                            ? String.Format("Next Split:\r\n{0}",
                                                                                            splits[splitCounter])
                                                                            : "End of splits"));
                }



                buttonReset.Enabled = true;

                // Populate labels
                string[] lines = Regex.Split(currentText, "\r\n");
                foreach (string line in lines)
                {
                    string l = line.Trim();

                    // The switch statement won't work with line.Contains, so we gotta do this the ugly way.

                    if (line.Contains("sceneIndex:"))
                    {
                        int sceneNumber = Int32.Parse(l.Substring(l.LastIndexOf(' ') + 1));
                        if (sceneNumber < scenes.Count())
                            ControlsDelegate.SetText(this, labelScene,
                                                     String.Format("{0} - {1}", l, scenes[sceneNumber]));
                    }
                    else if (line.Contains("checkpointIndex:"))
                        ControlsDelegate.SetText(this, labelCheckpoint, l);
                    else if (line.Contains("glove:"))
                        ControlsDelegate.SetText(this, labelGlove, l);
                    else if (line.Contains("blink:"))
                        ControlsDelegate.SetText(this, labelBlink, l);
                    else if (line.Contains("suit:"))
                        ControlsDelegate.SetText(this, labelSuit, l);
                    else if (line.Contains("staff:"))
                        ControlsDelegate.SetText(this, labelStaff, l);
                    else if (line.Contains("openBarriers:"))
                        ControlsDelegate.SetText(this, labelBarriers, l);
                    else if (line.Contains("orbsFound:"))
                        ControlsDelegate.SetText(this, labelOrbs, l);
                    else if (line.Contains("defeatedBosses:"))
                        ControlsDelegate.SetText(this, labelBosses, l);
                    else if (line.Contains("gameComplete:"))
                        ControlsDelegate.SetText(this, labelComplete, l);
                }


                ControlsDelegate.SetText(this, tbTitle, sbText.ToString());
                previousText = currentText;
            }

            catch (Exception e)
            {
                MessageBox.Show(String.Format("{0}: {1}", e.GetType(), e.Message));
                Application.Exit();
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            bool flip = !fswLegacy.EnableRaisingEvents;
            fswLegacy.EnableRaisingEvents = flip;
            if (fswCloud != null)
                fswCloud.EnableRaisingEvents = flip;
            ControlsDelegate.SetText(this, buttonStopStart, flip ? "Stop Watching" : "Start Watching");
            ControlsDelegate.SetText(this, tbTitle, flip ? String.Format("Watching Save Directories:\r\n{0}\r\n{1}", fswLegacy.Path, fswCloud == null ? "" : fswCloud.Path) : "Stopped");
            groupConfig.Visible = !flip;
            groupUrn.Visible = flip;

            if (HamsterDance.playState == WMPPlayState.wmppsPlaying)
                HamsterDance.controls.stop();

            if (!flip)
                return;
           
            // Set selected split list
            splitCounter = 0;
            scrollCounter = 0;
            ControlsDelegate.SetText(this, groupUrn, ListOfSplitLists[selectedSplitsIndex].Name);
            splits = ListOfSplitLists[selectedSplitsIndex].Splits;

            // Set pickups list for Item Animation Skip
            pickups = new List<string>();
            if (checkBoxGlove.Checked)
                pickups.Add("glove: 1");
            if (checkBoxBoots.Checked)
                pickups.Add("blink: 1");
            if (checkBoxCloak.Checked)
                pickups.Add("suit: 1");
            if (checkBoxStaff.Checked)
                pickups.Add("staff: 1");

            // Save config changes to app.config file
            SaveConfig();

        }

        private void SaveConfig()
        {
            // Set Item Animation Skip booleans
            config.AppSettings.Settings["ItemAnimationSkipGlove"].Value = checkBoxGlove.Checked.ToString();
            config.AppSettings.Settings["ItemAnimationSkipBoots"].Value = checkBoxBoots.Checked.ToString();
            config.AppSettings.Settings["ItemAnimationSkipCloak"].Value = checkBoxCloak.Checked.ToString();
            config.AppSettings.Settings["ItemAnimationSkipStaff"].Value = checkBoxStaff.Checked.ToString();

            // Save DataGridView Split List to the selected Split List
            ListOfSplitLists[selectedSplitsIndex].Splits =
                (from DataGridViewRow dgvr
                 in dgvSplits.Rows
                 where dgvr.Cells[0].Value != null && dgvr.Cells[0].Value.ToString() != ""
                 select dgvr.Cells[0].Value.ToString()).ToArray();

            // Build SplitList variable
            StringBuilder sbSplitList = new StringBuilder();
            foreach (SplitList sl in ListOfSplitLists)
                sbSplitList.Append(String.Format("[{0}]{1}", sl.Name, string.Join(",", sl.Splits)));
            
            config.AppSettings.Settings["SplitList"].Value = sbSplitList.ToString();
            config.AppSettings.Settings["SelectedSplitsIndex"].Value = selectedSplitsIndex.ToString();

            // Save modified config
            config.Save();
        }

        private void LoadSplits()
        {
            comboBoxSplitListSelector.Items.Clear();

            foreach (SplitList sl in ListOfSplitLists)
                comboBoxSplitListSelector.Items.Add(sl.Name);

            comboBoxSplitListSelector.SelectedIndex = selectedSplitsIndex;

            dgvSplits.Rows.Clear();

            foreach (string s in ListOfSplitLists[selectedSplitsIndex].Splits)
            {
                dgvSplits.Rows.Add(new[] { s });
            }
        }

        private void SendSplit()
        {
            if (splitFlag)
                return;

            splitFlag = true;
            splitCounter++;
            sbText.PrependLine(String.Format("[{0}] {1}", DateTime.Now.ToString("s"), "Sending a Split"));
            VirtualKeyCode? vkc = VKCTranslate(configSplitHotkey);
            if (vkc == null)
                return;

            InputSimulator.SimulateKeyDown((VirtualKeyCode) vkc);
            Thread.Sleep(250);
            InputSimulator.SimulateKeyUp((VirtualKeyCode) vkc);
        }

        private void SendPickupSkip()
        {
            VirtualKeyCode? vkc = VKCTranslate(configMapHotkey);
            if (vkc == null)
                return;

            sbText.PrependLine(String.Format("[{0}] {1}", DateTime.Now.ToString("s"), "Sending pickup animation skip"));
            InputSimulator.SimulateKeyDown((VirtualKeyCode) vkc);
            Thread.Sleep(100);
            InputSimulator.SimulateKeyUp((VirtualKeyCode) vkc);
            Thread.Sleep(100);
            InputSimulator.SimulateKeyDown((VirtualKeyCode) vkc);
            Thread.Sleep(100);
            InputSimulator.SimulateKeyUp((VirtualKeyCode) vkc);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            DoReset("Splits Reset!");
        }

        private void DoReset(string ResetReason = "")
        {
            ControlsDelegate.SetText(this, tbTitle, "");
            ControlsDelegate.SetText(this, labelNextSplit, "");
            ControlsDelegate.SetText(this, labelScene, "");
            ControlsDelegate.SetText(this, labelCheckpoint, "");
            ControlsDelegate.SetText(this, labelGlove, "");
            ControlsDelegate.SetText(this, labelBlink, "");
            ControlsDelegate.SetText(this, labelSuit, "");
            ControlsDelegate.SetText(this, labelStaff, "");
            ControlsDelegate.SetText(this, labelBarriers, "");
            ControlsDelegate.SetText(this, labelOrbs, "");
            ControlsDelegate.SetText(this, labelScrollCount, "");
            ControlsDelegate.SetText(this, labelBosses, "");
            ControlsDelegate.SetText(this, labelComplete, "");
            splitCounter = 0;
            scrollCounter = 0;
            previousText = "";
            sbText = new StringBuilder(ResetReason);
            sbText.PrependLine(String.Format("Watching Save Directories:\r\n{0}\r\n{1}", fswLegacy.Path, fswCloud == null ? "" : fswCloud.Path));
            buttonReset.Enabled = false;
            if (fswLegacy.EnableRaisingEvents)
                ControlsDelegate.SetText(this, tbTitle, sbText.ToString());

            if (HamsterDance.playState == WMPPlayState.wmppsPlaying)
                HamsterDance.controls.stop();
        }

        private static VirtualKeyCode? VKCTranslate(string code)
        {
            switch (code)
            {
                case "LBUTTON":
                    return VirtualKeyCode.LBUTTON;
                case "RBUTTON":
                    return VirtualKeyCode.RBUTTON;
                case "CANCEL":
                    return VirtualKeyCode.CANCEL;
                case "MBUTTON":
                    return VirtualKeyCode.MBUTTON;
                case "XBUTTON1":
                    return VirtualKeyCode.XBUTTON1;
                case "XBUTTON2":
                    return VirtualKeyCode.XBUTTON2;
                case "BACK":
                    return VirtualKeyCode.BACK;
                case "TAB":
                    return VirtualKeyCode.TAB;
                case "CLEAR":
                    return VirtualKeyCode.CLEAR;
                case "RETURN":
                    return VirtualKeyCode.RETURN;
                case "SHIFT":
                    return VirtualKeyCode.SHIFT;
                case "CONTROL":
                    return VirtualKeyCode.CONTROL;
                case "MENU":
                    return VirtualKeyCode.MENU;
                case "PAUSE":
                    return VirtualKeyCode.PAUSE;
                case "CAPITAL":
                    return VirtualKeyCode.CAPITAL;
                case "KANA":
                    return VirtualKeyCode.KANA;
                case "HANGEUL":
                    return VirtualKeyCode.HANGEUL;
                case "HANGUL":
                    return VirtualKeyCode.HANGUL;
                case "JUNJA":
                    return VirtualKeyCode.JUNJA;
                case "FINAL":
                    return VirtualKeyCode.FINAL;
                case "HANJA":
                    return VirtualKeyCode.HANJA;
                case "KANJI":
                    return VirtualKeyCode.KANJI;
                case "ESCAPE":
                    return VirtualKeyCode.ESCAPE;
                case "CONVERT":
                    return VirtualKeyCode.CONVERT;
                case "NONCONVERT":
                    return VirtualKeyCode.NONCONVERT;
                case "ACCEPT":
                    return VirtualKeyCode.ACCEPT;
                case "MODECHANGE":
                    return VirtualKeyCode.MODECHANGE;
                case "SPACE":
                    return VirtualKeyCode.SPACE;
                case "PRIOR":
                    return VirtualKeyCode.PRIOR;
                case "NEXT":
                    return VirtualKeyCode.NEXT;
                case "END":
                    return VirtualKeyCode.END;
                case "HOME":
                    return VirtualKeyCode.HOME;
                case "LEFT":
                    return VirtualKeyCode.LEFT;
                case "UP":
                    return VirtualKeyCode.UP;
                case "RIGHT":
                    return VirtualKeyCode.RIGHT;
                case "DOWN":
                    return VirtualKeyCode.DOWN;
                case "SELECT":
                    return VirtualKeyCode.SELECT;
                case "PRINT":
                    return VirtualKeyCode.PRINT;
                case "EXECUTE":
                    return VirtualKeyCode.EXECUTE;
                case "SNAPSHOT":
                    return VirtualKeyCode.SNAPSHOT;
                case "INSERT":
                    return VirtualKeyCode.INSERT;
                case "DELETE":
                    return VirtualKeyCode.DELETE;
                case "HELP":
                    return VirtualKeyCode.HELP;
                case "VK_0":
                    return VirtualKeyCode.VK_0;
                case "VK_1":
                    return VirtualKeyCode.VK_1;
                case "VK_2":
                    return VirtualKeyCode.VK_2;
                case "VK_3":
                    return VirtualKeyCode.VK_3;
                case "VK_4":
                    return VirtualKeyCode.VK_4;
                case "VK_5":
                    return VirtualKeyCode.VK_5;
                case "VK_6":
                    return VirtualKeyCode.VK_6;
                case "VK_7":
                    return VirtualKeyCode.VK_7;
                case "VK_8":
                    return VirtualKeyCode.VK_8;
                case "VK_9":
                    return VirtualKeyCode.VK_9;
                case "VK_A":
                    return VirtualKeyCode.VK_A;
                case "VK_B":
                    return VirtualKeyCode.VK_B;
                case "VK_C":
                    return VirtualKeyCode.VK_C;
                case "VK_D":
                    return VirtualKeyCode.VK_D;
                case "VK_E":
                    return VirtualKeyCode.VK_E;
                case "VK_F":
                    return VirtualKeyCode.VK_F;
                case "VK_G":
                    return VirtualKeyCode.VK_G;
                case "VK_H":
                    return VirtualKeyCode.VK_H;
                case "VK_I":
                    return VirtualKeyCode.VK_I;
                case "VK_J":
                    return VirtualKeyCode.VK_J;
                case "VK_K":
                    return VirtualKeyCode.VK_K;
                case "VK_L":
                    return VirtualKeyCode.VK_L;
                case "VK_M":
                    return VirtualKeyCode.VK_M;
                case "VK_N":
                    return VirtualKeyCode.VK_N;
                case "VK_O":
                    return VirtualKeyCode.VK_O;
                case "VK_P":
                    return VirtualKeyCode.VK_P;
                case "VK_Q":
                    return VirtualKeyCode.VK_Q;
                case "VK_R":
                    return VirtualKeyCode.VK_R;
                case "VK_S":
                    return VirtualKeyCode.VK_S;
                case "VK_T":
                    return VirtualKeyCode.VK_T;
                case "VK_U":
                    return VirtualKeyCode.VK_U;
                case "VK_V":
                    return VirtualKeyCode.VK_V;
                case "VK_W":
                    return VirtualKeyCode.VK_W;
                case "VK_X":
                    return VirtualKeyCode.VK_X;
                case "VK_Y":
                    return VirtualKeyCode.VK_Y;
                case "VK_Z":
                    return VirtualKeyCode.VK_Z;
                case "LWIN":
                    return VirtualKeyCode.LWIN;
                case "RWIN":
                    return VirtualKeyCode.RWIN;
                case "APPS":
                    return VirtualKeyCode.APPS;
                case "SLEEP":
                    return VirtualKeyCode.SLEEP;
                case "NUMPAD0":
                    return VirtualKeyCode.NUMPAD0;
                case "NUMPAD1":
                    return VirtualKeyCode.NUMPAD1;
                case "NUMPAD2":
                    return VirtualKeyCode.NUMPAD2;
                case "NUMPAD3":
                    return VirtualKeyCode.NUMPAD3;
                case "NUMPAD4":
                    return VirtualKeyCode.NUMPAD4;
                case "NUMPAD5":
                    return VirtualKeyCode.NUMPAD5;
                case "NUMPAD6":
                    return VirtualKeyCode.NUMPAD6;
                case "NUMPAD7":
                    return VirtualKeyCode.NUMPAD7;
                case "NUMPAD8":
                    return VirtualKeyCode.NUMPAD8;
                case "NUMPAD9":
                    return VirtualKeyCode.NUMPAD9;
                case "MULTIPLY":
                    return VirtualKeyCode.MULTIPLY;
                case "ADD":
                    return VirtualKeyCode.ADD;
                case "SEPARATOR":
                    return VirtualKeyCode.SEPARATOR;
                case "SUBTRACT":
                    return VirtualKeyCode.SUBTRACT;
                case "DECIMAL":
                    return VirtualKeyCode.DECIMAL;
                case "DIVIDE":
                    return VirtualKeyCode.DIVIDE;
                case "F1":
                    return VirtualKeyCode.F1;
                case "F2":
                    return VirtualKeyCode.F2;
                case "F3":
                    return VirtualKeyCode.F3;
                case "F4":
                    return VirtualKeyCode.F4;
                case "F5":
                    return VirtualKeyCode.F5;
                case "F6":
                    return VirtualKeyCode.F6;
                case "F7":
                    return VirtualKeyCode.F7;
                case "F8":
                    return VirtualKeyCode.F8;
                case "F9":
                    return VirtualKeyCode.F9;
                case "F10":
                    return VirtualKeyCode.F10;
                case "F11":
                    return VirtualKeyCode.F11;
                case "F12":
                    return VirtualKeyCode.F12;
                case "F13":
                    return VirtualKeyCode.F13;
                case "F14":
                    return VirtualKeyCode.F14;
                case "F15":
                    return VirtualKeyCode.F15;
                case "F16":
                    return VirtualKeyCode.F16;
                case "F17":
                    return VirtualKeyCode.F17;
                case "F18":
                    return VirtualKeyCode.F18;
                case "F19":
                    return VirtualKeyCode.F19;
                case "F20":
                    return VirtualKeyCode.F20;
                case "F21":
                    return VirtualKeyCode.F21;
                case "F22":
                    return VirtualKeyCode.F22;
                case "F23":
                    return VirtualKeyCode.F23;
                case "F24":
                    return VirtualKeyCode.F24;
                case "NUMLOCK":
                    return VirtualKeyCode.NUMLOCK;
                case "SCROLL":
                    return VirtualKeyCode.SCROLL;
                case "LSHIFT":
                    return VirtualKeyCode.LSHIFT;
                case "RSHIFT":
                    return VirtualKeyCode.RSHIFT;
                case "LCONTROL":
                    return VirtualKeyCode.LCONTROL;
                case "RCONTROL":
                    return VirtualKeyCode.RCONTROL;
                case "LMENU":
                    return VirtualKeyCode.LMENU;
                case "RMENU":
                    return VirtualKeyCode.RMENU;
                case "BROWSER_BACK":
                    return VirtualKeyCode.BROWSER_BACK;
                case "BROWSER_FORWARD":
                    return VirtualKeyCode.BROWSER_FORWARD;
                case "BROWSER_REFRESH":
                    return VirtualKeyCode.BROWSER_REFRESH;
                case "BROWSER_STOP":
                    return VirtualKeyCode.BROWSER_STOP;
                case "BROWSER_SEARCH":
                    return VirtualKeyCode.BROWSER_SEARCH;
                case "BROWSER_FAVORITES":
                    return VirtualKeyCode.BROWSER_FAVORITES;
                case "BROWSER_HOME":
                    return VirtualKeyCode.BROWSER_HOME;
                case "VOLUME_MUTE":
                    return VirtualKeyCode.VOLUME_MUTE;
                case "VOLUME_DOWN":
                    return VirtualKeyCode.VOLUME_DOWN;
                case "VOLUME_UP":
                    return VirtualKeyCode.VOLUME_UP;
                case "MEDIA_NEXT_TRACK":
                    return VirtualKeyCode.MEDIA_NEXT_TRACK;
                case "MEDIA_PREV_TRACK":
                    return VirtualKeyCode.MEDIA_PREV_TRACK;
                case "MEDIA_STOP":
                    return VirtualKeyCode.MEDIA_STOP;
                case "MEDIA_PLAY_PAUSE":
                    return VirtualKeyCode.MEDIA_PLAY_PAUSE;
                case "LAUNCH_MAIL":
                    return VirtualKeyCode.LAUNCH_MAIL;
                case "LAUNCH_MEDIA_SELECT":
                    return VirtualKeyCode.LAUNCH_MEDIA_SELECT;
                case "LAUNCH_APP1":
                    return VirtualKeyCode.LAUNCH_APP1;
                case "LAUNCH_APP2":
                    return VirtualKeyCode.LAUNCH_APP2;
                case "OEM_1":
                    return VirtualKeyCode.OEM_1;
                case "OEM_PLUS":
                    return VirtualKeyCode.OEM_PLUS;
                case "OEM_COMMA":
                    return VirtualKeyCode.OEM_COMMA;
                case "OEM_MINUS":
                    return VirtualKeyCode.OEM_MINUS;
                case "OEM_PERIOD":
                    return VirtualKeyCode.OEM_PERIOD;
                case "OEM_2":
                    return VirtualKeyCode.OEM_2;
                case "OEM_3":
                    return VirtualKeyCode.OEM_3;
                case "OEM_4":
                    return VirtualKeyCode.OEM_4;
                case "OEM_5":
                    return VirtualKeyCode.OEM_5;
                case "OEM_6":
                    return VirtualKeyCode.OEM_6;
                case "OEM_7":
                    return VirtualKeyCode.OEM_7;
                case "OEM_8":
                    return VirtualKeyCode.OEM_8;
                case "OEM_102":
                    return VirtualKeyCode.OEM_102;
                case "PROCESSKEY":
                    return VirtualKeyCode.PROCESSKEY;
                case "PACKET":
                    return VirtualKeyCode.PACKET;
                case "ATTN":
                    return VirtualKeyCode.ATTN;
                case "CRSEL":
                    return VirtualKeyCode.CRSEL;
                case "EXSEL":
                    return VirtualKeyCode.EXSEL;
                case "EREOF":
                    return VirtualKeyCode.EREOF;
                case "PLAY":
                    return VirtualKeyCode.PLAY;
                case "ZOOM":
                    return VirtualKeyCode.ZOOM;
                case "NONAME":
                    return VirtualKeyCode.NONAME;
                case "PA1":
                    return VirtualKeyCode.PA1;
                case "OEM_CLEAR":
                    return VirtualKeyCode.OEM_CLEAR;
                default:
                    return null;
            }
        }

        private static Keys KeysTranslate(string code)
        {
            switch (code)
            {
                case "LBUTTON":
                    return Keys.LButton;
                case "RBUTTON":
                    return Keys.RButton;
                case "CANCEL":
                    return Keys.Cancel;
                case "MBUTTON":
                    return Keys.MButton;
                case "XBUTTON1":
                    return Keys.XButton1;
                case "XBUTTON2":
                    return Keys.XButton2;
                case "BACK":
                    return Keys.Back;
                case "TAB":
                    return Keys.Tab;
                case "CLEAR":
                    return Keys.Clear;
                case "RETURN":
                    return Keys.Return;
                case "SHIFT":
                    return Keys.Shift;
                case "CONTROL":
                    return Keys.Control;
                case "MENU":
                    return Keys.Menu;
                case "PAUSE":
                    return Keys.Pause;
                case "CAPITAL":
                    return Keys.Capital;
                case "KANA":
                    return Keys.KanaMode;
                case "HANGEUL":
                    return Keys.HanguelMode;
                case "HANGUL":
                    return Keys.HangulMode;
                case "JUNJA":
                    return Keys.JunjaMode;
                case "FINAL":
                    return Keys.FinalMode;
                case "HANJA":
                    return Keys.HanjaMode;
                case "KANJI":
                    return Keys.KanjiMode;
                case "ESCAPE":
                    return Keys.Escape;
                case "CONVERT":
                    return Keys.IMEConvert;
                case "NONCONVERT":
                    return Keys.IMENonconvert;
                case "ACCEPT":
                    return Keys.IMEAccept;
                case "MODECHANGE":
                    return Keys.IMEModeChange;
                case "SPACE":
                    return Keys.Space;
                case "PRIOR":
                    return Keys.Prior;
                case "NEXT":
                    return Keys.Next;
                case "END":
                    return Keys.End;
                case "HOME":
                    return Keys.Home;
                case "LEFT":
                    return Keys.Left;
                case "UP":
                    return Keys.Up;
                case "RIGHT":
                    return Keys.Right;
                case "DOWN":
                    return Keys.Down;
                case "SELECT":
                    return Keys.Select;
                case "PRINT":
                    return Keys.Print;
                case "EXECUTE":
                    return Keys.Execute;
                case "SNAPSHOT":
                    return Keys.Snapshot;
                case "INSERT":
                    return Keys.Insert;
                case "DELETE":
                    return Keys.Delete;
                case "HELP":
                    return Keys.Help;
                case "VK_0":
                    return Keys.D0;
                case "VK_1":
                    return Keys.D1;
                case "VK_2":
                    return Keys.D2;
                case "VK_3":
                    return Keys.D3;
                case "VK_4":
                    return Keys.D4;
                case "VK_5":
                    return Keys.D5;
                case "VK_6":
                    return Keys.D6;
                case "VK_7":
                    return Keys.D7;
                case "VK_8":
                    return Keys.D8;
                case "VK_9":
                    return Keys.D9;
                case "VK_A":
                    return Keys.A;
                case "VK_B":
                    return Keys.B;
                case "VK_C":
                    return Keys.C;
                case "VK_D":
                    return Keys.D;
                case "VK_E":
                    return Keys.E;
                case "VK_F":
                    return Keys.F;
                case "VK_G":
                    return Keys.G;
                case "VK_H":
                    return Keys.H;
                case "VK_I":
                    return Keys.I;
                case "VK_J":
                    return Keys.J;
                case "VK_K":
                    return Keys.K;
                case "VK_L":
                    return Keys.L;
                case "VK_M":
                    return Keys.M;
                case "VK_N":
                    return Keys.N;
                case "VK_O":
                    return Keys.O;
                case "VK_P":
                    return Keys.P;
                case "VK_Q":
                    return Keys.Q;
                case "VK_R":
                    return Keys.R;
                case "VK_S":
                    return Keys.S;
                case "VK_T":
                    return Keys.T;
                case "VK_U":
                    return Keys.U;
                case "VK_V":
                    return Keys.V;
                case "VK_W":
                    return Keys.W;
                case "VK_X":
                    return Keys.X;
                case "VK_Y":
                    return Keys.Y;
                case "VK_Z":
                    return Keys.Z;
                case "LWIN":
                    return Keys.LWin;
                case "RWIN":
                    return Keys.RWin;
                case "APPS":
                    return Keys.Apps;
                case "SLEEP":
                    return Keys.Sleep;
                case "NUMPAD0":
                    return Keys.NumPad0;
                case "NUMPAD1":
                    return Keys.NumPad1;
                case "NUMPAD2":
                    return Keys.NumPad2;
                case "NUMPAD3":
                    return Keys.NumPad3;
                case "NUMPAD4":
                    return Keys.NumPad4;
                case "NUMPAD5":
                    return Keys.NumPad5;
                case "NUMPAD6":
                    return Keys.NumPad6;
                case "NUMPAD7":
                    return Keys.NumPad7;
                case "NUMPAD8":
                    return Keys.NumPad8;
                case "NUMPAD9":
                    return Keys.NumPad9;
                case "MULTIPLY":
                    return Keys.Multiply;
                case "ADD":
                    return Keys.Add;
                case "SEPARATOR":
                    return Keys.Separator;
                case "SUBTRACT":
                    return Keys.Subtract;
                case "DECIMAL":
                    return Keys.Decimal;
                case "DIVIDE":
                    return Keys.Divide;
                case "F1":
                    return Keys.F1;
                case "F2":
                    return Keys.F2;
                case "F3":
                    return Keys.F3;
                case "F4":
                    return Keys.F4;
                case "F5":
                    return Keys.F5;
                case "F6":
                    return Keys.F6;
                case "F7":
                    return Keys.F7;
                case "F8":
                    return Keys.F8;
                case "F9":
                    return Keys.F9;
                case "F10":
                    return Keys.F10;
                case "F11":
                    return Keys.F11;
                case "F12":
                    return Keys.F12;
                case "F13":
                    return Keys.F13;
                case "F14":
                    return Keys.F14;
                case "F15":
                    return Keys.F15;
                case "F16":
                    return Keys.F16;
                case "F17":
                    return Keys.F17;
                case "F18":
                    return Keys.F18;
                case "F19":
                    return Keys.F19;
                case "F20":
                    return Keys.F20;
                case "F21":
                    return Keys.F21;
                case "F22":
                    return Keys.F22;
                case "F23":
                    return Keys.F23;
                case "F24":
                    return Keys.F24;
                case "NUMLOCK":
                    return Keys.NumLock;
                case "SCROLL":
                    return Keys.Scroll;
                case "LSHIFT":
                    return Keys.LShiftKey;
                case "RSHIFT":
                    return Keys.RShiftKey;
                case "LCONTROL":
                    return Keys.LControlKey;
                case "RCONTROL":
                    return Keys.RControlKey;
                case "LMENU":
                    return Keys.LMenu;
                case "RMENU":
                    return Keys.RMenu;
                case "BROWSER_BACK":
                    return Keys.BrowserBack;
                case "BROWSER_FORWARD":
                    return Keys.BrowserForward;
                case "BROWSER_REFRESH":
                    return Keys.BrowserRefresh;
                case "BROWSER_STOP":
                    return Keys.BrowserStop;
                case "BROWSER_SEARCH":
                    return Keys.BrowserSearch;
                case "BROWSER_FAVORITES":
                    return Keys.BrowserFavorites;
                case "BROWSER_HOME":
                    return Keys.BrowserHome;
                case "VOLUME_MUTE":
                    return Keys.VolumeMute;
                case "VOLUME_DOWN":
                    return Keys.VolumeDown;
                case "VOLUME_UP":
                    return Keys.VolumeUp;
                case "MEDIA_NEXT_TRACK":
                    return Keys.MediaNextTrack;
                case "MEDIA_PREV_TRACK":
                    return Keys.MediaPreviousTrack;
                case "MEDIA_STOP":
                    return Keys.MediaStop;
                case "MEDIA_PLAY_PAUSE":
                    return Keys.MediaPlayPause;
                case "LAUNCH_MAIL":
                    return Keys.LaunchMail;
                case "LAUNCH_MEDIA_SELECT":
                    return Keys.SelectMedia;
                case "LAUNCH_APP1":
                    return Keys.LaunchApplication1;
                case "LAUNCH_APP2":
                    return Keys.LaunchApplication2;
                case "OEM_1":
                    return Keys.Oem1;
                case "OEM_PLUS":
                    return Keys.Oemplus;
                case "OEM_COMMA":
                    return Keys.Oemcomma;
                case "OEM_MINUS":
                    return Keys.OemMinus;
                case "OEM_PERIOD":
                    return Keys.OemPeriod;
                case "OEM_2":
                    return Keys.Oem2;
                case "OEM_3":
                    return Keys.Oem3;
                case "OEM_4":
                    return Keys.Oem4;
                case "OEM_5":
                    return Keys.Oem5;
                case "OEM_6":
                    return Keys.Oem6;
                case "OEM_7":
                    return Keys.Oem7;
                case "OEM_8":
                    return Keys.Oem8;
                case "OEM_102":
                    return Keys.Oem102;
                case "PROCESSKEY":
                    return Keys.ProcessKey;
                case "PACKET":
                    return Keys.Packet;
                case "ATTN":
                    return Keys.Attn;
                case "CRSEL":
                    return Keys.Crsel;
                case "EXSEL":
                    return Keys.Exsel;
                case "EREOF":
                    return Keys.EraseEof;
                case "PLAY":
                    return Keys.Play;
                case "ZOOM":
                    return Keys.Zoom;
                case "NONAME":
                    return Keys.NoName;
                case "PA1":
                    return Keys.Pa1;
                case "OEM_CLEAR":
                    return Keys.OemClear;
                default:
                    return new Keys();
            }
        }

        // Methods for editing Split List in the DataGridView
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;

        private void dgvSplits_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left)
                return;
            // If the mouse moves outside the rectangle, start the drag.
            if (dragBoxFromMouseDown != Rectangle.Empty &&
                !dragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                // Proceed with the drag and drop, passing in the list item.                    
                DragDropEffects dropEffect = dgvSplits.DoDragDrop(
                                                                  dgvSplits.Rows[rowIndexFromMouseDown],
                                                                  DragDropEffects.Move);
            }
        }

        private void dgvSplits_MouseDown(object sender, MouseEventArgs e)
        {
            // Get the index of the item the mouse is below.
            rowIndexFromMouseDown = dgvSplits.HitTest(e.X, e.Y).RowIndex;

            if (rowIndexFromMouseDown != -1)
            {
                // Remember the point where the mouse down occurred. 
                // The DragSize indicates the size that the mouse can move 
                // before a drag event should be started.                
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(
                          new Point(
                            e.X - (dragSize.Width / 2),
                            e.Y - (dragSize.Height / 2)),
                      dragSize);
            }
            else
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void dgvSplits_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dgvSplits_DragDrop(object sender, DragEventArgs e)
        {
            // The mouse locations are relative to the screen, so they must be 
            // converted to client coordinates.
            Point clientPoint = dgvSplits.PointToClient(new Point(e.X, e.Y));

            // Get the row index of the item the mouse is below. 
            rowIndexOfItemUnderMouseToDrop = dgvSplits.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If the drag operation was a move then remove and insert the row.
            if (e.Effect != DragDropEffects.Move)
                return;

            DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
            dgvSplits.Rows.RemoveAt(rowIndexFromMouseDown);
            dgvSplits.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);
        }

        private void dgvSplits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Insert)
                return;

            dgvSplits.Rows.Insert(dgvSplits.SelectedCells[0].RowIndex);
        }

        private void buttonAddList_Click(object sender, EventArgs e)
        {
            AddNewSplitList();
        }

        private void AddNewSplitList()
        {
            SaveConfig();

            SplitList newList = new SplitList
            {
                    Name = "New Split List",
                    Splits = new[]
                            {
                                    "sceneIndex: 1",
                                    "checkpointIndex: 2",
                                    "CollectScroll",
                                    "defeatedBosses: 1",
                                    "staff: 1",
                                    "ScrollCount: 15"
                            }
            };

            ListOfSplitLists.Add(newList);

            selectedSplitsIndex = ListOfSplitLists.Count - 1;

            LoadSplits();
        }

        private void buttonDeleteList_Click(object sender, EventArgs e)
        {
            SaveConfig();
            ListOfSplitLists.RemoveAt(selectedSplitsIndex);
            selectedSplitsIndex--;
            if (selectedSplitsIndex < 0)
                selectedSplitsIndex = 0;
            if (ListOfSplitLists.Count == 0)
                AddNewSplitList();
            LoadSplits();
        }

        private void comboBoxSplitListSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedSplitsIndex == comboBoxSplitListSelector.SelectedIndex
                || comboBoxSplitListSelector.SelectedIndex == -1)
                return;

            SaveConfig();
            selectedSplitsIndex = comboBoxSplitListSelector.SelectedIndex;
            LoadSplits();
        }

        private void comboBoxSplitListSelector_TextChanged(object sender, EventArgs e)
        {
            if (comboBoxSplitListSelector.SelectedIndex != -1)
                ListOfSplitLists[comboBoxSplitListSelector.SelectedIndex].Name = comboBoxSplitListSelector.Text;
            else
                ListOfSplitLists[selectedSplitsIndex].Name = comboBoxSplitListSelector.Text;
        }

        private void comboBoxSplitListSelector_DropDown(object sender, EventArgs e)
        {
            comboBoxSplitListSelector.Items.Clear();

            foreach (SplitList sl in ListOfSplitLists)
                comboBoxSplitListSelector.Items.Add(sl.Name);
        }

        private void checkBoxTopMost_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = checkBoxTopMost.Checked;
        }
    }

    public class SplitList
    {
        public string Name { get; set; }
        public string[] Splits { get; set; }
    }
}
