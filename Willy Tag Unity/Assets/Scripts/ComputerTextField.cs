using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComputerTextField : MonoBehaviour
{
    [Header("Type of TextField")]
    public bool isNameField;

    private void Start()
    {
        if (isNameField)
        {
            this.GetComponent<TextMeshPro>().text = PlayerPrefs.GetString("username");
        }
    }
}
