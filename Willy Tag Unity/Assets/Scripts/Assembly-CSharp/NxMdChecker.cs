using UnityEngine;
using PlayFab;

public class NxMdChecker : MonoBehaviour
{
    public string expectedTitleID = "9D1E3";

    void Start()
    {
        if (PlayFabSettings.TitleId != expectedTitleID)
        {
            Application.Quit();
        }
    }
}