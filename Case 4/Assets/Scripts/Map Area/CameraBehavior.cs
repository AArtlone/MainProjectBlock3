﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraBehavior : MonoBehaviour
{
    #region Camera screen movement variables
    public GameObject Background;
    public GameObject BackgroundLeft;
    public GameObject BackgroundRight;
    public MapPart MapPart;
    [NonSerialized]
    public Bounds BackgroundBounds;
    public Bounds BackgroundBoundsLeft;
    public Bounds BackgroundBoundsRight;
    #endregion

    #region Camera screen interaction properties
    [Range(0, 100)]
    [SerializeField]
    private bool _isCameraTravelling = false;
    public Vector3 TapPosition;
    public bool IsInteracting = false;
    public bool IsEntityTappedOn = false;
    public bool IsInterfaceElementSelected = false;
    public bool IsUIOpen = false;
    public bool IsIntructionManualOn = true;
    #endregion

    // Camera components
    private Escape _gameInterface;
    private HiddenObjectsPuzzle _hiddenObjectPuzzleScript;

    private void Start()
    {
        _hiddenObjectPuzzleScript = FindObjectOfType<HiddenObjectsPuzzle>();
        if (SceneManager.GetActiveScene().name == "Escape Game")
        {
            if (BackgroundLeft != null && BackgroundRight != null)
            {
                BackgroundBoundsLeft = BackgroundLeft.GetComponent<SpriteRenderer>().bounds;
                BackgroundBoundsRight = BackgroundRight.GetComponent<SpriteRenderer>().bounds;
            }
        }
        else
        {
            if (Background != null)
            {
                BackgroundBounds = Background.GetComponent<SpriteRenderer>().bounds;
            }
        }
        _gameInterface = FindObjectOfType<Escape>();
        InterfaceManager.Instance.CameraBehavior = this;
        Character.Instance.CameraBehavior = this;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Hidden Objects Puzzle")
        {
            if (Input.touchCount > 0 &&
                Input.GetTouch(0).phase == TouchPhase.Began)
            {
                RaycastHit2D hitObj = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector3.forward, 1000);

                if (hitObj.transform != null)
                {
                    switch (hitObj.transform.tag)
                    {
                        case "HOP Item":
                            _hiddenObjectPuzzleScript.PickItem(hitObj.transform.gameObject);
                            break;
                    }
                }
            }
        }

        //IsEntityTappedOn = false;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            //if (AudioManager.Instance != null)
            //{
            //    AudioManager.Instance.PlaySound(AudioManager.Instance.HitScreen);
            //}

            //if (InterfaceManager.Instance != null)
            //{
            //    if (InterfaceManager.Instance.ItemActionsWindow != null)
            //    {
            //        if (InterfaceManager.Instance.ItemActionsWindow.activeSelf == true)
            //        {
            //            InterfaceManager.Instance.ClosePopup(InterfaceManager.Instance.ItemActionsWindow);
            //        }
            //    }
            //}

            RaycastHit2D hitCanvas = Physics2D.Raycast(Camera.main.ScreenToViewportPoint(Input.GetTouch(0).position), Vector2.down, 1000);

            RaycastHit2D hit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector2.down, 1000);

            if (hitCanvas.transform != null)
            {
                // Layer 11 is IgnoreCamera meant only for this script!
                if (hitCanvas.transform.gameObject.layer == 11)
                {
                    IsInterfaceElementSelected = true;
                }
            }

            if (hit2D.transform != null)
            {
                // Layer 11 is IgnoreCamera meant only for this script!
                // We also check if we clicked over an entity in the map area, because
                // we want it to be interactable unlike the canvas elements. This
                // includes the second if statement after!
                if (hit2D.transform.gameObject.layer == 11 &&
                    (hit2D.transform.tag != "Item Drop" &&
                    hit2D.transform.tag != "NPC" &&
                    hit2D.transform.tag != "Hidden Objects Puzzle" &&
                    hit2D.transform.tag != "Guess Clothes Puzzle" &&
                    hit2D.transform.tag != "Escape Game" &&
                    hit2D.transform.tag != "Sign"))
                {
                    IsInterfaceElementSelected = true;
                }
            }
        }

        //Debug.Log(IsInteracting + " " + IsInterfaceElementSelected + " " + IsEntityTappedOn + " " + IsUIOpen);
        if (IsInteracting == false &&
            IsInterfaceElementSelected == false &&
            IsEntityTappedOn == false &&
            IsUIOpen == false)
        {
            if ((Input.touchCount > 0 &&
                Input.GetTouch(0).phase == TouchPhase.Ended) ||
                _isCameraTravelling == false)
            {
                if (Input.touchCount > 0 &&
                    IsUIOpen == false)
                {
                    TapPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                }

                _isCameraTravelling = true;
            }
            else
            {
                if (Vector3.Distance(transform.position, TapPosition) < 1f)
                {
                    _isCameraTravelling = false;
                }
            }
        }
        if (SceneManager.GetActiveScene().name != "Escape Game" && SceneManager.GetActiveScene().name != "Hidden Objects Puzzle" &&
    SceneManager.GetActiveScene().name != "Spelling Puzzle")
        {
            if ((Input.touchCount > 0 &&
                Input.GetTouch(0).phase == TouchPhase.Began) &&
                IsInteracting == false &&
                IsInterfaceElementSelected == false &&
                IsUIOpen == false &&
                IsEntityTappedOn == false &&
                DialogueManager.Instance.DialogueTemplate.gameObject.activeSelf == false)
            {
                RaycastHit2D hitObj = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector3.forward, 1000);

                if (hitObj.transform != null)
                {
                    switch (hitObj.transform.tag)
                    {
                        case "NPC":
                            if (hitObj.transform.GetComponent<NPC>().CanInteractWithPlayer)
                            {
                                InterfaceManager.Instance.LoadCharacterDialogueAppearance();
                                // Replace with interaction mechanic in the future
                                hitObj.transform.GetComponent<NPC>().ContinueDialogue();
                                IsEntityTappedOn = true;
                                IsInteracting = true;
                                IsUIOpen = true;
                            }
                            break;
                        case "Hidden Objects Puzzle":
                            InterfaceManager.Instance.LoadScene("Hidden Objects Puzzle");
                            break;
                        case "Guess Clothes Puzzle":
                            InterfaceManager.Instance.LoadScene("Guess Clothing Puzzle");
                            IsEntityTappedOn = true;
                            IsInteracting = true;
                            IsUIOpen = true;
                            break;
                        case "Escape Game":
                            Character.Instance.LastScene = SceneManager.GetActiveScene().name;
                            Character.Instance.RefreshJsonData();
                            InterfaceManager.Instance.LoadScene("Escape Game");
                            break;
                        case "Item Drop":
                            if (SceneManager.GetActiveScene().name == "Escape Game")
                            {
                                Item hitItemDrop = hitObj.transform.GetComponent<Item>();
                                _gameInterface.Inventory.AddItem(hitItemDrop);
                            }
                            else
                            {
                                Item hitItemDrop = hitObj.transform.GetComponent<Item>();
                                InterfaceManager.Instance.ItemSelected = hitItemDrop;
                                InterfaceManager.Instance.DisplayActionsMenu();
                            }
                            break;
                        case "Area Interactable":
                            hitObj.transform.GetComponent<ToggleObject>().Replace();
                            break;
                    }
                }
            }
        }
        else
        {
            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) && IsInteracting == false && IsUIOpen == false && IsEntityTappedOn == false)
            {
                RaycastHit2D hitObj = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector3.forward, 1000);
                Vector2 origin = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                RaycastHit2D[] hitObjs = Physics2D.RaycastAll(origin, Vector3.forward, Mathf.Infinity);
                for (int i = 0; i < hitObjs.Length; i++)
                {
                    if (hitObjs[i].transform.tag == "ObsIntElement")
                    {
                        hitObjs[i].transform.GetComponent<ObstacleIntercationElement>().AssignRefugee();
                    } else if (hitObjs[i].transform.tag == "Escape Drum")
                    {
                        AudioManager.Instance.PlaySound(AudioManager.Instance.Drum);
                    }
                }

                if (hitObj.transform != null)
                {
                    switch (hitObj.transform.tag)
                    {
                        case "Item Drop":
                            Item hitItemDrop = hitObj.transform.GetComponent<Item>();
                            if (_gameInterface.Inventory.AddItem(hitItemDrop) == true)
                            {
                                if (hitItemDrop.Type != Item.ItemType.BunchOfHay)
                                {
                                    hitItemDrop.gameObject.SetActive(false);
                                } else if (hitItemDrop.Type == Item.ItemType.BunchOfHay)
                                {
                                    FindObjectOfType<Escape>().HayParticle.SetActive(false);
                                }
                                if (hitItemDrop.Reactivatable)
                                {
                                    _gameInterface.ReactivateItem(hitItemDrop.gameObject);
                                }
                            }
                            break;
                    }
                }
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                RaycastHit2D hitObj = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector3.forward, 1000);

                if (hitObj.transform != null)
                {
                    switch (hitObj.transform.tag)
                    {
                        case "Cannon Ball":
                            FindObjectOfType<CannonBallShooting>().HitCannonBallAway();
                            break;
                    }
                }

            }
        }
    }

}
