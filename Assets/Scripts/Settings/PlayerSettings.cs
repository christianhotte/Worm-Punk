using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance;

    public CharacterData charData;

    private void OnEnable()
    {
        charData = new CharacterData();
        Instance = this;
    }

    public string CharDataToString() => JsonUtility.ToJson(Instance.charData);

}

public class CharacterData
{
    public int playerID;
    public string playerName;
    public Color testColor;
}