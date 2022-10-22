using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    public KeyCode saveKey;
    public KeyCode loadKey;
    public KeyCode infoKey;
    public Transform BuildingParent;
    string saveFolder;
    List<FileInfo> saveFiles;
    public GameObject UIparent;
    public Text txtObj;
    int loadIDX = 0;
    bool loadDialogueOpen = false;
    bool infoDialogueOpen = false;
    public RobotHandle RH;
    void Start()
    {
        saveFolder = Application.dataPath + "/Saves/";
        System.IO.Directory.CreateDirectory(saveFolder);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(saveKey))
        {
            TriggerSave();
        }
        if (Input.GetKeyDown(loadKey))
        {
            if (loadDialogueOpen)
            {
                CloseLoadDialogue();
            }
            else
            {
                OpenLoadDialogue();
            }
        }
        if (Input.GetKeyDown(infoKey))
        {
            if (infoDialogueOpen)
            {
                CloseInfoDialogue();
            }
            else
            {
                OpenInfoDialogue();
            }
        }
        if (loadDialogueOpen)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                loadIDX--;
                if (loadIDX<0)
                {
                    loadIDX = saveFiles.Count - 1;
                }
                UpdateText(true);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                loadIDX++;
                if (loadIDX>=saveFiles.Count)
                {
                    loadIDX = 0;
                }
                UpdateText(true);
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                LoadSelected();
                CloseLoadDialogue();
            }
        }
    }
    void LoadSelected()
    {
        BuildingManager.instance.SetBuildMode(false);
        for (int i = 0; i < BuildingParent.childCount; i++)
        {
            Destroy(BuildingParent.GetChild(i).gameObject);
        }
        string filePath = saveFiles[loadIDX].FullName;
        string[] loadedLines = File.ReadAllLines(filePath);
        for (int i = 0; i <loadedLines.Length; i++)
        {
            BuildingManager.instance.BuildFromLoad(loadedLines[i]);
        }
    }
    void OpenInfoDialogue()
    {
        loadDialogueOpen = false;
        infoDialogueOpen = true;
        UIparent.SetActive(true);
        UpdateText(false);
    }
    void OpenLoadDialogue()
    {
        //load existing files
        loadDialogueOpen = true;
        UIparent.SetActive(true);
        DirectoryInfo saveDir = new DirectoryInfo(saveFolder);
        FileInfo[] saves = saveDir.GetFiles();
        saveFiles = new List<FileInfo>();
        for (int i = 0; i < saves.Length; i++)
        {
            if (!saves[i].Name.Contains(".txt.meta"))
            {
                saveFiles.Add(saves[i]);
            }
        }
        UpdateText(true);

    }
    void CloseInfoDialogue()
    {
        infoDialogueOpen = false;
        UIparent.SetActive(false);
    }
    void CloseLoadDialogue()
    {
        loadDialogueOpen = false;
        UIparent.SetActive(false);
    }
    void UpdateText(bool loading)
    {
        string allText;
        if (loading)
        {
            allText = "Found saves: \n";
            string tmp;
            for (int i = 0; i < saveFiles.Count; i++)
            {
                tmp = saveFiles[i].Name;
                if (i == loadIDX)
                {
                    tmp += "   < press enter to load";
                }
                tmp += "\n";
                allText += tmp;
            }
            if (saveFiles.Count > 1)
            {
                allText += "Use up/down arrow to select";
            }
        }
        else
        {
            allText = "KEYBINDS:\n\nF1     toggle info panel\nF11     toggle load screen\nF12     save layout\nWASDQE     move and rotate camera\nV     toggle vertical camera\nZ     zoom out\nLeftClick (on robot)     set camera follow target\nB     toggle build mode\nMouseWheel (in build mode)     cycle blueprints\nC (in build mode)     cycle snap size (1,0.1,0.01)\nLeftClick (in build mode)     build\n2x RightClick (in build mode)     remove";
        }
        txtObj.text = allText;
    }
    void TriggerSave()
    {
        DateTime now = DateTime.Now;
        string path = saveFolder+"save_d" + now.Day+"_"+now.Month+"_"+now.Year+"_t"+now.Hour+"_"+now.Minute+".txt";
        string saveData="";
        string thisLine;
        int totalChildren = BuildingParent.childCount;
        GameObject thisObj;
        for (int i = 0; i < totalChildren; i++)
        {
            thisObj = BuildingParent.GetChild(i).gameObject;
            thisLine = "";
            thisLine += "i" + GetBP_ID(thisObj.name) + "x" + thisObj.transform.position.x.ToString("F2") + "y" + thisObj.transform.position.y.ToString("F2") + "z" + thisObj.transform.position.z.ToString("F2") + "r" + thisObj.transform.rotation.eulerAngles.y.ToString("F0")+"e\n";
            saveData += thisLine;
        }
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.WriteAllText(path, saveData);
    }
    int GetBP_ID(string objName)
    {
        for (int i = 0; i < BuildingManager.instance.blueprints.Length; i++)
        {
            if (objName.Contains(BuildingManager.instance.blueprints[i].prefab.name))
            {
                return i;
            }
        }
        return -1;
    }
}
