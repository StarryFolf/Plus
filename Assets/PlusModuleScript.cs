using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;
using Array = System.Array;
using KModkit;

public class PlusModuleScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Info;
    public KMBombModule Module;
    public KMSelectable plusBtn;
    public MeshRenderer plusBtnRender;
    public KMBossModule boss;
    public Material[] plusBtnColor;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    int newColor, oldColor;
    bool FirstTime = true, ModuleFirstTime = true;
    bool TimeModeActive;
    private bool TimeMode;
    private string[] colors;
    private static string[] exceptions = null;
    int SolvableModules;
    private int[] submission,store;
    private string submissionString;
    int pressCount = -1;

    int BatHld, Ind, PrtPlts, Unsolved, Solved, Modules, t;
    int Total = 0;
    char ThirdChar;

    int Time, StartingTime;

    int stage, remainder, SubmitStage = 0, TotalStages = 0, CurrentStage = 0;

    bool ModuleSolved, processing = false;
    private bool AllModulesSolved, LightsOn;
    bool holding, paused;

    string SubmitSeq;
    private static readonly string[,] Letters = new string[5, 5] {
        { "A", "B", "C", "D", "E" },
        { "F", "G", "H", "I", "J" },
        { "L", "M", "N", "O", "P" },
        { "Q", "R", "S", "T", "U" },
        { "V", "W", "X", "Y", "Z" }
    };

    Coroutine ReleaseCoroutine = null;

    // Use this for initialization
    void Start()
    {
        _moduleId = _moduleIdCounter++;
        exceptions = boss.GetIgnoredModules("+", new string[]
        {
            "+",
            "14",
            "42",
            "501",
            "A>N<D",
            "Bamboozling Time Keeper",
            "Brainf---",
            "Busy Beaver",
            "Don't Touch Anything",
            "Forget Any Color",
            "Forget Enigma",
            "Forget Everything",
            "Forget Infinity",
            "Forget It Not",
            "Forget Me Later",
            "Forget Me Not",
            "Forget Perspective",
            "Forget The Colors",
            "Forget Them All",
            "Forget This",
            "Forget Us Not",
            "Iconic",
            "Keypad Directionality",
            "Kugelblitz",
            "Multitask",
            "OmegaDestroyer",
            "OmegaForget",
            "Organization",
            "Password Destroyer",
            "Purgatory",
            "RPS Judging",
            "Security Council",
            "Shoddy Chess",
            "Simon Forgets",
            "Simon's Stages",
            "Souvenir",
            "Tallordered Keys",
            "The Time Keeper",
            "The Troll",
            "The Twin",
            "The Very Annoying Button",
            "Timing is Everything",
            "Turn The Key",
            "Ultimate Custom Night",
            "Übermodule",
            "Whiteout",
            "Forget Maze Not"
        });
        //Somehow Magenta doesn't appear when Range is (1,5)
        colors = new string[5] { "Red", "Blue", "Yellow", "Green", "Magenta"};
        plusBtn.OnInteract += delegate ()
        {
            HandlePress();
            return false;
        };
        plusBtn.OnInteractEnded += HandleRelease;
        Module.OnActivate += TimeModeCheck;
        Module.OnActivate += Activate;
    }

    void TimeModeCheck()
    {
        if (TimeModeActive) TimeMode = true;
    }

    void Activate()
    {
        if (Info.GetSolvableModuleNames().Where(x => !exceptions.Contains(x)).Count() == 0)
        {
            Debug.LogFormat("[+ #{0}] No solvable modules. Commencing auto solve.", _moduleId);
            ModuleSolved = true;
            Module.HandlePass();
        }
        else
        {
            store = new int[1000];
            LightsOn = true;
            stage = 0;
            SolvableModules = Info.GetSolvableModuleNames().Where(x => !exceptions.Contains(x)).Count();
            BatHld = Info.GetBatteryHolderCount();
            PrtPlts = Info.GetPortPlateCount();
            Modules = Info.GetModuleNames().Count;
            ThirdChar = Info.GetSerialNumber()[2];
            Ind = Info.GetIndicators().Count();
            StartingTime = (int)Info.GetTime();
            SubmitSeq = "";
            submissionString = "";
            if (TimeMode)
            {
                StartCoroutine("ChooseColor");
            }
        }
    }

    void Update()
    {
        int progress = Info.GetSolvedModuleNames().Where(x => !exceptions.Contains(x)).Count();
        if (progress > stage && !AllModulesSolved)
        {
            stage++;
            if (stage >= SolvableModules)
            {
                Debug.LogFormat("[+ #{0}] All modules have been solved. Now going into conversion and submit phase.", _moduleId);
                plusBtnRender.material = plusBtnColor[5];
                AllModulesSolved = true;
                StopCoroutine("ChooseColor");
                StopCoroutine("NormalModeCC");
                stage = 0;
                Conversion();
            }
        }
        Time = (int)Info.GetTime();
        if (LightsOn && !AllModulesSolved && !TimeMode) StartCoroutine("NormalModeCC");
    }

    IEnumerator NormalModeCC()
    {
        if (Time % 60 == 0)
        {
            if (Time == StartingTime && !processing)
            {
                ModuleFirstTime = false;
                yield break;
            }
            else if (ModuleFirstTime && !processing)
            {
                processing = true;
                yield return new WaitForSeconds(20f);
                ModuleFirstTime = false;
                processing = false;
            }
            else if (!processing)
            {
                processing = true;
                StartCoroutine("ChooseColor");
                yield return new WaitForSeconds(2f);
                processing = false;
            }
        }
    }

    IEnumerator ChooseColor()
    {
        if (TimeMode)
        {
            if (FirstTime)
            {
                oldColor = 0;
                FirstTime = false;
                yield return new WaitForSeconds(61f);
            }
            t++;
            TotalStages++;
            newColor = Random.Range(0, 5);
            while (oldColor == newColor) newColor = Random.Range(0, 5);
            oldColor = newColor;
            plusBtnRender.material = plusBtnColor[newColor];
            store[CurrentStage] = newColor;
            switch (newColor)
            {
                case 0:
                    Total += BatHld;
                    break;
                case 1:
                    Total += Ind;
                    break;
                case 2:
                    Total += PrtPlts;
                    break;
                case 3:
                    Solved = Info.GetSolvedModuleNames().Count();
                    Unsolved = Modules - Solved;
                    Total += Unsolved;
                    break;
                default:
                    Total += (int)Char.GetNumericValue(ThirdChar);
                    break;
            }
            Audio.PlaySoundAtTransform("Beep", Module.transform);
            if (t > 1) Debug.LogFormat("[+ #{0}] {1} minutes have passed. Your color is now {2} and your new total is {3}.", _moduleId, t, colors[newColor], Total);
            else Debug.LogFormat("[+ #{0}] A minute has passed. Your color is now {1} and your new total is {2}.", _moduleId, colors[newColor], Total);
            yield return new WaitForSeconds(60f);
            CurrentStage++;
            StartCoroutine("ChooseColor");
        }
        else
        {
            if (FirstTime)
            {
                oldColor = 0;
                FirstTime = false;
            }
            t++;
            TotalStages++;
            newColor = Random.Range(0, 5);
            while (oldColor == newColor) newColor = Random.Range(0, 5);
            oldColor = newColor;
            plusBtnRender.material = plusBtnColor[newColor];
            store[CurrentStage] = newColor;
            switch (newColor)
            {
                case 0:
                    Total += BatHld;
                    break;
                case 1:
                    Total += Ind;
                    break;
                case 2:
                    Total += PrtPlts;
                    break;
                case 3:
                    Solved = Info.GetSolvedModuleNames().Count();
                    Unsolved = Modules - Solved;
                    Total += Unsolved;
                    break;
                default:
                    Total += (int)Char.GetNumericValue(ThirdChar);
                    break;
            }
            Audio.PlaySoundAtTransform("Beep", Module.transform);
            if (t > 1) Debug.LogFormat("[+ #{0}] {1} minutes of bomb time have passed. Your color is now {2} and your new total is {3}.", _moduleId, t, colors[newColor], Total);
            else Debug.LogFormat("[+ #{0}] A minute of bomb time has passed. Your color is now {1} and your new total is {2}.", _moduleId, colors[newColor], Total);
            CurrentStage++;
        }
    }

    void Conversion()
    {
        if (Total == 0)
        {
            SubmitSeq = "A";
        }
        while (Total > 0)
        {
            remainder = Total % 25;
            if (remainder <= 9) remainder += 65;
            else remainder += 66;
            SubmitSeq += (char)remainder;
            Total /= 25;
        }
        submission = new int[SubmitSeq.Length * 2];
        Debug.LogFormat("[+ #{0}] The submit sequence is {1}.", _moduleId, SubmitSeq);
    }

    private void HandlePress()
    {
        StopCoroutine("StageRecover");
        plusBtnRender.material = plusBtnColor[5];
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, plusBtn.transform);
        plusBtn.AddInteractionPunch();
        if (ReleaseCoroutine != null)
        {
            StopCoroutine(ReleaseCoroutine);
        }
        if (!AllModulesSolved) return;
        if (ModuleSolved) return;
        if (!LightsOn) return;
        holding = true;
    }

    private void HandleRelease()
    {
        if (!holding || !LightsOn) return;
        if (ModuleSolved) return;
        holding = false;
        if (paused)
        {
            if (pressCount != -1)
            {
                submission[SubmitStage] = pressCount;
                SubmitStage++;
                if (SubmitStage == SubmitSeq.Length * 2)
                {
                    AnsCheck();
                    return;
                }
            }
            pressCount = -1;
            paused = false;
        }
        pressCount++;
        ReleaseCoroutine = StartCoroutine(StartButtonReleaseTimer());
    }

    private IEnumerator StartButtonReleaseTimer()
    {
        double t = 0;
        while (t < 1)
        {
            yield return new WaitForSeconds(0.1f);
            t += 0.1;
        }
        paused = true;
        Audio.PlaySoundAtTransform("Tap", Module.transform);
    }

    private void AnsCheck()
    {
        int chars = 0;

        for (int c = 0; c < SubmitSeq.Length; c++)
        {
            int y = submission[chars];
            int x = submission[chars + 1];

            submissionString += x > 4 || y > 4 ? "?" : Letters[y, x];
            chars += 2;
        }
        if (submissionString == SubmitSeq)
        {
            Debug.LogFormat("[+ #{0}] You submitted {1}. Thats correct!", _moduleId, submissionString);
            Debug.LogFormat("[+ #{0}] Module passed!", _moduleId);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, Module.transform);
            Module.HandlePass();
            ModuleSolved = true;
        }
        else
        {
            Debug.LogFormat("[+ #{0}] You Submitted {1}. Strike!", _moduleId, submissionString);
            Module.HandleStrike();
            ResetEntry();
            StartCoroutine("StageRecover");
        }
    }

    IEnumerator StageRecover()
    {
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < TotalStages; i++)
        {
            Audio.PlaySoundAtTransform("Tap", Module.transform);
            plusBtnRender.material = plusBtnColor[store[i]];
            yield return new WaitForSeconds(1.5f);
        }
        Audio.PlaySoundAtTransform("Tap", Module.transform);
        plusBtnRender.material = plusBtnColor[5];
        StartCoroutine("StageRecover");
    }

    void ResetEntry()
    {
        SubmitStage = 0;
        pressCount = -1;
        submissionString = "";
        submission = new int[SubmitSeq.Length * 2];
    }

    //twitch plays
    #pragma warning disable 414
    private string TwitchHelpMessage = "Tap your answer with !{0} tap XX XX.... Example: !{0} tap 23 11 34 32";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (!AllModulesSolved)
        {
            yield return "sendtochaterror All other modules have not been solved yet!";
            yield break;
        }
        if (command.StartsWith("tap "))
        {
            int[] valids = { 11, 12, 13, 14, 15, 21, 22, 23, 24, 25, 31, 32, 33, 34, 35, 41, 42, 43, 44, 45, 51, 52, 53, 54, 55 };
            foreach (string tap in command.Split(' '))
            {
                if (tap != "tap")
                {
                    int taps;
                    if (!int.TryParse(tap.ToString(), out taps))
                    {
                        yield return "sendtochaterror The pair of numbers '" + tap.ToString() + "' is invalid!";
                        yield break;
                    }
                    if (!valids.Contains(taps))
                    {
                        yield return "sendtochaterror The pair of numbers '" + taps + "' is invalid!";
                        yield break;
                    }
                }
            }
            foreach (string tap in command.Split(' '))
            {
                if (tap != "tap")
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int i = 0; i < int.Parse(tap[j].ToString()); i++)
                        {
                            plusBtn.OnInteract();
                            yield return new WaitForSeconds(0.05f);
                            plusBtn.OnInteractEnded();
                            yield return new WaitForSeconds(0.05f);
                        }
                        yield return new WaitUntil(() => paused);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            string tempSub = "";
            int chars = 0;
            for (int c = 0; c < SubmitSeq.Length; c++)
            {
                int y = submission[chars];
                int x = submission[chars + 1];

                tempSub += x > 4 || y > 4 ? "?" : Letters[y, x];
                chars += 2;
            }
            if (tempSub == SubmitSeq)
            {
                yield return "awardpointsonsolve " + TotalStages;
            }
            plusBtn.OnInteract();
            yield return new WaitForSeconds(0.05f);
            plusBtn.OnInteractEnded();
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!LightsOn || !AllModulesSolved) yield return true;
        List<int> corrects = new List<int>();
        for (int c = 0; c < SubmitSeq.Length; c++)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int k = 0; k < 5; k++)
                {
                    if (Letters[i, k] == SubmitSeq[c].ToString())
                    {
                        corrects.Add(i + 1);
                        corrects.Add(k + 1);
                        if (c == SubmitSeq.Length - 1)
                            goto proceed;
                        else
                            goto nextChar;
                    }
                }
            }
            nextChar:
                yield return null;
        }
        proceed:
        for (int i = 0; i < corrects.Count; i++)
        {
            if (submission[i] == 0)
                break;
            if (corrects[i] != submission[i])
            {
                Module.HandlePass();
                ModuleSolved = true;
                yield break;
            }
        }
        if (pressCount > corrects[SubmitStage])
        {
            Module.HandlePass();
            ModuleSolved = true;
            yield break;
        }
        int start = SubmitStage;
        for (int c = start; c < corrects.Count; c++)
        {
            int start2;
            if (c == start && (pressCount != -1))
                start2 = pressCount;
            else
                start2 = 0;
            for (int l = start2; l < corrects[c]; l++)
            {
                plusBtn.OnInteract();
                yield return new WaitForSeconds(0.05f);
                plusBtn.OnInteractEnded();
                yield return new WaitForSeconds(0.05f);
            }
            while (!paused) yield return true;
            yield return new WaitForSeconds(0.1f);
        }
        plusBtn.OnInteract();
        yield return new WaitForSeconds(0.05f);
        plusBtn.OnInteractEnded();
        yield return new WaitForSeconds(0.05f);
    }
}