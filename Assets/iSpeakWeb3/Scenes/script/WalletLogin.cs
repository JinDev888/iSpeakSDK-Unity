using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace iSpeak.games
{
    public class WalletLogin : MonoBehaviour
    {

        public TMP_InputField ProjectID_Input;
        public TMP_InputField ProjectKey_Input;
        public async void LoginProject()
        {
            InformationUI.Instance.ShowWaitingUI(true, "Logining to Project");
            try
            {
                LoginSessionResponse loginSession = await iSpeakSDK.LoginUsingWallet(ProjectID_Input.text, ProjectKey_Input.text);
                InformationUI.Instance.ShowWaitingUI(false);
                SceneManager.LoadScene("GameScene");
            }
            catch (Exception ex)
            {

                InformationUI.Instance.ShowWaitingUI(false);
                InformationUI.Instance.ShowInfomationUI(ex.Message);
            }
        }



    }
}

