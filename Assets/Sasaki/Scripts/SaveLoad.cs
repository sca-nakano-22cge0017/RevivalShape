using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class JsonSaveManager<T>
{
    //Application.persistentDataPathはフォルダ名
    static string SavePath(string path)
        =>$"{Application.persistentDataPath}/{path}.json";

    public static void Save(T data, string path)
    {
        using (StreamWriter sw = new StreamWriter(SavePath(path), false))
        {
            string jsonstr = JsonUtility.ToJson(data, true);
            sw.Write(jsonstr);
            sw.Flush();
        }
    }

    public static T Load(string path)
    {
        if (File.Exists(SavePath(path)))///データが存在する場合は返す
        {
            using (StreamReader sr = new StreamReader(SavePath(path)))
            {
                string datastr = sr.ReadToEnd();
                return JsonUtility.FromJson<T>(datastr);
            }
        }
        //存在しない場合はdefaultを返却
        return default;
    }
}

public class SaveLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
