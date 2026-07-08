using System;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : SingletonBase<GameDataManager>
{
    [Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> datas;
    }

    private Dictionary<Type, object> allDataDict = new Dictionary<Type, object>();

    protected override void Awake()
    {
        base.Awake();
        LoadAllData();
    }

    public void LoadAllData()
    {
        LoadData<HeroBattleData>("HeroBattle");
        LoadData<EnemyBattleData>("EnemyBattle");
        LoadData<RoomData>("Room");
        LoadData<SupportItem>("SupportItem");
    }

    private void LoadData<T>(string table) where T : GameDataBase
    {
        string resourcePath = $"JsonOutput/{table}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        Dictionary<string, T> dataDict = new Dictionary<string, T>();

        if (textAsset != null)
        {
            try
            {
                string jsonString = textAsset.text;
                string wrappedJson = "{\"datas\":" + jsonString + "}";
                SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

                if (wrapper != null && wrapper.datas != null)
                {
                    foreach (T data in wrapper.datas)
                    {
                        if (!dataDict.ContainsKey(data.ID))
                        {
                            dataDict.Add(data.ID, data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
            }
        }

        if (!allDataDict.ContainsKey(typeof(T)))
        {
            allDataDict.Add(typeof(T), dataDict);
        }
    }

    // 매 스크립트마다 캐싱을 하기 귀찮고, 캐싱을 안하면 메모리 부담이 생기기에 미리 캐싱을 해두는 방식입니다.
    // 사용 시에 GetData<Hero>("hero_01").Name 이런식으로 사용하시면 됩니다.

    public T GetData<T>(string id) where T : GameDataBase
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        object dictObj;

        if (allDataDict.TryGetValue(typeof(T), out dictObj))
        {
            Dictionary<string, T> dict = dictObj as Dictionary<string, T>;

            if (dict != null)
            {
               T item;

                if (dict.TryGetValue(id, out item))
                {
                    return item;
                }
            }
        }

        return null;
    }
}