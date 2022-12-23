using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace iSpeak.games
{
    public partial class iSpeakSDK
    {
        private static async Task<string> InitSDK(string projectID, string projectKey)
        {
            WWWForm form = new WWWForm();
            form.AddField("p-id", projectID);
            form.AddField("p-key", projectKey);
            string url = host + "/request_login";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.ConnectionError)
                {
                    try
                    {
                        string respose = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                        try
                        {
                            LoginTokenTokenResponse token = JsonConvert.DeserializeObject<LoginTokenTokenResponse>(respose);
#if UNITY_WEBGL && !UNITY_EDITOR
                            Web3GL.SetNetwork(token.chainId);
#endif
                            return token.token;
                        }
                        catch
                        {
                            throw new Exception(respose);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    throw new Exception("Connection Error");
                }
            }
        }

        private static async Task<LoginSessionResponse> VerifySignature(string signature, string loginToken)
        {
            WWWForm form = new WWWForm();
            form.AddField("signature", signature);
            form.AddField("loginToken", loginToken);
            string url = host + "/LoginSignature";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.ConnectionError)
                {
                    string respose = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                    try
                    {
                        try
                        {
                            LoginSessionResponse session = JsonConvert.DeserializeObject<LoginSessionResponse>(respose);
                            return session;
                        }
                        catch
                        {
                            throw new Exception(respose);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    throw new Exception("Connection Error");
                }
            }
        }

        private static BigInteger CalcualtAssetPrice(uint[] tokenIDs, uint[] amounts)
        {
            if (InGameAssets == null)
            {
                return new BigInteger(0);
            }
            BigInteger allPrice = new BigInteger(0);
            for (int tokenIndex = 0; tokenIndex < tokenIDs.Length; tokenIndex++)
            {
                for (int i = 0; i < InGameAssets.assets.Length; i++)
                {
                    if (InGameAssets.assets[i].id == tokenIDs[tokenIndex])
                    {
                        allPrice += BigInteger.Parse(InGameAssets.assets[i].cost);
                    }
                }
            }
            return allPrice;
        }

        private static async Task<string> PayForPurchaseItem(uint[] tokenIDs, uint[] amounts)
        {
            if (LoginSession == null)
            {
                throw new Exception("Invalid login session");
            }
            if (InGameAssets == null)
            {
                await getInGameItems();
            }
            if (InGameAssets == null)
            {
                return null;
            }
            BigInteger allPrice = CalcualtAssetPrice(tokenIDs, amounts);
            Debug.Log("PurchaseItem:allPrice = " + allPrice.ToString());
            try
            {
                BigInteger balance = await iSpeakSDK.getBalances();
                if (balance < allPrice)
                {
                    throw new Exception("lack of money");
                }
                BigInteger allowance = await GetAllowanceTokenCount();
                if (allowance < allPrice)
                {
                    string transactionHash = await ApproveToken(balance);
                    bool approveResult = await WaitForTransaction(transactionHash);
                    if (approveResult == false)
                    {
                        throw new Exception("Approve Transaction Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            try
            {
                object[] arg_obj = { tokenIDs, amounts };
                string args = JsonConvert.SerializeObject(arg_obj);

#if UNITY_WEBGL && !UNITY_EDITOR
            try {
                    Debug.Log("args=" + args);
                    string gasLimit = "";
                    string gasPrice = "";
                    try
                    {
                        string response = await Web3GL.SendContract(method_purchase, contract_abi, LoginSession.projectInfo.contract_address, args, "0", gasLimit, gasPrice);
                        Debug.Log("BuyNewToken:" + response);
                        return response;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
            } 
            catch (Exception ex) {
                throw ex;
            }
#else

                Debug.Log("args=" + args);
                string contractData = await CreateContractData(contract_abi, method_purchase, args);
                string response = await Web3Wallet.SendTransaction(LoginSession.projectInfo.chainId.ToString(), LoginSession.projectInfo.contract_address, "0", contractData, "", "");
                return response;
#endif
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async Task<string> RequestDelivery(string transactionHash)
        {
            if (LoginSession == null)
            {
                throw new Exception("Invalid login session");
            }
            WWWForm form = new WWWForm();
            form.AddField("hash", transactionHash);
            string url = host + "/deliveryItem";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                webRequest.SetRequestHeader("session", LoginSession.session);
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.ConnectionError)
                {
                    string response = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                    try
                    {
                        try
                        {
                            TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(response);
                            return token.token;
                        }
                        catch
                        {
                            throw new Exception(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                        throw ex;
                    }
                }
                else
                {
                    throw new Exception("Connection Error");
                }
            }
        }

        private static async Task<string> TransferToken(uint itemID, uint amount)
        {
            if (LoginSession == null)
            {
                throw new Exception("Invalid login session");
            }
            if (OwnedGameAssets == null)
            {
                await getOwnedItems();
            }
            if (OwnedGameAssets == null)
            {
                throw new Exception("Invalid item ID");
            }
            bool hasOwned = false;
            for (int i = 0; i < OwnedGameAssets.assets.Length; i++)
            {
                if (OwnedGameAssets.assets[i].id == itemID && OwnedGameAssets.assets[i].amount >= amount)
                {
                    hasOwned = true;
                }
            }
            if (hasOwned == false)
            {
                throw new Exception("Invalid item ID");
            }
            try
            {

                object[] arg_obj = { LoginSession.account, LoginSession.projectInfo.owner_address, itemID, amount, "" };
                string args = JsonConvert.SerializeObject(arg_obj);

#if UNITY_WEBGL && !UNITY_EDITOR
            try {
                    Debug.Log("args=" + args);
                    string gasLimit = "";
                    string gasPrice = "";
                    try
                    {
                        string response = await Web3GL.SendContract(method_transfer, contract_abi, LoginSession.projectInfo.contract_address, args, "0", gasLimit, gasPrice);
                        Debug.Log("Transfer:" + response);
                        return response;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
            } 
            catch (Exception ex) {
                throw ex;
            }
#else

                Debug.Log("args=" + args);
                string contractData = await CreateContractData(contract_abi, method_transfer, args);
                string response = await Web3Wallet.SendTransaction(LoginSession.projectInfo.chainId.ToString(), LoginSession.projectInfo.contract_address, "0", contractData, "", "");
                return response;
#endif
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async Task<string> RequestRefund(string transactionHash)
        {
            if (LoginSession == null)
            {
                throw new Exception("Invalid login session");
            }
            WWWForm form = new WWWForm();
            form.AddField("hash", transactionHash);
            string url = host + "/refoundItem";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                webRequest.SetRequestHeader("session", LoginSession.session);
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.ConnectionError)
                {
                    string response = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                    try
                    {
                        try
                        {
                            TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(response);
                            return token.token;
                        }
                        catch
                        {
                            throw new Exception(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                        throw ex;
                    }
                }
                else
                {
                    throw new Exception("Connection Error");
                }
            }

        }




    }
}

