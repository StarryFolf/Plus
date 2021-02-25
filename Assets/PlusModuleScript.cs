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
    bool FirstTime = true,ModuleFirstTime=true;
    bool TimeModeActive;
    private bool TimeMode;
    private string[] colors;
    private static string[] exceptions = null;
    int SolvableModules;
    private int[] submission;
    private string submissionString;
    int pressCount = -1;

    int BatHld, Ind, PrtPlts, Unsolved, Solved, Modules, t;
    int Total = 0;
    char ThirdChar;

    int Time, StartingTime;

    int stage,remainder,SubmitStage = 0;

    bool ModuleSolved, processing = false;
    private bool AllModulesSolved,LightsOn;
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
        colors = new string[6] { "Red", "Blue", "Yellow", "Green", "Magenta", "Magenta" };
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
                Debug.LogFormat("[+ #{0}] All modules have been solved. Now going into conversion and submit phase.",_moduleId);
                plusBtnRender.material = plusBtnColor[0]; 
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
            else if (ModuleFirstTime&&!processing)
            {
                yield return new WaitForSeconds(20f);
                ModuleFirstTime = false;
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
            newColor = Random.Range(1, 6);
            while (oldColor == newColor) newColor = Random.Range(1, 6);
            oldColor = newColor;
            plusBtnRender.material = plusBtnColor[newColor];
            switch (newColor)
            {
                case 1:
                    Total += BatHld;
                    break;
                case 2:
                    Total += Ind;
                    break;
                case 3:
                    Total += PrtPlts;
                    break;
                case 4:
                    Solved = Info.GetSolvedModuleNames().Count();
                    Unsolved = Modules - Solved;
                    Total += Unsolved;
                    break;
                default:
                    Total += (int)Char.GetNumericValue(ThirdChar);
                    break;
            }
            Audio.PlaySoundAtTransform("Beep", Module.transform);
            if (t > 1) Debug.LogFormat("[+ #{0}] {1} minutes have passed. Your color is now {2} and your new total is {3}.", _moduleId, t, colors[newColor-1], Total);
            else Debug.LogFormat("[+ #{0}] A minute has passed. Your color is now {1} and your new total is {2}.", _moduleId, colors[newColor-1], Total);
            yield return new WaitForSeconds(60f);
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
            newColor = Random.Range(1, 6);
            while (oldColor == newColor) newColor = Random.Range(1, 6);
            oldColor = newColor;
            plusBtnRender.material = plusBtnColor[newColor];
            switch (newColor)
            {
                case 1:
                    Total += BatHld;
                    break;
                case 2:
                    Total += Ind;
                    break;
                case 3:
                    Total += PrtPlts;
                    break;
                case 4:
                    Solved = Info.GetSolvedModuleNames().Count();
                    Unsolved = Modules - Solved;
                    Total += Unsolved;
                    break;
                default:
                    Total += (int)Char.GetNumericValue(ThirdChar);
                    break;
            }
            Audio.PlaySoundAtTransform("Beep", Module.transform);
            if (t > 1) Debug.LogFormat("[+ #{0}] {1} minutes of bomb time have passed. Your color is now {2} and your new total is {3}.", _moduleId, t, colors[newColor - 1], Total);
            else Debug.LogFormat("[+ #{0}] A minute of bomb time has passed. Your color is now {1} and your new total is {2}.", _moduleId, colors[newColor - 1], Total);
        }
    }

    void Conversion()
    {
        if (Total==0)
        {
            SubmitSeq = "A";
        }
        while (Total>0)
        {
            remainder = Total % 25;
            if (remainder < 9) remainder += 65;
            else remainder += 66;
            SubmitSeq += (char)remainder;
            Total /= 25;
        }
        submission = new int[SubmitSeq.Length*2];
        Debug.LogFormat("[+ #{0}] The submit sequence is {1}.", _moduleId, SubmitSeq);
    }

    private void HandlePress()
    {
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
                if (SubmitStage == SubmitSeq.Length*2)
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

            submissionString += x > 4 || y > 4 ? "?" : Letters[y , x];
            chars += 2;
        }
        if (submissionString == SubmitSeq)
        {
            Debug.LogFormat("[+ #{0}] You submitted {1}. Thats correct!",_moduleId, submissionString);
            Debug.LogFormat("[+ #{0}] Module passed!", _moduleId);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, Module.transform);
            Module.HandlePass();
            ModuleSolved = true;
        }
        else
        {
            Debug.LogFormat("[+ #{0}] You Submitted {1}. Strike!",_moduleId, submissionString);
            Module.HandleStrike();
            ResetEntry();
        }
    }

    void ResetEntry()
    {
        SubmitStage = 0;
        pressCount = -1;
        submissionString = "";
        submission = new int[SubmitSeq.Length*2];
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "Tap your answer with !{0} tap XX XX.... Example: !{0} tap 23 11 34 32";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand (string command)
    {
        if (!AllModulesSolved) yield return null;
        foreach (char tap in command)
        {
            yield return "trycancel";

            int taps;
            if (!int.TryParse(tap.ToString(), out taps)) continue;
            for (int i = 0; i < taps; i++)
            {
                yield return plusBtn;
                yield return new WaitForSeconds(0.05f);
                yield return plusBtn;
                yield return new WaitForSeconds(0.05f);
                yield return "trycancel";
            }

            yield return new WaitUntil(() => paused);
            yield return new WaitForSeconds(0.1f);
        }

        yield return "trycancel";
        yield return plusBtn;
        yield return new WaitForSeconds(0.05f);
        yield return plusBtn;
        yield return new WaitForSeconds(0.05f);
    }
}
