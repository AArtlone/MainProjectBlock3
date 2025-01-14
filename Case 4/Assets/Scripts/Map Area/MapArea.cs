﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapArea : MonoBehaviour
{
    public string Name;
    public Area.AreaStatus AreaStatus;
    public GameObject AreaEnvironment;
    public GameObject CurrentLocationPointer;
    public Sprite EnabledMapPoint;
    public Sprite DisabledMapPoint;

    private void Start()
    {
        foreach (Area area in Character.Instance.Areas)
        {
            if (area.Name == Name)
            {
                Name = area.Name;
                AreaStatus = area.Status;
                if(AreaStatus == Area.AreaStatus.Locked)
                {
                    GetComponent<Image>().sprite = DisabledMapPoint;
                } else
                {
                    GetComponent<Image>().sprite = EnabledMapPoint;
                    GetComponentInParent<Animator>().enabled = true;
                }
            }
        }

        if (Character.Instance.LastScene == Name)
        {
            CurrentLocationPointer.SetActive(true);
        } else
        {
            CurrentLocationPointer.SetActive(false);
        }
    }

    /// <summary>
    /// This function runs whenever the player selects a map point on the map to
    /// enter an area and validated whether or not he qualifies for it to enter.
    /// </summary>
    /// <param name="SceneToLoad"></param>
    public void EnterArea(string SceneToLoad)
    {
        if (AreaStatus == Area.AreaStatus.Unlocked)
        {
            Character.Instance.LastScene = SceneToLoad;
            Character.Instance.RefreshJsonData();
            InterfaceManager.Instance.LoadScene(SceneToLoad);
            //AreaEnvironment.SetActive(true);
            //Icons.SetActive(false);
        }
        else
        {
            InterfaceManager.Instance.OpenPopup(InterfaceManager.Instance.AreaLockedErrorPopup);
        }
    }
}