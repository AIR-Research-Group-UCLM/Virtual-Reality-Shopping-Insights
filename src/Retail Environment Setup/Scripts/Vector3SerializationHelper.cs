using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Vector3SerializationHelper
{
    public float X, Y, Z;
    private static string filePath;
    
    private static string directoryName = "PLAYERS_JSONs";
    public Vector3SerializationHelper(Vector3 vector3)
    {
        X = vector3.x;
        Y = vector3.y;
        Z = vector3.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }

    public static void SerializeVector3List(List<Vector3> vectors, string fileName)
    {
        List<Vector3SerializationHelper> serializableList = new List<Vector3SerializationHelper>();
        foreach (var vector in vectors)
        {
            serializableList.Add(new Vector3SerializationHelper(vector));
        }
        
        string path = Path.Combine(Application.persistentDataPath, directoryName);
        string json = JsonConvert.SerializeObject(serializableList);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        filePath = Path.Combine(path, fileName);
        
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(json);
            }
        }
    }

    public static List<Vector3> DeserializeVector3List(string directoryAndFileNameWithExtension)
    {
        // directoryAndFileNameWithExtension must not begin with a slash
        filePath = Path.Combine(Application.persistentDataPath, directoryAndFileNameWithExtension);
        string json = "";
        
        if (File.Exists(filePath))
        {   
            json = File.ReadAllText(filePath);
        }
        List<Vector3SerializationHelper> serializableList = 
            JsonConvert.DeserializeObject<List<Vector3SerializationHelper>>(json);
        List<Vector3> vectors = new List<Vector3>();        foreach (var serializedVector in serializableList)
        {
            vectors.Add(serializedVector.ToVector3());
        }
        return vectors;
    }
}