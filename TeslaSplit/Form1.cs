using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using WMPLib;
using WindowsInput;
using System.Text;

namespace TeslaSplit
{
    public partial class Form1 : Form
    {
        private FileSystemWatcher fsw;
        private string previousText;
        private string savePath;
        private string splitHotkey;
        private string resetHotkey;
        private string splitList;
        private bool splitFlag;
        private string[] splits;
        private int splitCounter;
        private short scrollCounter;
        private string[] scenes;
        private WindowsMediaPlayer HamsterDance;
        private StringBuilder sbText; 

        public Form1()
        {
            InitializeComponent();
            savePath = "";
            splitHotkey = "";
            splitList = "";
            tbTitle.Text = "";
            labelScene.Text = "";
            labelSceneName.Text = "";
            labelCheckpoint.Text = "";
            labelGlove.Text = "";
            labelBlink.Text = "";
            labelSuit.Text = "";
            labelStaff.Text = "";
            labelBarriers.Text = "";
            labelOrbs.Text = "";
            labelScrollCount.Text = "";
            labelBosses.Text = "";
            labelComplete.Text = "";
            scenes = new[] { "Home", "Scales", "Rooftops", "Broken Bridge", "Chimneys", "Balconies", "Stave Church", "Moat", "Courtyard", "Classroom", "Levitation", "Pistons", "Chapel", "Barrier", "Trials", "Well", "Thunderbolt", "Iron Lice", "Magic Carpet", "Snakeway", "Fernus", "Maglev", "Hidey Hole", "Fernus", "Cooling Room", "Cages", "Magnetflies", "Grues", "Waterworks", "Waterworks", "Roots", "Act One", "Heartwood", "Maze", "Mural", "Ventilation", "Faradeus", "Shrine", "Wintergarden", "Wintergarden", "Wintergarden", "Wintergarden", "Wintergarden", "Wintergarden", "Storage", "Act Two", "Scrapyard", "Crusher", "Smelter", "Molten Pool", "Fireproof", "Forge", "Licemover", "Pipes", "Chokepoint", "Brazen Bull", "Magnetbridge", "Guardian", "Electromagnets", "Clerestory", "Oleg", "Act Three", "Control Room", "Wheeltrack", "Acrobatics", "Race", "Feast hall", "Happy Ending", "Magnetic Lift", "Magnetic Ball", "Fatal Attraction", "Surprise", "Alternation", "Grand Design", "Solomon Tesla", "Pinnacle", "Guerickes Orb", "Dodge This", "Deep Down", "Hidden Library", "Tower", "Tanngrisne", "Tanngnjost", "Bridge", "Room", "Stormfront", "Palace Stairs", "Downfall", "Passage", "Scrolls", "Dungeon", "Scrolls", "Secret Passage", "Grand Hall", "The King", "Cooler", "Assembler", "Forge", "Homage", "Crown Space", "Home", "Scales", "Rooftops", "Broken Bridge", "Chimneys", "Balconies", "Stave Church", "Moat", "Tower", "Tower", "Tower", "Tower", "Tower", "Tower" };
            HamsterDance = new WindowsMediaPlayer();
            sbText = new StringBuilder();


            try
            {
                // Read the app config settings
                savePath = ConfigurationManager.AppSettings["SaveGamePath"];
                splitHotkey = ConfigurationManager.AppSettings["SplitHotkey"];
                resetHotkey = ConfigurationManager.AppSettings["ResetHotkey"];
                splitList = ConfigurationManager.AppSettings["SplitList"];

                // Set up the file watcher
                fsw = new FileSystemWatcher
                {
                    Path = CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), savePath),
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = "SavedGame.asset",
                    IncludeSubdirectories = true
                };
                fsw.Changed += fsw_Changed;
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("{0}: {1}", e.GetType(), e.Message));
                Application.Exit();
            }
            finally
            {
                // If the app config has issues, report them. Else, turn on the file watcher.
                if (savePath == "")
                {
                    buttonStopStart.Enabled = false;
                    MessageBox.Show("SaveGamePath is missing from app config.");
                }
                else if (splitHotkey == "")
                {
                    buttonStopStart.Enabled = false;
                    MessageBox.Show("SplitHotkey is missing from app config.");
                }
                else if (splitList == "")
                {
                    buttonStopStart.Enabled = false;
                    MessageBox.Show("SplitList is missing from app config.");
                }
                else
                {
                    splits = splitList.Split(',').Select(s => s.Trim()).ToArray();
                    splitCounter = 0;
                    scrollCounter = 0;
                    previousText = "";
                    fsw.EnableRaisingEvents = true;
                    buttonStopStart.Text = "Stop Watching";
                    tbTitle.Text = String.Format("Watching the directory: {0}", fsw.Path);
                }
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


        private void fsw_Changed(object source, FileSystemEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.FullPath != savePath)
                {
                    // A new saved game has been opened
                    savePath = eventArgs.FullPath;
                    previousText = "";
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

                buttonReset.Enabled = true;

                // Populate labels
                string[] lines = Regex.Split(currentText, "\r\n");
                foreach (string line in lines)
                {
                    string l = line.Trim();
                    if (line.Contains("sceneIndex:"))
                    {
                        labelScene.Text = l;
                        int sceneNumber = Int32.Parse(l.Substring(l.LastIndexOf(' ') + 1));
                        if (sceneNumber < scenes.Count())
                            labelSceneName.Text = scenes[sceneNumber];
                    }
                    else if (line.Contains("checkpointIndex:"))
                        labelCheckpoint.Text = l;
                    else if (line.Contains("glove:"))
                        labelGlove.Text = l;
                    else if (line.Contains("blink:"))
                        labelBlink.Text = l;
                    else if (line.Contains("suit:"))
                        labelSuit.Text = l;
                    else if (line.Contains("staff:"))
                        labelStaff.Text = l;
                    else if (line.Contains("openBarriers:"))
                        labelBarriers.Text = l;
                    else if (line.Contains("orbsFound:"))
                        labelOrbs.Text = l;
                    else if (line.Contains("defeatedBosses:"))
                        labelBosses.Text = l;
                    else if (line.Contains("gameComplete:"))
                        labelComplete.Text = l;
                }

                if (previousText == "")
                {
                    tbTitle.Text = String.Format("Loaded {0}\r\n", eventArgs.FullPath);
                }
                else
                {
                    if (splitCounter >= splits.Count())
                        return;

                    string currentSplit = splits[splitCounter];
                    splitFlag = false;

                    tbTitle.Text = "";
                    // Get differences between previousText and currentText
                    Differ d = new Differ();
                    InlineDiffBuilder idb = new InlineDiffBuilder(d);
                    var result = idb.BuildDiffModel(previousText, currentText);

                    // Now analyze for splits
                    foreach (string lineText in result.Lines.Where(t => t.Type == ChangeType.Inserted).Select(line => line.Text.Trim()))
                    {
                        sbText.AppendLine(String.Format("[{0}] {1}", DateTime.Now.ToString("s"), lineText));

                        if (currentSplit.Trim() == lineText)
                            SendSplit();

                        else if (lineText.Contains("orbsFound:") && lineText != "orbsFound:")
                        {
                            scrollCounter++;
                            labelScrollCount.Text = String.Format("Scrolls Collected: {0}", scrollCounter);
                            if (currentSplit == "CollectScroll" || currentSplit == "ScrollCount: " + scrollCounter)
                                SendSplit();
                        }

                        else if (lineText.Contains("openBarriers:") && currentSplit == "BarrierChange")
                            SendSplit();

                        // Hamster Dance Easter Egg!
                        // Play the mp3 during scenes 63, 64, 65, and stop upon reaching scene 98
                        if (lineText.Contains("sceneIndex: 63") && File.Exists("hamsterdance.mp3"))
                        {
                            sbText.AppendLine(String.Format("[{0}] {1}", DateTime.Now.ToString("s"), "Hamster Dance!"));
                            HamsterDance.URL = "hamsterdance.mp3";
                            HamsterDance.controls.play();
                        }

                        if (lineText.Contains("sceneIndex: 98") && HamsterDance.playState == WMPPlayState.wmppsPlaying)
                        {
                            HamsterDance.controls.stop();
                        }
                    }

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
                    if (splitFlag)
                    {
                        sbText.AppendLine(splitCounter < splits.Count()
                                                  ? String.Format("[{0}] Next Split is {1}", DateTime.Now.ToString("s"),
                                                                  splits[splitCounter])
                                                  : String.Format("[{0}] End of splits", DateTime.Now.ToString("s")));
                    }

                }
                tbTitle.AppendText(sbText.ToString());
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
            fsw.EnableRaisingEvents = !fsw.EnableRaisingEvents;
            buttonStopStart.Text = fsw.EnableRaisingEvents ? "Stop Watching" : "Start Watching";
            tbTitle.Text = fsw.EnableRaisingEvents ? String.Format("Watching the directory: {0}", fsw.Path) : "Stopped";

            if (HamsterDance.playState == WMPPlayState.wmppsPlaying)
                HamsterDance.controls.stop();
        }

        private void SendSplit()
        {
            if (splitFlag)
                return;

            splitFlag = true;
            splitCounter++;
            sbText.AppendLine(String.Format("[{0}] {1}", DateTime.Now.ToString("s"), "Sending a Split"));
            VirtualKeyCode? vkc = VKCTranslate(splitHotkey);
            if (vkc == null)
                return;

            InputSimulator.SimulateKeyDown((VirtualKeyCode) vkc);
            Thread.Sleep(500);
            InputSimulator.SimulateKeyUp((VirtualKeyCode) vkc);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            savePath = "";
            tbTitle.Text = "Reset!";
            labelScene.Text = "";
            labelSceneName.Text = "";
            labelCheckpoint.Text = "";
            labelGlove.Text = "";
            labelBlink.Text = "";
            labelSuit.Text = "";
            labelStaff.Text = "";
            labelBarriers.Text = "";
            labelOrbs.Text = "";
            labelScrollCount.Text = "";
            labelBosses.Text = "";
            labelComplete.Text = "";
            splitCounter = 0;
            scrollCounter = 0;
            previousText = "";
            sbText = new StringBuilder();
            buttonReset.Enabled = false;
            if (fsw.EnableRaisingEvents)
                tbTitle.Text = String.Format("Watching the directory: {0}", fsw.Path);

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
    }
}
