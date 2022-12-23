using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InformationUI : MonoBehaviour
{
    public static InformationUI Instance;

    public HelpData helpAssets;
    public GameObject WaitingPanelObject;
    public TextMeshProUGUI WaitingUIText;
    public GameObject InformPanelObject;
    public TextMeshProUGUI InformUIText;
    public GameObject HelpPanelObject;
    public TextMeshProUGUI HelpUIText;
    public TextMeshProUGUI HelpTitleText;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        WaitingPanelObject.SetActive(false);
        InformPanelObject.SetActive(false);
    }

    public void ShowWaitingUI(bool show, string text="")
    {
        WaitingUIText.text = text;
        WaitingPanelObject.SetActive(show);
    }

    public void ShowInfomationUI(string informText)
    {
        InformUIText.text = informText;
        InformPanelObject.SetActive(true);
    }
    public void InformPanelOK()
    {
        InformPanelObject.SetActive(false);
    }
    public void ShowHelpUI(string helpTitle)
    {
        
        for (int i=0; i< helpAssets.HelpDataAssets.Length; i++)
        {
            if (helpAssets.HelpDataAssets[i].title == helpTitle)
            {
                HelpUIText.text = helpAssets.HelpDataAssets[i].helpData;
                HelpTitleText.text = helpTitle;
                HelpPanelObject.SetActive(true);
                break;
            }
        }
    }
    public void HelpPanelClose()
    {
        HelpPanelObject.SetActive(false);
    }
}
