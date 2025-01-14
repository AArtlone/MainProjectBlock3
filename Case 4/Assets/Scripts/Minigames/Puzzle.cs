﻿using LitJson;
using System.IO;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    public string Name;
    private int _stars;
    public GameObject StarsDisplay;

    public enum PuzzleType { HiddenObjects, SpellingPuzzle, GuessClothes, EscapeGame };
    public PuzzleType TypeOfPuzzle;

    private void Start()
    {
        LoadStars();
    }

    public void LoadStars()
    {
        // Depending on what type of puzzle this puzzle script is assigned to in the editor,
        // a different puzzle data's file will be retrieved and its contents loaded into
        // this script.
        JsonData puzzleData = new JsonData();
        switch (TypeOfPuzzle)
        {
            case PuzzleType.HiddenObjects:
                puzzleData = JsonMapper.ToObject(File.ReadAllText(Application.persistentDataPath + "/HiddenObjectPuzzles.json"));
                break;
            case PuzzleType.GuessClothes:
                puzzleData = JsonMapper.ToObject(File.ReadAllText(Application.persistentDataPath + "/GuessingPuzzles.json"));
                break;
            case PuzzleType.SpellingPuzzle:
                puzzleData = JsonMapper.ToObject(File.ReadAllText(Application.persistentDataPath + "/SpellingPuzzles.json"));
                break;
            case PuzzleType.EscapeGame:
                puzzleData = JsonMapper.ToObject(File.ReadAllText(Application.persistentDataPath + "/EscapeGames.json"));
                break;
        }

        if (TypeOfPuzzle != PuzzleType.EscapeGame)
        {
            // Here we retrieve the stars from the puzzle that matches this one's name in storage.
            for (int i = 0; i < puzzleData["Puzzles"].Count; i++)
            {
                if (puzzleData["Puzzles"][i]["Name"].ToString() == Name)
                {
                    _stars = int.Parse(puzzleData["Puzzles"][i]["Stars"].ToString());
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < puzzleData["EscapeGames"].Count; i++)
            {
                if (puzzleData["EscapeGames"][i]["Name"].ToString() == Name)
                {
                    _stars = 3;
                    break;
                }
            }
        }

        // Once the stars from the data file have been loaded into this script, we
        // enable the stars visible in the area for as many as this script now has.
        if (StarsDisplay != null)
        {
            for (int i = 0; i < StarsDisplay.transform.childCount; i++)
            {
                if (i < _stars)
                {
                    StarsDisplay.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    StarsDisplay.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }
}
