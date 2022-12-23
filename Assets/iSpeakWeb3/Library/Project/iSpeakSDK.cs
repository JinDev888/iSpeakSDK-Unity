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
    public enum asset_type
    {
        ERC1155 = 0,
        ERC721 = 1,
    }
    public class LoginTokenTokenResponse
    {
        [SerializeField]
        public string token;
        [SerializeField]
        public int chainId;
    }
    public class TokenResponse
    {
        [SerializeField]
        public string token;
    }
    public class ProjectInfo
    {
        public string project_name;
        public string description;
        public string contract_address;
        public string owner_address;
        public string image;
        public int chainId;
    }
    public class LoginSessionResponse
    {
        [SerializeField]
        public string session;
        [SerializeField]
        public string account;
        [SerializeField]
        public ProjectInfo projectInfo;
    }
    public class GameAssetData
    {
        [SerializeField]
        public uint id;
        [SerializeField]
        public string name;
        [SerializeField]
        public string description;
        [SerializeField]
        public string image;
        [SerializeField]
        public string json;
        [SerializeField]
        public int amount;
        [SerializeField]
        public string cost;
        [SerializeField]
        public asset_type assetType;
        [SerializeField]
        public bool buyable;
    }
    public class GameAssetResponse
    {
        [SerializeField]
        public GameAssetData[] assets;
    }

    public class TransactionStatusResponse
    {
        [SerializeField]
        public int status;
        [SerializeField]
        public int confirmations;
    }

    public partial class iSpeakSDK
    {
        public static async Task<LoginSessionResponse> LoginUsingWallet(string projectID, string projectKey)
        {
            try
            {
                string loginToken = await InitSDK(projectID, projectKey);
                Debug.Log("loginToken = " + loginToken);
                string signature = "";
#if UNITY_WEBGL && !UNITY_EDITOR
                Web3GL.SetConnectAccount("");
                Web3GL.Web3Connect();
                string account = Web3GL.ConnectAccount();
                while (account == "") {
                    await new WaitForSeconds(1f);
                    account = Web3GL.ConnectAccount();
                };
                Web3GL.SetSignMessageResponse("");
                Web3GL.SignMessage(loginToken);
                signature = Web3GL.SignMessageResponse();
                while (signature == "")
                {
                    await new WaitForSeconds(1f);
                    signature = Web3GL.SignMessageResponse();
                };
                if (signature.StartsWith("0x") == false || signature.Length != 132)
                {
                    throw new Exception("sign error");
                }
#else
                signature = await Web3Wallet.Sign(loginToken);
                Debug.Log("signature = " + signature);
#endif
                LoginSession = await VerifySignature(signature, loginToken);
                return LoginSession;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<BigInteger> getBalances()
        {
            if (LoginSession == null)
            {
                throw new Exception("Invalid Login Session");
            }
            WWWForm form = new WWWForm();
            string url = host + "/getBalances";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                webRequest.SetRequestHeader("session", LoginSession.session);
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.ConnectionError)
                {
                    string respose = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                    try
                    {
                        TokenResponse res = JsonConvert.DeserializeObject<TokenResponse>(respose);
                        return BigInteger.Parse(res.token);
                    }
                    catch
                    {
                        throw new Exception(respose);
                    }
                }
                else
                {
                    throw new Exception("Connection Error");
                }

            }
        }

        public static async Task<GameAssetResponse> getInGameItems()
        {
            if (LoginSession == null)
            {
                return null;
            }
            WWWForm form = new WWWForm();
            string url = host + "/getItems";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                webRequest.SetRequestHeader("session", LoginSession.session);
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.ConnectionError)
                {
                    string respose = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                    try
                    {
                        Debug.Log("Assets Response:" + respose);
                        InGameAssets = JsonConvert.DeserializeObject<GameAssetResponse>(respose);
                        return InGameAssets;
                    }
                    catch
                    {
                        throw new Exception(respose);
                    }
                }
                else
                {
                    throw new Exception("Connection Error");
                }
            }
        }
        public static async Task<GameAssetResponse> getOwnedItems()
        {
            if (LoginSession == null)
            {
                throw new Exception("Invalid login session");
            }
            WWWForm form = new WWWForm();
            string url = host + "/getOwnedItems";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                webRequest.SetRequestHeader("session", LoginSession.session);
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.ConnectionError)
                {
                    string respose = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                    try
                    {
                        Debug.Log("Assets Response:" + respose);
                        OwnedGameAssets = JsonConvert.DeserializeObject<GameAssetResponse>(respose);
                        return OwnedGameAssets;
                    }
                    catch
                    {
                        throw new Exception(respose);
                    }
                }
                else
                {
                    throw new Exception("Connection Error");
                }
            }
        }

        public static async Task<string> BuyItems(uint[] itemIDs, uint[] amounts)
        {
            string transaction = await PayForPurchaseItem(itemIDs, amounts);
            Debug.Log(transaction);
            bool transactionResult = await WaitForTransaction(transaction);
            Debug.Log("Transaction Result = " + transactionResult);
            string deliveryResult = await RequestDelivery(transaction);
            return deliveryResult;
        }
        

        public static async Task<string> RequestTokens(BigInteger tokenAmount)
        {
            if (LoginSession == null)
            {
                throw new Exception("Invalid login session");
            }
            WWWForm form = new WWWForm();
            form.AddField("amount", tokenAmount.ToString());
            string url = host + "/requestTokens";
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

        public static async Task<string>UpdateMetadata(uint itemID, string propertyName, string newValue)
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
            for (int i=0; i< OwnedGameAssets.assets.Length; i++)
            {
                if (OwnedGameAssets.assets[i].id == itemID && OwnedGameAssets.assets[i].assetType == asset_type.ERC721)
                {
                    hasOwned = true;
                }
            }

            if (hasOwned == false)
            {
                throw new Exception("Invalid item ID");
            }

            WWWForm form = new WWWForm();
            form.AddField("assetID", itemID.ToString());
            form.AddField("prop_name", propertyName);
            form.AddField("prop_value", newValue);
            string url = host + "/updateMetadata";
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

        public static async Task<string> RefundItem(uint itemID, uint amount)
        {
            string transaction = await TransferToken(itemID, amount);
            Debug.Log("TransferToken:" + transaction);
            bool transactionResult = await WaitForTransaction(transaction);
            if (transactionResult == true)
            {
                string refundTransaction = await RequestRefund(transaction);
                Debug.Log("refundTransaction:" + refundTransaction);
                return refundTransaction;
            }
            else
            {
                throw new Exception("Transaction is failed");
            }
        }






        public static async Task<string> Logout()
        {
            if (LoginSession == null)
            {
                return null;
            }
            WWWForm form = new WWWForm();
            string url = host + "/logout";
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                webRequest.SetRequestHeader("session", LoginSession.session);
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.ConnectionError)
                {
                    try
                    {
                        TokenResponse response = JsonUtility.FromJson<TokenResponse>(System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data));
                        Debug.Log(response.token);
                        LoginSession = null;
                        return response.token;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                        return null;
                    }
                }

            }
            return null;
        }
    }
}

