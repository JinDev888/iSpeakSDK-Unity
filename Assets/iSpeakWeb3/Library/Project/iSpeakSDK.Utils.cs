using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace iSpeak.games
{
    public partial class iSpeakSDK
    {
        public async static Task<string> CreateContractData(string abi, string method, string args)
        {
            WWWForm form = new WWWForm();
            form.AddField("abi", abi);
            form.AddField("method", method);
            form.AddField("args", args);
            string url = host + "/utils/createContractData";
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
        public async static Task<BigInteger> GetAllowanceTokenCount()
        {
            WWWForm form = new WWWForm();
            string url = host + "/utils/getAllowanceTokenCount";
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
                            Debug.Log("GetAllowanceTokenCount Response:" + token.token);
                            return BigInteger.Parse(token.token);
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

        public async static Task<string> ApproveToken(BigInteger amount)
        {
            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                string[] obj = { LoginSession.projectInfo.contract_address, amount.ToString()};
                string args = JsonConvert.SerializeObject(obj);
                string transaction = "";
                try
                {
                    transaction = await Web3GL.SendContract(method_token_approve, erc20_contract_abi , token_contract_address, args, "0", "", "");
                }
                catch (Exception ex)
                {
                    throw new Exception("Transaction rejected");
                }
                Debug.Log("Approve Transaction=" + transaction);
                return transaction;
#else   
                string[] obj = { LoginSession.projectInfo.contract_address, amount.ToString() };
                string args = JsonConvert.SerializeObject(obj);
                string contractData = await CreateContractData(erc20_contract_abi, method_token_approve, args);
                string response = await Web3Wallet.SendTransaction(LoginSession.projectInfo.chainId.ToString(), token_contract_address, "0", contractData, "", "");
                return response;
#endif
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async static Task<TransactionStatusResponse> CheckTransaction(string transactionHash)
        {
            WWWForm form = new WWWForm();
            form.AddField("hash", transactionHash);
            string url = host + "/utils/checkTransaction";
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
                            TransactionStatusResponse result = JsonConvert.DeserializeObject<TransactionStatusResponse>(response);
                            Debug.Log("CheckTransaction Response:" + JsonConvert.SerializeObject(result));
                            return result;
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

        public static async Task<bool> WaitForTransaction(string transactionHash)
        {
            TransactionStatusResponse transactionStatus = await CheckTransaction(transactionHash);
            Debug.Log("Status = " + transactionStatus.status);
            while (transactionStatus.status == -1)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                await new WaitForSeconds(0.5f);
#else
                await Task.Delay(500);
#endif
                transactionStatus = await CheckTransaction(transactionHash);
                Debug.Log("Status = " + transactionStatus.status);
            }
            if (transactionStatus.status == 0)
                return false;
            else
                return true;
        }
    }
}

