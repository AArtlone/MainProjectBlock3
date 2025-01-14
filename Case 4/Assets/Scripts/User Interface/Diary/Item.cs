﻿using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[Serializable]
public class Item : MonoBehaviour
{
    public bool Reactivatable;
    public enum ItemType { Torch, Sword, EmptyBucket, FullBucket, GroningenFlag, BommenBerendFlag, BunchOfHay, Cloth, Planks, Stretcher };
    public ItemType Type;
    public string Name;
    public string NameDutch;
    [Space(10)]
    public string Description;
    public string DescriptionDutch;
    [Space(10)]
    public string Active;
    public string ActiveDutch;
    [Space(10)]
    public string AssetsImageName;

    #region Hidden objects puzzle bools
    // Specifically for determining which items to hint at for the player.
    [NonSerialized]
    public bool IsItemHintedAt;
    [NonSerialized]
    public bool ItemFound;
    #endregion

    private CameraBehavior _cameraBehaviour;
    private int _timer;

    private Vector3 _previousPos;
    private Escape _gameInterface;
    private Canvas _canvas;

    public Item(
        string Name,
        string NameDutch,
        string Description,
        string DescriptionDutch,
        string Active,
        string ActiveDutch,
        string AssetsImageName)
    {
        this.Name = Name;
        this.NameDutch = NameDutch;

        this.Description = Description;
        this.DescriptionDutch = DescriptionDutch;

        this.Active = Active;
        this.ActiveDutch = ActiveDutch;

        this.AssetsImageName = AssetsImageName;
    }

    public void HoldingDown()
    {
        _previousPos = transform.position;
        InvokeRepeating("CountTimerUp", 0f, 1f);
    }

    public void Release()
    {
        CancelInvoke();
        _timer = 0;
    }

    private void CountTimerUp()
    {
        _timer++;

        if (_timer == 2 && _previousPos == transform.position)
        {
            DisplayItemDetails();
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Escape Game")
        {

            _gameInterface = FindObjectOfType<Escape>();
        }
        _cameraBehaviour = FindObjectOfType<CameraBehavior>();
        _canvas = GetComponent<Canvas>();
    }

    /// <summary>
    /// This functions shows the item's parameters in this class to the details
    /// window in the game whenever the player holds on an item in the inventory.
    /// </summary>
    public void DisplayItemDetails()
    {
        InterfaceManager.Instance.OpenPopup(InterfaceManager.Instance.ItemDetailsWindow);

        Sprite sprite = Resources.Load<Sprite>("Items/Inventory/" + AssetsImageName);

        switch (SettingsManager.Instance.Language)
        {
            case "English":
                InterfaceManager.Instance.ItemDetailsPortrait.sprite = sprite;
                InterfaceManager.Instance.ItemDetailsName.text = Name;
                InterfaceManager.Instance.ItemDetailsDescription.text = Description;
                InterfaceManager.Instance.ItemDetailsActives.text = Active;
                break;
            case "Dutch":
                InterfaceManager.Instance.ItemDetailsPortrait.sprite = sprite;
                InterfaceManager.Instance.ItemDetailsName.text = NameDutch;
                InterfaceManager.Instance.ItemDetailsDescription.text = DescriptionDutch;
                InterfaceManager.Instance.ItemDetailsActives.text = ActiveDutch;
                break;
        }
    }

    public void DragItem()
    {
        _cameraBehaviour.IsInterfaceElementSelected = true;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            transform.SetParent(GameObject.FindGameObjectWithTag("Items Panel").transform);
            transform.position = new Vector2(touch.position.x, touch.position.y);
            // This makes it so that the timer is not increasing if youre holding the
            // item AND dragging it at the same time.
            _timer = 0;
            _canvas.sortingOrder += 50;
        }
    }

