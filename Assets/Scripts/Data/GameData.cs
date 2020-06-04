using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 게임내 데이터를 저장할 클래스
public class GameData : MonoBehaviour
{
    private static GameData instance;

    public static GameData Instance
    {
        get
        {
            return instance;
        }
    }

    // 게임내 모든 아이템 정보
    public List<Item> itemDataListt = new List<Item>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);

    }


    public Item FindData(Item target)
    {
        return itemDataListt.Find(x => x.data.id == target.data.id);
    }
}
