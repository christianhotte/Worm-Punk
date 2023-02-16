using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance;
    public CharacterData charData;

    private void OnEnable()
    {
        Instance = this;
        charData = new CharacterData();
    }

    public string CharDataToString() => JsonUtility.ToJson(charData);

}

public class CharacterData
{
    public int playerID;
    public string playerName;
    public Color testColor = new Color(1, 1, 1, 1);
}