    public void DropItem()
    {
        _cameraBehaviour.IsInterfaceElementSelected = false;

        _canvas.sortingOrder -= 50;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            Vector2 origin = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            RaycastHit2D[] hitObjs = Physics2D.RaycastAll(origin, Vector3.forward, Mathf.Infinity);
            RaycastHit2D hitObj = Physics2D.Raycast(origin, Vector3.forward, Mathf.Infinity);
            for (int i = 0; i < hitObjs.Length; i++)
            {
                if (hitObjs[i].transform != null)
                {
                    if (hitObjs[i].transform.GetComponent<Refugee>() != null && hitObjs[i].transform.GetComponent<Refugee>().Status == Refugee.RefugeeStatus.Injured)
                    {
                        if (Type == ItemType.Stretcher && hitObjs[i].transform.GetComponent<Refugee>().Status == Refugee.RefugeeStatus.Injured)
                        {
                            hitObjs[i].transform.gameObject.GetComponent<Refugee>().CureRefugee();
                        }
                    }
                }
                if (hitObjs[i].transform.tag == "Item Interactable Object")
                {
                    if (hitObjs[i].transform.name == "Well" && Type == ItemType.EmptyBucket)
                    {
                        _gameInterface.Inventory.RemoveItem(this);
                        _gameInterface.Inventory.AddItem(_gameInterface.FullBucketPrefab);
                    }
                }
                if (hitObjs[i].transform.tag == "Escape Water For Bucket")
                {
                    if (Type == ItemType.EmptyBucket)
                    {
                        _gameInterface.Inventory.RemoveItem(this);
                        _gameInterface.Inventory.AddItem(_gameInterface.FullBucketPrefab);
                    }
                }
            }
            if (hitObj.transform != null)
            {
                /*if (hitObj.transform.GetComponent<Refugee>() != null)
                {
                    if (Type == ItemType.Stretcher && hitObj.transform.GetComponent<Refugee>().Status == Refugee.RefugeeStatus.Injured)
                    {
                        hitObj.transform.gameObject.GetComponent<Refugee>().CureRefugee();
                    }
                }*/
                // Checking if the item was dropped on top of the object in the escape
                // game that can be interacted with the item
                if (hitObj.transform.tag == "Item Interactable Object")
                {
                    /*if (hitObj.transform.name == "Well" && Type == ItemType.EmptyBucket)
                    {
                        _gameInterface.Inventory.RemoveItem(this);
                        _gameInterface.Inventory.AddItem(_gameInterface.FullBucketPrefab);
                    }*/
                    /*if (hitObj.transform.tag == "Escape NPC" && Type == ItemType.Stretcher)
                    {
                        hitObj.transform.GetComponent<Refugee>().CureRefugee();
                    }*/
                    
                    if (hitObj.transform.gameObject.GetComponent<Obstacle>() != null)
                    {
                        Obstacle.ObstacleType obstacleType = hitObj.transform.gameObject.GetComponent<Obstacle>().Type;
                        // Check if the dropping item is an empty buccket and if the interactable
                        // object is a well. If yes, then it replaces the empty bucket with a
                        // full bucket

                        if (obstacleType == Obstacle.ObstacleType.Flag && Type == ItemType.GroningenFlag)
                        {
                            _gameInterface.Inventory.RemoveItem(this);
                            _gameInterface.Inventory.AddItem(_gameInterface.BommenBerendFlagPrefab);
                            hitObj.transform.gameObject.GetComponent<Obstacle>().PlayFlag();
                        }

                        if (obstacleType == Obstacle.ObstacleType.Fire && Type == ItemType.FullBucket)
                        {
                            _gameInterface.Inventory.RemoveItem(this);
                            _gameInterface.Inventory.AddItem(_gameInterface.EmptyBucketPrefab);
                            hitObj.transform.gameObject.GetComponent<Obstacle>().PLayFire();
                        }

                        if (obstacleType == Obstacle.ObstacleType.Sheeps)
                        {
                            hitObj.transform.gameObject.GetComponent<Obstacle>().PlaySheeps(Type);
                        }

                        if (obstacleType == Obstacle.ObstacleType.EvilPlant)
                        {
                            bool isPLantOnFire = false;
                            GameObject plantFire = null;
                            for (int i = 0; i < hitObj.transform.childCount; i++)
                            {
                                if (hitObj.transform.GetChild(i).transform.tag == "Escape Fire Under Plant")
                                {
                                    plantFire = hitObj.transform.GetChild(i).gameObject;
                                }
                            }
                            if (plantFire.activeSelf == true)
                            {
                                isPLantOnFire = true;
                            }

                            if (!isPLantOnFire)
                            {
                                if (Type == ItemType.Sword)
                                {
                                    hitObj.transform.gameObject.GetComponent<Obstacle>().CutPlant();
                                }
                                else if (Type == ItemType.Torch)
                                {
                                    hitObj.transform.gameObject.GetComponent<Obstacle>().SetPlantOnFire();
                                }
                            }
                            else
                            {
                                if (Type == ItemType.FullBucket)
                                {
                                    _gameInterface.Inventory.RemoveItem(this);
                                    _gameInterface.Inventory.AddItem(_gameInterface.EmptyBucketPrefab);
                                    hitObj.transform.gameObject.GetComponent<Obstacle>().ExtinguishPlant();
                                }
                            }
                        }
                    }
                }
            }

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.tag == "Item")
                {
                    _gameInterface.Inventory.CraftItem(this, hit.transform.GetComponent<Item>());
                }

