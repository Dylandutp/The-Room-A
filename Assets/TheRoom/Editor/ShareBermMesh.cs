using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ShareBermMesh
{
    [InitializeOnLoadMethod] static void Init() { if(File.Exists("Temp/share-berm.request")) EditorApplication.delayCall += Apply; }
    static void Apply()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.delayCall += Apply; return; }
        if(!File.Exists("Temp/share-berm.request")) return;
        File.Delete("Temp/share-berm.request");
        try
        {
            const string meshPath="Assets/TheRoom/Meshes/011_Berm.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(mesh==null) throw new Exception("Missing shared Berm mesh");
            var scene=SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");
            if(!scene.IsValid() || !scene.isLoaded) scene=EditorSceneManager.OpenScene("Assets/TheRoom/Scenes/TheRoom.unity",OpenSceneMode.Additive);
            File.Copy(scene.path,"Previews/TheRoom.before-share-berm."+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
            int count=0;
            foreach(var root in scene.GetRootGameObjects()) foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if(filter.sharedMesh==null || !AssetDatabase.GetAssetPath(filter.sharedMesh).EndsWith("_Berm.asset")) continue;
                Undo.RecordObject(filter,"Share Berm mesh");
                filter.sharedMesh=mesh;
                EditorUtility.SetDirty(filter);
                var collider=filter.GetComponent<MeshCollider>();
                if(collider!=null) { Undo.RecordObject(collider,"Share Berm mesh");collider.sharedMesh=mesh; }
                count++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText("Previews/shared-berm-validation.txt", "Berm objects sharing 011_Berm.asset: "+count+"\nTransforms and materials preserved.\n");
            Debug.Log("THEROOM_SHARED_BERM_COMPLETE: "+count);
        }
        catch(Exception e) { File.WriteAllText("Previews/shared-berm-error.txt",e.ToString());Debug.LogException(e); }
    }
}
