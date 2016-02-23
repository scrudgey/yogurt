using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

public partial class GameManager : Singleton<GameManager> {
    
    public List<Commercial> listAllCommercials(){
        List<Commercial> passList = new List<Commercial>();
        Object[] XMLObjects = Resources.LoadAll("data/commercials");
        List<TextAsset> xmlList = new List<TextAsset>();
        for (int i = 0; i < XMLObjects.Length; i++){
            xmlList.Add((TextAsset)XMLObjects[i]);
        }
        var serializer = new XmlSerializer(typeof(Commercial));
        foreach (TextAsset asset in xmlList){
            var reader = new System.IO.StringReader(asset.text);
            Commercial newCommercial = serializer.Deserialize(reader) as Commercial;
            passList.Add(newCommercial);
        }
        return passList;
    }
    public Commercial LoadCommercialByName(string filename){
        Commercial commercial = null;
        TextAsset xml = Resources.Load("data/commercials/"+filename) as TextAsset;
        var serializer = new XmlSerializer(typeof(Commercial));
        var reader = new System.IO.StringReader(xml.text);
        commercial = serializer.Deserialize(reader) as Commercial;
        return commercial;
    }

    void ScriptPrompt(){
        GameObject menu = Instantiate(Resources.Load("UI/ScriptSelector")) as GameObject;
        menu.GetComponent<Canvas>().worldCamera = cam;
    }

    public void UnlockCommercial(string filename){
        Commercial unlocked = LoadCommercialByName(filename);
        unlockedCommercials.Add(unlocked);
    }
    public void EvaluateCommercial(Commercial commercial){
        bool success = false;
        if (activeCommercial != null){
            success = commercial.Evaluate(activeCommercial);
        }
        if (success){
            //process reward
            money += activeCommercial.reward;
            foreach (string unlock in activeCommercial.unlockUponCompletion){
               UnlockCommercial(unlock);
            }
            GameObject report = Instantiate(Resources.Load("UI/CommercialReport")) as GameObject;
            report.GetComponent<CommercialReportMenu>().Report(activeCommercial);
            if (activeCommercial.name != "freestyle")
                completeCommercials.Add(activeCommercial);
        } else {
            // do something to display why the commercial is not done yet
        }
    }

}