                if (hit.transform.tag == "NPC")
                {
                    // TODO: Based on item throw, show a wrong item drop dialogue or the correct one for the main npc dialogue starting from the point after you drop the item.
                    if (DialogueManager.Instance.CurrentNPCDialogue == null)
                    {
                        bool isCorrectItemDropped = false;
                        NPC npc = hit.transform.gameObject.GetComponent<NPC>();

                        int index = 0;
                        for (int i = 0; i < npc.DialogueFormats.Count; i++)
                        {
                            if (npc.DialogueFormats[i].Language == SettingsManager.Instance.Language)
                            {
                                index = i;
                                break;
                            }
                        }

                        if (npc.DialogueFormats[index].Dialogue[0].DialogueBranches[0].ItemsDropped.Count == 0 &&
                            npc.DialogueFormats[index].Dialogue[0].DialogueBranches[0].ItemsRequired[0] ==
                            Name)
                        {
                            npc.DialogueFormats[index].Dialogue[0].DialogueBranches[0].ItemsDropped.Add(Name);
                            isCorrectItemDropped = true;
                        }
                        if (isCorrectItemDropped)
                        {
                            npc.ContinueDialogue();
                        }
                    }
                }

                // Here we check if the item was dragged and dropped on top of the
                // dialogue box and if so, then we populate the dropped items list in
                // that dialogue branch's array of dropped items and check whether or
                // not all the required items have been dropped in order to progress
                // further in the dialogue
                if (hit.transform.tag == "Dialogue Box")
                {
                    NPC currentNpc = DialogueManager.Instance.CurrentNPCDialogue;

                    int index = 0;
                    for (int i = 0; i < currentNpc.DialogueFormats.Count; i++)
                    {
                        if (currentNpc.DialogueFormats[i].Language == SettingsManager.Instance.Language)
                        {
                            index = i;
                            break;
                        }
                    }

                    foreach (DialogueBranch branch in currentNpc.DialogueFormats[index].Dialogue[currentNpc.CurrentDialogueIndex].DialogueBranches)
                    {
                        foreach (string requiredItem in branch.ItemsRequired)
                        {
                            // If the item's name matches one of the required ones, then we
                            // give feedback to the player the item has been dropped.
                            if (Name == requiredItem)
                            {
                                branch.ItemsDropped.Add(Name);

                                if (branch.ItemsDropped.Count == branch.ItemsRequired.Count)
                                {
                                    currentNpc.ContinueDialogue();
                                }
                            }
                        }
                    }
                }
            }

            if (SceneManager.GetActiveScene().name == "Escape Game")
            {
                FindObjectOfType<EscapeInventory>().RefreshPanel();
            }
            else
            {
                transform.SetParent(GameObject.Find("Items").transform);
                Character.Instance.ReloadInventory();
            }
        }
    }
}
