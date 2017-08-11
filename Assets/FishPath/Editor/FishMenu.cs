using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using LitJson;
using System.Xml;

public class FishMenu
{
    [MenuItem("Fish/FishPath/CombineToOneFileJson")]
    static void CombineToOneFileJson()
    {
        string dir = Application.dataPath + "/FishPath/Resources/PathConfig/";
        string resultFile = EditorUtility.SaveFilePanel("Save", dir, "pathes","json");
        if (resultFile.Length > 0)
        {
            JsonPathList pathList = new JsonPathList();
            string[] files = Directory.GetFiles(dir,"*.bytes");
            foreach (string file in files)
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string jsonStr = sr.ReadToEnd();
                JsonPath jsonPath = new JsonPath();
                jsonPath = JsonMapper.ToObject<JsonPath>(jsonStr);
                pathList.AddPath(jsonPath);
                fs.Close();
            }
            string str = JsonMapper.ToJson(pathList);
            FileStream fs1 = new FileStream(resultFile, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.Write(str);
            sw.Flush();
            fs1.Close();
        }
    }

    [MenuItem("Fish/FishPath/CombineToOneFileXML")]
    static void CombineToOneFileXML()
    {
        string dir = Application.dataPath + "/FishPath/Resources/PathConfig/";
        string resultFile = EditorUtility.SaveFilePanel("Save", dir, "pathes", "xml");
        if (resultFile.Length > 0)
        {
            JsonPathList pathList = new JsonPathList();
            string[] files = Directory.GetFiles(dir, "*.bytes");
            foreach (string file in files)
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string jsonStr = sr.ReadToEnd();
                JsonPath jsonPath = new JsonPath();
                jsonPath = JsonMapper.ToObject<JsonPath>(jsonStr);
                pathList.AddPath(jsonPath);
                fs.Close();
            }

            XmlDocument xmlDoc = new XmlDocument();
            //创建根节点
            XmlElement pathes = xmlDoc.CreateElement("Pathes");
            xmlDoc.AppendChild(pathes);
            foreach (JsonPath path in pathList.PathList)
            {
                XmlElement onePath = xmlDoc.CreateElement("OnePath");
                pathes.AppendChild(onePath);
                onePath.SetAttribute("id", path.id.ToString());
                int index = 0;
                foreach (JsonControlPoint cp in path.pointList)
                {
                    XmlElement onePoint = xmlDoc.CreateElement("ControlPoint");
                    onePoint.SetAttribute("id", index.ToString());
                    onePoint.SetAttribute("time", cp.time.ToString());
                    onePoint.SetAttribute("speedMulti", cp.speedScale.ToString());
                    onePoint.SetAttribute("rotation", cp.r.ToString());
                    onePath.AppendChild(onePoint);
                    index++;
                }
            }
            xmlDoc.Save(resultFile);
        }
    }

    [MenuItem("Fish/FishCollider/AddCollider")]
    static void AddCollider()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
        {
            EditorUtility.DisplayDialog("", "请选择一条鱼!","OK");
            return;
        }
        int childcnt = obj.transform.childCount;
        float radius = 30;
        float disFront = 0, disBack = 0,lastFrontCircleScale = 0,lastBackCircleScale = 0;
        for (int i = 0; i < childcnt; i++)
        {
            Transform child = obj.transform.FindChild(i.ToString());
            float scale = child.localScale.x;
            if (i % 2 != 0)
            {
                disFront += radius * scale;
            }
            else if (i % 2 == 0 && i != 0)
            {
                disBack -= radius * scale;
            }
            else
            {
                lastBackCircleScale = scale;
                lastFrontCircleScale = scale;
            }
        }

        Object assetObj = Resources.Load("ColliderCircle");
        GameObject colliderCircle = GameObject.Instantiate(assetObj) as GameObject;
        colliderCircle.transform.parent = obj.transform;
        colliderCircle.transform.localScale = Vector3.one;
        colliderCircle.transform.localPosition = Vector3.zero;
        colliderCircle.name = (childcnt).ToString();
        UIWidget widget = colliderCircle.GetComponent<UIWidget>();
        if (childcnt == 0)
        {
            widget.pivot = UIWidget.Pivot.Center;
        }
        else
        {
            if (childcnt % 2 != 0)
            {
                widget.pivot = UIWidget.Pivot.Left;
                colliderCircle.transform.localPosition = new Vector3(disFront, 0, 0);
            }
            else
            {
                widget.pivot = UIWidget.Pivot.Right;
                colliderCircle.transform.localPosition = new Vector3(disBack, 0, 0);
            }
        }
    }

    [MenuItem("Fish/FishCollider/ExportCollider")]
    static void ExportCollider()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj != null)
        {
            string dir = Application.dataPath + "/FishPath/Resources/ColliderConfig/";
            string resultFile = EditorUtility.SaveFilePanel("Save", dir, "colliders", "txt");
            if (resultFile.Length > 0)
            {
                int childCnt = selectedObj.transform.childCount;
                string colliderMulStr = "";
                Dictionary<int, string> colliderDic = new Dictionary<int, string>();
                for (int i = 0; i < childCnt; i++)
                {
                    Transform child = selectedObj.transform.GetChild(i);
                    string[] nameSplit = child.gameObject.name.Split('_');
                    int id = int.Parse(nameSplit[1]);
                    colliderMulStr = "";
                    for (int j = 0; j < child.childCount; j++)
                    {
                        colliderMulStr += child.GetChild(j).localScale.x.ToString();
                        if (j < child.childCount - 1)
                            colliderMulStr += ",";
                    }
                    UIWidget widget = child.GetComponent<UIWidget>();
                    colliderMulStr = id.ToString() + '\t' + '\t' + widget.width + '\t' + widget.height + '\t' + child.childCount.ToString() + '\t' + colliderMulStr;
                    colliderDic.Add(id, colliderMulStr);
                }
                FileStream fs = new FileStream(resultFile,FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                for (int i = 0; i < 100; i++)
                {
                    if (colliderDic.ContainsKey(i))
                        sw.WriteLine(colliderDic[i]);
                    else
                        sw.WriteLine("");
                }
                sw.Flush();
                fs.Close();
            }
        }
    }

    [MenuItem("Fish/FishCollider/Snap")]
    static void Snap()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj != null)
        {
            int childCnt = selectedObj.transform.childCount;

            for (int i = 0; i < childCnt; i++)
            {
                Transform child = selectedObj.transform.GetChild(i);
                UIWidget widget = child.GetComponent<UIWidget>();
                widget.MakePixelPerfect();
            }
        }
    }
    int sortById(int a, int b)
    {
        if (a > b)
            return 1;
        else
            return 0;
    }

	[MenuItem("Fish/CreateFishSeason")]
	static void CreateFishSeasonInfo()
	{

	}

	[MenuItem("Fish/Export 3D Season")]
	static void ExportSeason()
	{
        GameObject fishSeasonObj = Selection.activeGameObject;
        if (fishSeasonObj == null)
        {
            Debug.LogError("no seasonobj selected!");
            return;
        }
        FishSeason fishseason = new FishSeason();
        OneWave oneWave = new OneWave();
        fishseason.AddWave(oneWave);
        oneWave.speed = 20;
        oneWave.pathid = 5;
        oneWave.rootea = fishSeasonObj.transform.localEulerAngles;
        int fishcellscnt = fishSeasonObj.transform.childCount;
        for (int i = 0; i < fishcellscnt; i++)
        {
            Transform child = fishSeasonObj.transform.GetChild(i);
            if (child.name == "HeadModel")
            {
                oneWave.ea = child.eulerAngles;
                oneWave.o = fishSeasonObj.transform.position;
                continue;
            }
            WaveFish wavefish = new WaveFish();
            wavefish.fkid = child.GetComponent<FishId>().fishKindId;
            wavefish.p = child.position;
            wavefish.s = child.localScale;
            oneWave.AddWaveFish(wavefish);
        }

        string destpath = EditorUtility.SaveFilePanel("", "Assets/Resources/SeasonConfigs/", "untitled","bytes");
        if (destpath.Length > 0)
        {
            string json = JsonUtility.ToJson(fishseason);
            FileStream fs = new FileStream(destpath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(json);
            sw.Flush();
            fs.Close();
            AssetDatabase.Refresh();
        }
	}

    [MenuItem("Fish/Load Season")]
    public static void LoadSeason()
    {
        string filepath = EditorUtility.OpenFilePanel("Open", Application.dataPath + "/Resources/SeasonConfigs/", "");
        if (filepath.Length > 0)
        {
            string filename = Path.GetFileNameWithoutExtension(filepath);
            TextAsset textasset = Resources.Load("SeasonConfigs/" + filename) as TextAsset;
            FishSeason season = JsonUtility.FromJson<FishSeason>(textasset.text);
            if (season != null)
            {
                foreach (OneWave wave in season.waves)
                {
                    GameObject oneWaveObj = new GameObject();
                    oneWaveObj.name = "FishSeason_Wave";
                    oneWaveObj.transform.localPosition = wave.o;
                    oneWaveObj.transform.eulerAngles = wave.rootea;
                    oneWaveObj.transform.localScale = Vector3.one;

                    GameObject head = GameObject.Instantiate<GameObject>(Resources.Load("FishPrefabs/FishSeason_Wave") as GameObject);
                    head.name = "HeadModel";
                    head.transform.parent = oneWaveObj.transform;
                    head.transform.position = wave.o;
                    head.transform.eulerAngles = wave.ea;
                    head.transform.localScale = Vector3.one;

                    foreach (WaveFish fish in wave.fishes)
                    {
                        GameObject fishobj = GameObject.Instantiate<GameObject>(Resources.Load("FishPrefabs/Fish_" + fish.fkid) as GameObject);
                        fishobj.transform.parent = oneWaveObj.transform;
                        fishobj.transform.position = fish.p;
                        fishobj.transform.eulerAngles = wave.ea;
                        fishobj.transform.localScale = fish.s;
                    }
                }
            }
        }
    }

    [MenuItem("Fish/Export 2D Season")]
    public static void Export2DSeason()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj != null)
        {
            string dir = Application.dataPath + "/FishPath/Resources/SeasonConfig/";
            string resultFile = EditorUtility.SaveFilePanel("Save", dir, "oneseason", "xml");

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement oneSeason = xmlDoc.CreateElement("OneSeason");
            xmlDoc.AppendChild(oneSeason);
            oneSeason.SetAttribute("id", "0");
            oneSeason.SetAttribute("angle", "0");
            for (int i = 0; i < selectedObj.transform.childCount; i++)
            {
                Transform childTrans = selectedObj.transform.GetChild(i);
                XmlElement fish = xmlDoc.CreateElement("fish");
                oneSeason.AppendChild(fish);
                int id = 0;
                if(childTrans.gameObject.name.Contains("ShaYu"))
                    id = 2;
                else if(childTrans.gameObject.name.Contains("FeiYu"))
                    id = 0;
                else if(childTrans.gameObject.name.Contains("DieYu"))
                    id = 1;
                fish.SetAttribute("id", id.ToString());
                fish.SetAttribute("x", childTrans.localPosition.x.ToString());
                fish.SetAttribute("y", childTrans.localPosition.y.ToString());
            }
            xmlDoc.Save(resultFile);
        }
    }
}
