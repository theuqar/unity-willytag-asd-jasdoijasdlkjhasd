using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Collections;

public class CheckDownMessage : MonoBehaviour
{
    public AudioSource audioSource;

    private void Start()
    {
        // Start the coroutine
        StartCoroutine(CheckDownMessageCoroutine());
    }

    private IEnumerator CheckDownMessageCoroutine()
    {
        while (true)
        {
            // Get the title data for "downmessage"
            GetTitleDataRequest request = new GetTitleDataRequest { Keys = new List<string> { "downmessage" } };
            PlayFabClientAPI.GetTitleData(request, OnGetTitleDataSuccess, OnGetTitleDataError);

            // Wait for 1 minute before calling the function again
            yield return new WaitForSeconds(60f);
        }
    }

    private void OnGetTitleDataSuccess(GetTitleDataResult result)
    {
        // Check if the "downmessage" data is "true"
        if (result.Data.ContainsKey("downmessage") && result.Data["downmessage"] == "true")
        {
            // Play the audio source
            audioSource.Play();
        }
    }

    private void OnGetTitleDataError(PlayFabError error)
    {
        // Handle any errors
        Debug.LogError("Failed to get title data: " + error.ErrorMessage);
    }
}
