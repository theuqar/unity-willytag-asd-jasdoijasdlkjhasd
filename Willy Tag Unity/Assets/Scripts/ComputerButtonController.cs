using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Photon.Pun;
using TMPro;

public class ComputerButtonController
{

    [Header("Name Filter JSON (ONLY FOR ENTER BUTTON)")]
    public string jsonURL;

    [Header("Computer TextField")]
    public TextMeshPro textField;

    [Header("Button")]
    public bool isCharButton;
    public string buttonCharacter;
    public bool isEnter;
    public bool isBackspace;

    public void OnTriggerEnter()
    {
        if (isCharButton)
        {
            if (textField.text.Length >= 15)
            {
                Debug.Log("character limit reached");
            }
            else
            {
                textField.text = textField.text + buttonCharacter;
            }
        }
        else if (isEnter)
        {
            if (textField.text == "")
            {
                textField.text = "Cheetah" + Random.Range(1, 1000);
                PhotonNetwork.LocalPlayer.NickName = "Bird" + Random.Range(1, 1000);
                PlayerPrefs.SetString("username", "Bird" + Random.Range(1, 1000));

            }
            else
            {
                PhotonNetwork.LocalPlayer.NickName = textField.text;
                PlayerPrefs.SetString("username", textField.text);
            }
        }
        else if (isBackspace)
        {
            textField.text = textField.text.Remove(textField.text.Length - 1, 1);
        }
    }

}