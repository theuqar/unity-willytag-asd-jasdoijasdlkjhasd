using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCosmetics : MonoBehaviour
{

    public List<GameObject> Cosmetics;

    [PunRPC]
    void EnableCosmetic(string cosmeticName)
    {
        for (int i = 0; i < Cosmetics.Count; i++)
        {
            if (Cosmetics[i].name == cosmeticName)
            {
                Cosmetics[i].SetActive(true);
            }
        }
    }

    [PunRPC]
    void DisableCosmetic(string cosmeticName)
    {
        for (int i = 0; i < Cosmetics.Count; i++)
        {
            if (Cosmetics[i].name == cosmeticName)
            {
                Cosmetics[i].SetActive(false);
            }
        }
    }
}
