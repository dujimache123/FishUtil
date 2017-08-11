using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

//这个类主要是为了序列化json数据时用的
public class JsonControlPoint
{
    public double time;
    public double speedScale;
    public double r;

    public string GetJson()
    {
        string json = "{\"time\":" + (float)time + ",\"speedScale\":" + (float)speedScale + ",\"r\":" + (float)r + "}";
        return json;
    }
}

public class JsonPath
{
    public int id;
    public List<JsonControlPoint> pointList;

    public string GetJson()
    {
        string json = "";
        json = "{\"id\":" + id + ",\"pointList\":[";
        for(int i = 0; i < pointList.Count ; i ++)
        {
            if (i < pointList.Count - 1)
                json += pointList[i].GetJson() + ",";
            else
                json += pointList[i].GetJson();
        }
        json += "]}";
        
        return json;
    }
}

public class JsonPathList
{
    public List<JsonPath> PathList;
    public JsonPathList()
    {
        PathList = new List<JsonPath>();
    }
    public void AddPath(JsonPath path)
    {
        if (path != null)
        {
            PathList.Add(path);
        }
    }

    public bool ContainsPathId(int id)
    {
        foreach (JsonPath path in PathList)
        {
            if (path.id == id)
                return true;
        }
        return false;
    }

    public string GetJson()
    {
        string json = "{\"PathList\":[";
        for (int i = 0; i < PathList.Count; i++)
        {
            if (i < PathList.Count - 1)
                json += PathList[i].GetJson() + ",";
            else
                json += PathList[i].GetJson();
        }
        json += "]}";
        return json;
    }

    int CompareFunc(JsonPath path1, JsonPath path2)
    {
        if (path1.id < path2.id) return -1;
        else return 1;
    }

    public void SortById()
    {
        PathList.Sort(CompareFunc);
    }
}

public class PathConfigManager 
{

	private PathConfigManager() { }
	private static PathConfigManager mInstance;

    private Dictionary<int, FishPath> mFishPathMap = new Dictionary<int, FishPath>();
	public static PathConfigManager GetInstance()
	{
		if (mInstance == null)
		{
			mInstance = new PathConfigManager();
		}
		return mInstance;
	}

    public void Initialize()
    {
        LoadAllPathes();
    }

	public bool Save(string filepath,FishPath path)
	{
        JsonPath jsonPath = new JsonPath();
        jsonPath.id = path.mPathId;
        jsonPath.pointList = new List<JsonControlPoint>();
        foreach (FishPathControlPoint point in path.controlPoints)
        {
            JsonControlPoint cp = new JsonControlPoint();
            cp.time = point.mTime;
            cp.speedScale = point.mSpeedScale;
            cp.r = point.mRotationChange;
            jsonPath.pointList.Add(cp);
        }
        string json = jsonPath.GetJson();
        FileStream fs = new FileStream(filepath, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        sw.Write(json);
        sw.Flush();
        fs.Close();
		return true;
	}

    public FishPath Load(string filepath)
    {
        FishPath fishPath = ScriptableObject.CreateInstance<FishPath>();
        FileStream fs = new FileStream(filepath,FileMode.Open);
        StreamReader sr = new StreamReader(fs);
        string jsonStr = sr.ReadToEnd();
        JsonPath jsonPath = new JsonPath();
        jsonPath = JsonMapper.ToObject<JsonPath>(jsonStr);
        fishPath.ResetPath();
        foreach (JsonControlPoint point in jsonPath.pointList)
        {
            FishPathControlPoint fpcp = ScriptableObject.CreateInstance<FishPathControlPoint>();
            fpcp.mSpeedScale = (float)point.speedScale;
            fpcp.mRotationChange = (float)point.r;
            fpcp.mTime = (float)point.time;
            fishPath.AddPoint(fpcp);
        }
        return fishPath;
    }

    private void LoadAllPathes()
    {
        for (int i = 0; i < 6; i++)
        {
            TextAsset ta = Resources.Load<TextAsset>("Pathes/" + i.ToString());
            //Debug.Log(ta == null);
            if (ta != null)
            {
                string jsonStr = ta.text;
                if (jsonStr != null && jsonStr.Length > 0)
                {
                    JsonPath jsonPath = new JsonPath();
                    jsonPath = JsonMapper.ToObject<JsonPath>(jsonStr);
                    if (jsonPath == null) continue;
                    FishPath fishPath = ScriptableObject.CreateInstance<FishPath>();
                    foreach (JsonControlPoint point in jsonPath.pointList)
                    {
                        FishPathControlPoint fpcp = ScriptableObject.CreateInstance<FishPathControlPoint>();
                        fpcp.mSpeedScale = (float)point.speedScale;
                        fpcp.mRotationChange = (float)point.r;
                        fpcp.mTime = (float)point.time;
                        fishPath.AddPoint(fpcp);
                    }
                    mFishPathMap.Add(i, fishPath);
                }
            }
        }
    }

    public FishPath GetPath(int id)
    {
        FishPath path = null;
        mFishPathMap.TryGetValue(id, out path);
        return path;
    }
}
