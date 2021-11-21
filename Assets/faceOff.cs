using KModkit;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;

public class faceOff : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] rotationButtons;
    public Renderer[] faceSymbols;
    public Transform polyhedron;
    public Texture[] symbolsA;
    public Texture[] symbolsB;
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

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in rotationButtons)
        {
            var ix = Array.IndexOf(rotationButtons, button);
            button.OnInteract += delegate () { buttonsHeld[ix] = true; audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform); StartCoroutine(Rotate(ix)); return false; };
            button.OnInteractEnded += delegate { buttonsHeld[ix] = false; audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform); };
        }
    }

    private void Start()
    {
        polyhedron.localRotation = rnd.rotation;
        for (int i = 0; i < 2; i++)
            buttonColorIndices[i] = rnd.Range(0, 3);
        var buttonRenders = rotationButtons.Select(x => x.GetComponent<Renderer>()).ToArray();
        for (int i = 0; i < 6; i++)
            buttonRenders[i].material.color = i < 3 ? buttonColors[buttonColorIndices[0]] : buttonColors[buttonColorIndices[1]];
        Debug.LogFormat("[Face Off #{0}] The button colors are {1} on the left and {2} on the right.", moduleId, colorNames[buttonColorIndices[0]], colorNames[buttonColorIndices[1]]);

        var keyword = keywords[buttonColorIndices[0] * 3 + buttonColorIndices[1]];
        Debug.LogFormat("[Face Off #{0}] The keyword is {1}.", moduleId, keyword[0] + keyword.Substring(1).ToLowerInvariant());
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
        fakeLetter = alphabet.PickRandom();
        while (Math.Abs(alphabet.IndexOf(fakeLetter) - alphabet.IndexOf(rearrangedAlphabet[mismatchedPosition])) % 2 != 0)
            fakeLetter = alphabet.PickRandom();
        Debug.Log(alphabet.IndexOf(fakeLetter) + ", " + alphabet.IndexOf(rearrangedAlphabet[mismatchedPosition])); // TEMP
        var secondarySymbolSet = new bool[26];
        for (int i = 0; i < 26; i++)
            secondarySymbolSet[i] = rnd.Range(0, 2) == 0;
        while (secondarySymbolSet[mismatchedPosition] == secondarySymbolSet[Array.IndexOf(rearrangedAlphabet, fakeLetter)])
            secondarySymbolSet[mismatchedPosition] = rnd.Range(0, 2) == 0;
        for (int i = 0; i < 26; i++)
            faceSymbols[i].material.mainTexture = i != mismatchedPosition ? (secondarySymbolSet[i] ? symbolsA : symbolsB)[alphabet.IndexOf(rearrangedAlphabet[i])] : (secondarySymbolSet[i] ? symbolsA : symbolsB)[alphabet.IndexOf(fakeLetter)];
        Debug.LogFormat("[Face Off #{0}] Where a symbol corresponding to {1} should be displayed, one that corresponds to {2} can be found instead.", moduleId, rearrangedAlphabet[mismatchedPosition], fakeLetter);
        var average = (alphabet.IndexOf(fakeLetter) + alphabet.IndexOf(rearrangedAlphabet[mismatchedPosition])) / 2;
        betweenLetters[0] = alphabet[average];
        betweenLetters[1] = alphabet[(average + 13) % 26];
        values = betweenLetters.Select(x => alphabet.IndexOf(x) + 2).ToArray();
        Debug.LogFormat("[Face Off #{0}] {1} can be found between these two letters, so the numbers the time remaining can be divisible by are {2}.", moduleId, betweenLetters.Join(" and "), values.Join(" and "));
        Debug.Log((int)bomb.GetTime());
    }

    private IEnumerator Rotate(int ix)
    {
        while (buttonsHeld[ix])
        {
            switch (ix)
            {
                case 0:
                    polyhedron.RotateAround(polyhedron.position, Vector3.up, 3);
                    break;
                case 1:
                    polyhedron.RotateAround(polyhedron.position, Vector3.right, 3);
                    break;
                case 2:
                    polyhedron.RotateAround(polyhedron.position, Vector3.back, 3);
                    break;
                case 3:
                    polyhedron.RotateAround(polyhedron.position, Vector3.down, 3);
                    break;
                case 4:
                    polyhedron.RotateAround(polyhedron.position, Vector3.left, 3);
                    break;
                case 5:
                    polyhedron.RotateAround(polyhedron.position, Vector3.forward, 3);
                    break;
            }
            yield return null;
        }
    }

    // Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} ";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string input)
    {
        yield return null;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
