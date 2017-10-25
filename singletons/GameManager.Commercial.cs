using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
// using System;
public partial class GameManager : Singleton<GameManager> {
    
    public List<Commercial> listAllCommercials(){
        List<Commercial> passList = new List<Commercial>();
        UnityEngine.Object[] XMLObjects = Resources.LoadAll("data/commercials");
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
    // public Commercial LoadCommercialByName(string filename){
    //     try{
    //         Commercial commercial = null;
    //         TextAsset xml = Resources.Load("data/commercials/"+filename) as TextAsset;
    //         var serializer = new XmlSerializer(typeof(Commercial));
    //         var reader = new System.IO.StringReader(xml.text);
    //         commercial = serializer.Deserialize(reader) as Commercial;
    //         return commercial;
    //     } catch(Exception e){
    //         Debug.Log(e.Message);
    //         return null;
    //     }
    // }

    public void UnlockCommercial(string filename){
        //TODO: do not unlock same commercial twice
        // Debug.Log("unlocking "+filename);
        Commercial unlocked = Commercial.LoadCommercialByFilename(filename);
        foreach(Commercial commercial in data.unlockedCommercials){
            if (commercial.name == unlocked.name){
                // Debug.Log("already unlocked. skipping...");
                return;
            }
        }
        data.unlockedCommercials.Add(unlocked);
        data.newUnlockedCommercials.Add(unlocked);
    }
    public void EvaluateCommercial(Commercial commercial){
        bool success = false;
        if (activeCommercial != null){
            success = commercial.Evaluate(activeCommercial);
        }
        if (success){
            //process reward
           CommercialCompleted(commercial);
        } else {
            // do something to display why the commercial is not done yet
            Debug.Log("commercial did not pass.");
            Debug.Log("loaded commercial:");
            Debug.Log(activeCommercial);
        }
    }
    public void CommercialCompleted(Commercial commercial){
        // data.money += activeCommercial.reward;
        foreach (string unlock in activeCommercial.unlockUponCompletion){
            UnlockCommercial(unlock);
        }
        GameObject report = UINew.Instance.ShowMenu(UINew.MenuType.commercialReport);
        report.GetComponent<CommercialReportMenu>().Report(activeCommercial, commercial);
        if (activeCommercial.name != "Freestyle")
            data.completeCommercials.Add(activeCommercial);
        if (activeCommercial.name == "Healthy Eggplant Commercial"){
            // send the duplicator email, and deliver duplicator package
            ReceiveEmail("duplicator");
            ReceivePackage("duplicator");
        }
    }
}

