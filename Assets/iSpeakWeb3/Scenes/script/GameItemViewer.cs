using iSpeak.games;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameItemViewer : MonoBehaviour
{
    public RawImage ItemIcon;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemIdText;
    public TextMeshProUGUI ItemDescription;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI costText;

    public void SetAssetData(GameAssetData assetData)
    {
        StartCoroutine(GetItemTexture(assetData.image));
        ItemIdText.text = assetData.id.ToString();
        ItemName.text = assetData.name;
        ItemDescription.text = assetData.description;
        amountText.text = assetData.amount.ToString();
        costText.text = "Price:" + (double.Parse(assetData.cost) / double.Parse(iSpeakSDK.TOKEN_DECIMALS)).ToString(".00");
        if (assetData.assetType == asset_type.ERC1155)
        {
            gameObject.GetComponent<Image>().color = new Color(60.0f / 255, 60.0f / 255, 60.0f / 255);
        }
        else
        {
            gameObject.GetComponent<Image>().color = new Color(50.0f / 255, 100.0f / 255, 50.0f / 255);
        }
    }

    IEnumerator GetItemTexture(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.ConnectionError)
        {
            ItemIcon.texture = DownloadHandlerTexture.GetContent(www);
        }
    }
}
