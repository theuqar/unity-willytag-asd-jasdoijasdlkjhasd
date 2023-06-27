using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class UpdateMOTD : MonoBehaviour
{
    public Text motdText;

    void Start()
    {
        // Register a PlayFab login callback function
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        }, OnPlayFabLoginSuccess, OnPlayFabLoginFailure);
    }

    // Callback function for a successful PlayFab login
    void OnPlayFabLoginSuccess(LoginResult result)
    {
        // Wait for 3 seconds before calling the GetTitleData API
        Invoke("GetMOTDTitleData", 3f);
    }

    // Callback function for a failed PlayFab login
    void OnPlayFabLoginFailure(PlayFabError error)
    {
        Debug.LogError("PlayFab login failed: " + error.ErrorMessage);
    }

    void GetMOTDTitleData()
    {
        // Call GetTitleData API to retrieve the MOTD title data
        List<string> keys = new List<string>();
        keys.Add("motd");

        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest()
        {
            Keys = keys
        }, OnGetTitleDataSuccess, OnGetTitleDataFailure);
    }

    void OnGetTitleDataSuccess(GetTitleDataResult result)
    {
        if (result.Data.TryGetValue("motd", out string motdMessage))
        {
            motdText.text = motdMessage;
        }
        else
        {
            Debug.LogError("MOTD title data not found.");
        }
    }

    void OnGetTitleDataFailure(PlayFabError error)
    {
        Debug.LogError("GetTitleData failed: " + error.ErrorMessage);
    }
}
