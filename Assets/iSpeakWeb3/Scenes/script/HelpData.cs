using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HelpString
{
    public string title;
    [TextArea(3, 10)]
    public string helpData;
}

[CreateAssetMenu(fileName = "HelpData", menuName = "iSpeakHelp/HelpDataAsset", order = 1)]
public class HelpData : ScriptableObject
{
    public HelpString[] HelpDataAssets;
}