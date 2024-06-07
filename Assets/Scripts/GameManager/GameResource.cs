using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResource : MonoBehaviour
{
    public static GameResource instance;
    public static GameResource Instance
    {
        get
        {
            if(instance == null)
            {
                instance = Resources.Load<GameResource>("GameResource");
            }
            return instance;
        }
    }

    #region Header DUNGEON
    [Space(10)]
    [Header("DUNGEON")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the dungeon RoomNodeTypeListSO")]
    #endregion

    public RoomNodeTypeListSO RoomNodeTypeListSO;

}
