using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using rnd = UnityEngine.Random;

public class faceOff : MonoBehaviour
{
    public new KMAudio audio;
    private KMAudio.KMAudioRef mainRef;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] rotationButtons;
    public KMSelectable submitButton;
    public Renderer[] faceSymbols;
    public Transform polyhedron;
    public Texture[] symbolsA;
    public Texture[] symbolsB;
    public TextMesh colorblindText;
    public Color[] polyhedronColors;
    public Color[] buttonColors;

    private int[] buttonColorIndices = new int[2];
    private char[] rearrangedAlphabet = new char[26];
    private int mismatchedPosition;
    private char fakeLetter;
    private char[] betweenLetters = new char[2];
    private int[] values = new int[2];

    private static readonly string[] colorNames = new[] { "cyan", "yellow", "magenta" };
    private static readonly string[] keywords = new[] { "SUCKLE", "FIDGET", "KNIGHT", "RINSED", "ALBINO", "SQUAWK", "KLUTZY", "DUVETS", "QUENCH" };
    private bool[] buttonsHeld = new bool[6];
    private float degrees = 3f;
    private bool TwitchPlaysActive;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in rotationButtons)
        {
            var ix = Array.IndexOf(rotationButtons, button);
            button.OnInteract += delegate ()
            {
                buttonsHeld[ix] = true;
                if (mainRef != null)
                {
                    mainRef.StopSound();
                    mainRef = null;
                }
                mainRef = audio.HandlePlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                StartCoroutine(Rotate(ix));
                return false;
            };
            button.OnInteractEnded += delegate
            {
                buttonsHeld[ix] = false;
                if (mainRef != null)
                {
                    mainRef.StopSound();
                    mainRef = null;
                }
                mainRef = audio.HandlePlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);
            };
        }
        submitButton.OnInteract += delegate () { Submit(); return false; };
        module.OnActivate += delegate ()
        {
            if (TwitchPlaysActive)
                degrees = 1f;
        };
    }

    private void Start()
    {
        polyhedron.localRotation = rnd.rotation;
        polyhedron.GetComponent<Renderer>().material.color = polyhedronColors.PickRandom();
        for (int i = 0; i < 26; i++)
        {
            var rot = faceSymbols[i].transform.localEulerAngles;
            faceSymbols[i].transform.localEulerAngles = new Vector3(rot.x, rot.y, rnd.Range(0f, 360f));
        }
        for (int i = 0; i < 2; i++)
            buttonColorIndices[i] = rnd.Range(0, 3);
        var buttonRenders = rotationButtons.Select(x => x.GetComponent<Renderer>()).ToArray();
        for (int i = 0; i < 6; i++)
            buttonRenders[i].material.color = i < 3 ? buttonColors[buttonColorIndices[0]] : buttonColors[buttonColorIndices[1]];
        Debug.LogFormat("[Face Off #{0}] The button colors are {1} on the left and {2} on the right.", moduleId, colorNames[buttonColorIndices[0]], colorNames[buttonColorIndices[1]]);
        colorblindText.text = GetComponent<KMColorblindMode>().ColorblindModeActive ? buttonColorIndices.Select(x => "CYM"[x]).Join("") : "";

        var keyword = keywords[buttonColorIndices[0] * 3 + buttonColorIndices[1]];
        Debug.LogFormat("[Face Off #{0}] The keyword is \"{1}\".", moduleId, keyword[0] + keyword.Substring(1).ToLowerInvariant());
        var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var restOfTheAlphabet = new string(alphabet.Where(c => !keyword.Contains(c)).ToArray());
        if (bomb.GetSerialNumberNumbers().First() % 2 == 0)
        {
            var tempArray = restOfTheAlphabet.ToCharArray();
            Array.Reverse(tempArray);
            restOfTheAlphabet = new string(tempArray);
        }
        rearrangedAlphabet = bomb.GetSerialNumberNumbers().Last() % 2 == 0 ? (keyword + restOfTheAlphabet).ToCharArray() : (restOfTheAlphabet + keyword).ToCharArray();
        Debug.LogFormat("[Face Off #{0}] The full rearranged alphabet is {1}.", moduleId, rearrangedAlphabet.Join(""));

        mismatchedPosition = rnd.Range(0, 26);
        fakeLetter = alphabet.Where(ch => rearrangedAlphabet[mismatchedPosition] != ch && rearrangedAlphabet[mismatchedPosition] % 2 == ch % 2).PickRandom();
        var secondarySymbolSet = new bool[26];
        for (int i = 0; i < 26; i++)
            secondarySymbolSet[i] = rnd.Range(0, 2) == 0;
        if (secondarySymbolSet[mismatchedPosition] == secondarySymbolSet[Array.IndexOf(rearrangedAlphabet, fakeLetter)])
            secondarySymbolSet[mismatchedPosition] = !secondarySymbolSet[mismatchedPosition];
        var shrinkTheseOnese = new[] { "C2", "R1" };
        var triangularFaces = new[] { 3, 5, 9, 11, 17, 19, 23, 25 };
        for (int i = 0; i < 26; i++)
        {
            faceSymbols[i].material.mainTexture = i != mismatchedPosition ? (secondarySymbolSet[i] ? symbolsA : symbolsB)[alphabet.IndexOf(rearrangedAlphabet[i])] : (secondarySymbolSet[i] ? symbolsA : symbolsB)[alphabet.IndexOf(fakeLetter)];
            var str = i != mismatchedPosition ? rearrangedAlphabet[i].ToString() : fakeLetter.ToString();
            str += secondarySymbolSet[i] ? "2" : "1";
            if (!triangularFaces.Contains(i) && shrinkTheseOnese.Contains(str))
                faceSymbols[i].transform.localScale = new Vector3(.45f, .45f, .45f);
        }
        Debug.LogFormat("[Face Off #{0}] Where a symbol corresponding to {1} should be displayed, one that corresponds to {2} can be found instead.", moduleId, rearrangedAlphabet[mismatchedPosition], fakeLetter);
        var average = (alphabet.IndexOf(fakeLetter) + alphabet.IndexOf(rearrangedAlphabet[mismatchedPosition])) / 2;
        betweenLetters[0] = alphabet[average];
        betweenLetters[1] = alphabet[(average + 13) % 26];
        values = betweenLetters.Select(x => alphabet.IndexOf(x) + 2).ToArray();
        Debug.LogFormat("[Face Off #{0}] {1} can be found between these two letters, so the numbers the time remaining can be divisible by are {2}.", moduleId, betweenLetters.Join(" and "), values.Join(" and "));
    }

    private IEnumerator Rotate(int ix)
    {
        while (buttonsHeld[ix])
        {
            switch (ix)
            {
                case 0:
                    polyhedron.RotateAround(polyhedron.position, Vector3.back, degrees);
                    break;
                case 1:
                    polyhedron.RotateAround(polyhedron.position, Vector3.right, degrees);
                    break;
                case 2:
                    polyhedron.RotateAround(polyhedron.position, Vector3.down, degrees);
                    break;
                case 3:
                    polyhedron.RotateAround(polyhedron.position, Vector3.forward, degrees);
                    break;
                case 4:
                    polyhedron.RotateAround(polyhedron.position, Vector3.left, degrees);
                    break;
                case 5:
                    polyhedron.RotateAround(polyhedron.position, Vector3.up, degrees);
                    break;
            }
            yield return null;
        }
    }

    private void Submit()
    {
        submitButton.AddInteractionPunch(.25f);
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submitButton.transform);
        if (moduleSolved)
            return;
        var submittedTime = (int)bomb.GetTime();
        var div1 = submittedTime % values[0] == 0;
        var div2 = submittedTime % values[1] == 0;
        Debug.LogFormat("[Face Off #{0}] You pressed the submit button at {1} ({2} seconds remaining). This is{3} divisible by {4}, and is{5} divisible by {6}.", moduleId, bomb.GetFormattedTime(), submittedTime, div1 ? "" : " not", values[0], div2 ? "" : " not", values[1]);
        if (submittedTime < values.Min())
        {
            Debug.LogFormat("[Face Off #{0}] This time is lower than {1}. Striking and solving simultaneously.", moduleId, values.Min());
            StartCoroutine(StrikeAndThenSolve());
        }
        else if (div1 ^ div2)
        {
            Debug.LogFormat("[Face Off #{0}] The time remaining was divisibly by exactly one of the values, so this was a valid time. Module solved!", moduleId);
            moduleSolved = true;
            module.HandlePass();
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            polyhedron.Find("led base").Find("led").GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            Debug.LogFormat("[Face Off #{0}] The time remaining was divisible by {1} of the values, so this was an invalid time.", moduleId, div1 ? "both" : "neither");
            module.HandleStrike();
        }
    }

    private IEnumerator StrikeAndThenSolve()
    {
        module.HandleStrike();
        yield return new WaitForSeconds(.1f);
        module.HandlePass();
        polyhedron.Find("led base").Find("led").GetComponent<Renderer>().material.color = Color.yellow;
    }

    // Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} <C/U/R/W/D/L> # [Presses the rotation button with that label for # seconds, can be chained e.g. !{0} C 5 D 4] !{0} submit 58 [Presses the red button the next time the seconds digits of the timer are 58]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string input)
    {
        input = input.ToUpperInvariant().Trim();
        var inputArray = input.Split(' ');
        var validButtons = new[] { "C", "U", "R", "W", "D", "L" };
        Match m = Regex.Match(input, @"^(?:submit (\d{1,2}))$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            yield return null;
            var value = int.Parse(m.Groups[1].Value);
            if (value >= 0 && value < 60)
            {
                while ((int)bomb.GetTime() % 60 != value)
                {
                    yield return null;
                    yield return "trycancel";
                };
            }
            else
            {
                yield return "sendtochaterror Invalid time.";
                yield break;
            }
            submitButton.OnInteract();
        }
        else if (inputArray.Length % 2 == 0)
        {
            var holdCommands = new List<string[]>();
            var count = inputArray.Length / 2;
            var times = new float[count];
            for (int i = 0; i < count; i++)
                holdCommands.Add(inputArray.Skip(2 * i).Take(2).ToArray());
            for (int i = 0; i < count; i++)
                if (!validButtons.Contains(holdCommands[i][0]) || !float.TryParse(holdCommands[i][1], out times[i]))
                    yield break;
            yield return null;
            for (int i = 0; i < count; i++)
            {
                rotationButtons[Array.IndexOf(validButtons, holdCommands[i][0])].OnInteract();
                yield return new WaitForSeconds(times[i]);
                rotationButtons[Array.IndexOf(validButtons, holdCommands[i][0])].OnInteractEnded();
                yield return new WaitForSeconds(1f);
            }
        }
        else
            yield break;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!((int)bomb.GetTime() % values[0] == 0 ^ (int)bomb.GetTime() % values[1] == 0))
        {
            yield return true;
            yield return null;
        }
        yield return null;
        submitButton.OnInteract();
    }
}
