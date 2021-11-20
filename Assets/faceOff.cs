using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class faceOff : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] rotationButtons;
    public Transform polyhedron;

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
