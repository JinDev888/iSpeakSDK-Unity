using iSpeak.games;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;

public class iSpeakSDKTest : MonoBehaviour
{
    public TextMeshProUGUI walletAddress;
    public TextMeshProUGUI chainIDText;
    public TextMeshProUGUI projectNameText;
    public TextMeshProUGUI balanceText;
    public Transform getItemsListTrans;
    public GameObject gameItemPrefab;
    public Transform getOwnedItemsListTrans;
    public TMP_InputField buyItemIdsText;
    public TMP_InputField buyItemAmountsText;
    public TMP_InputField requestCoinText;
    public TMP_InputField updateItemIDText;
    public TMP_InputField updatePropNameText;
    public TMP_InputField updatePropvalueText;

    public TMP_InputField refundItemIDText;
    public TMP_InputField refundItemAmountText;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform childTrans in getItemsListTrans)
        {
            Destroy(childTrans.gameObject);
        }
        foreach (Transform childTrans in getOwnedItemsListTrans)
        {
            Destroy(childTrans.gameObject);
        }

        walletAddress.text = iSpeakSDK.LoginSession.account;
        chainIDText.text = iSpeakSDK.LoginSession.projectInfo.chainId.ToString();
        projectNameText.text = iSpeakSDK.LoginSession.projectInfo.project_name;
    }

    public async void onGetBalances()
    {
        InformationUI.Instance.ShowWaitingUI(true, "Waiting Response...");
        try
        {
            BigInteger balance = await iSpeakSDK.getBalances();
            balanceText.text = (double.Parse(balance.ToString()) / double.Parse(iSpeakSDK.TOKEN_DECIMALS)).ToString(".00");
        }
        catch (Exception ex)
        {
            InformationUI.Instance.ShowWaitingUI(false);
            InformationUI.Instance.ShowInfomationUI(ex.Message);
        }
        InformationUI.Instance.ShowWaitingUI(false);
    }
    public async void GetInGameItems()
    {
        foreach (Transform childTrans in getItemsListTrans)
        {
            Destroy(childTrans.gameObject);
        }
        InformationUI.Instance.ShowWaitingUI(true, "Waiting For Response...");
        try
        {
            GameAssetResponse response = await iSpeakSDK.getInGameItems();
            if (response != null && response.assets != null)
            {
                foreach (GameAssetData assetData in response.assets)
                {
                    GameObject gameAssetItem = GameObject.Instantiate(gameItemPrefab, getItemsListTrans);
                    gameAssetItem.GetComponent<GameItemViewer>().SetAssetData(assetData);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        InformationUI.Instance.ShowWaitingUI(false);
    }
    public async void GetOwnedItems()
    {
        InformationUI.Instance.ShowWaitingUI(true, "Waiting For Response...");
        foreach (Transform childTrans in getOwnedItemsListTrans)
        {
            Destroy(childTrans.gameObject);
        }
        try
        {
            GameAssetResponse response = await iSpeakSDK.getOwnedItems();
            if (response != null && response.assets != null)
            {
                foreach (GameAssetData assetData in response.assets)
                {
                    GameObject gameAssetItem = GameObject.Instantiate(gameItemPrefab, getOwnedItemsListTrans);
                    gameAssetItem.GetComponent<GameItemViewer>().SetAssetData(assetData);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }


        InformationUI.Instance.ShowWaitingUI(false);

    }

    public async void BuyItems()
    {
        string[] itemIds_string = buyItemIdsText.text.Split(",");
        string[] amounts_string = buyItemAmountsText.text.Split(",");
        if (itemIds_string.Length != amounts_string.Length)
        {
            return;
        }
        uint[] itemIds = new uint[itemIds_string.Length];
        uint[] amounts = new uint[itemIds_string.Length];
        for (int i=0; i< itemIds_string.Length; i++)
        {
            itemIds[i] = uint.Parse(itemIds_string[i].Trim());
            amounts[i] = uint.Parse(amounts_string[i].Trim());
        }
        InformationUI.Instance.ShowWaitingUI(true, "Waiting For Response...");
        try
        {
            string result = await iSpeakSDK.BuyItems(itemIds, amounts);
            Debug.Log("Buy Item Result = " + result);
        }
        catch (Exception ex)
        {
            InformationUI.Instance.ShowWaitingUI(false);
            InformationUI.Instance.ShowInfomationUI(ex.Message);
        }
        InformationUI.Instance.ShowWaitingUI(false);
    }

    public async void RequestTokenFromServer()
    {
        InformationUI.Instance.ShowWaitingUI(true, "Waiting For Response...");
        try
        {
            BigInteger amount = BigInteger.Parse(requestCoinText.text) * iSpeakSDK.TokenDecimal;
            string transaction = await iSpeakSDK.RequestTokens(amount);
            bool transactionResult = await iSpeakSDK.WaitForTransaction(transaction);
            if (transactionResult == false)
            {
                throw new Exception("Transaction failed");
            }
        }
        catch (Exception ex)
        {
            InformationUI.Instance.ShowWaitingUI(false);
            InformationUI.Instance.ShowInfomationUI(ex.Message);
        }

        InformationUI.Instance.ShowWaitingUI(false);
    }

    public async void UpdateMetaData()
    {
        InformationUI.Instance.ShowWaitingUI(true, "Waiting For Response...");
        try
        {
            uint itemID = uint.Parse(updateItemIDText.text);
            string result = await iSpeakSDK.UpdateMetadata(itemID, updatePropNameText.text, updatePropvalueText.text);
            InformationUI.Instance.ShowWaitingUI(false);
            InformationUI.Instance.ShowInfomationUI("Updating Metadata is succeed.");
        }
        catch (Exception ex)
        {
            InformationUI.Instance.ShowWaitingUI(false);
            InformationUI.Instance.ShowInfomationUI(ex.Message);
        }
    }

    public async void RefundItem()
    {
        InformationUI.Instance.ShowWaitingUI(true, "Waiting For Response...");
        try
        {
            uint itemID = uint.Parse(refundItemIDText.text);
            uint amount = uint.Parse(refundItemAmountText.text);
            string refundTransaction = await iSpeakSDK.RefundItem(itemID, amount);
            bool transactionResult = await iSpeakSDK.WaitForTransaction(refundTransaction);
            if (transactionResult == false)
            {
                throw new Exception("Refund Transaction is failed");
            }
            InformationUI.Instance.ShowWaitingUI(false);
            InformationUI.Instance.ShowInfomationUI("The item refunding is succeeded.");
        }
        catch (Exception ex)
        {
            InformationUI.Instance.ShowWaitingUI(false);
            InformationUI.Instance.ShowInfomationUI(ex.Message);
        }

        
    }


    private void OnApplicationQuit()
    {
        iSpeakSDK.Logout();
    }

}
