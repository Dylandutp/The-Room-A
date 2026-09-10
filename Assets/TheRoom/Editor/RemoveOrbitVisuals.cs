using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class RemoveOrbitVisuals
{
    [InitializeOnLoadMethod] static void Init(){if(File.Exists("Temp/remove-orbits.request"))EditorApplication.delayCall+=Apply;}
    static void Apply(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=Apply;return;}
        if(!File.Exists("Temp/remove-orbits.request"))return;
        File.Delete("Temp/remove-orbits.request");
        try{
            var scene=SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");
            if(!scene.IsValid()||!scene.isLoaded)scene=EditorSceneManager.OpenScene("Assets/TheRoom/Scenes/TheRoom.unity",OpenSceneMode.Additive);
            File.Copy(scene.path,"Previews/TheRoom.before-remove-orbits."+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
            int removed=0;
            foreach(var root in scene.GetRootGameObjects())foreach(var tr in root.GetComponentsInChildren<Transform>(true)){
                if(tr==null)continue;
                if(tr.name=="Moon_Orbit"||tr.name=="Stable_Orbit"){Undo.DestroyObjectImmediate(tr.gameObject);removed++;}
                else if(tr.name=="Stable_Orbit_Guide_STATIC"){Undo.RecordObject(tr.gameObject,"Rename orbit pivot");tr.name="Stable_Orbit_Pivot";}
            }
            foreach(var root in scene.GetRootGameObjects())foreach(var tr in root.GetComponentsInChildren<Transform>(true))
                if(tr.name=="Moon_Orbit_Guide_STATIC"&&tr.childCount==0)Undo.DestroyObjectImmediate(tr.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            File.WriteAllText("Previews/remove-orbits-validation.txt","Removed orbit visuals: "+removed+"\nCelestial bodies and transforms preserved. No runtime rotation script added.");
        }catch(Exception e){File.WriteAllText("Previews/remove-orbits-error.txt",e.ToString());Debug.LogException(e);}
    }
}
