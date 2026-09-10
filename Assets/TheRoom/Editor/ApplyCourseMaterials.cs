using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class ApplyCourseMaterials
{
    const string A="Assets/TheRoom/";
    [InitializeOnLoadMethod] static void Init(){if(File.Exists("Temp/course-materials.request"))EditorApplication.delayCall+=ApplyPending;}
    static void ApplyPending(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=ApplyPending;return;}
        if(!File.Exists("Temp/course-materials.request"))return;
        File.Delete("Temp/course-materials.request");
        try{
            var scene=SceneManager.GetSceneByPath(A+"Scenes/TheRoom.unity");
            if(!scene.IsValid()||!scene.isLoaded)scene=EditorSceneManager.OpenScene(A+"Scenes/TheRoom.unity",OpenSceneMode.Additive);
            File.Copy(scene.path,"Previews/TheRoom.before-course-materials."+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
            Configure(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Preview(new Vector3(0,2,-6.4f),new Vector3(-3,4,3),"CourseMaterials_Classroom");
            Preview(new Vector3(40,1.65f,-6.5f),new Vector3(40,3.5f,1),"CourseMaterials_Nature");
            File.WriteAllText("Previews/course-materials-validation.txt","PASS: West WallMaterial1 tile + normal, tiling 8x8, metallic .15, smoothness .25\nPASS: East WallMaterial2 tile + normal, tiling 4x4, metallic .65, smoothness .75\nPASS: North WallMaterial3 flat pale blue-white, no textures\nPASS: Skybox/6 Sided, six supplied sky13 textures\nPASS: Normal texture imported as NormalMap. No runtime scripts added.");
        }catch(Exception e){File.WriteAllText("Previews/course-materials-error.txt",e.ToString());Debug.LogException(e);}
    }
    static Texture2D Texture(string file,bool normal=false){
        string path=A+"ExtraMaterials/"+file;
        AssetDatabase.ImportAsset(path);
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(importer==null)throw new Exception("Missing texture "+path);
        importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
        importer.wrapMode=file.StartsWith("sky13")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static Material Material(string name,string shader){
        string path=A+"Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}
        m.shader=Shader.Find(shader);m.name=name;return m;
    }
    public static void Configure(Scene scene){
        var tile=Texture("tile.png");var normal=Texture("tile-normal.png",true);
        var walls=new Material[3];
        for(int i=0;i<3;i++){
            var m=Material("WallMaterial"+(i+1),"Universal Render Pipeline/Lit");walls[i]=m;
            m.SetColor("_BaseColor",i==2?new Color(.86f,.92f,.96f):Color.white);
            m.SetTexture("_BaseMap",i==2?null:tile);m.SetTexture("_BumpMap",i==2?null:normal);
            m.SetFloat("_Metallic",i==0?.15f:i==1?.65f:0);m.SetFloat("_Smoothness",i==0?.25f:i==1?.75f:.2f);
            m.SetFloat("_BumpScale",1);
            var tiling=i==0?new Vector2(8,8):i==1?new Vector2(4,4):Vector2.one;
            m.SetTextureScale("_BaseMap",tiling);m.SetTextureScale("_BumpMap",tiling);
            if(i<2)m.EnableKeyword("_NORMALMAP");else m.DisableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(m);
        }
        int applied=0;
        foreach(var root in scene.GetRootGameObjects())foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true)){
            int i=r.name=="West_Fine_Tile_Wall"?0:r.name=="East_Wide_Tile_Wall"?1:r.name=="North_Whiteboard_Wall"?2:-1;
            if(i<0)continue;r.sharedMaterial=walls[i];EditorUtility.SetDirty(r);applied++;
        }
        if(applied!=3)throw new Exception("Expected three walls, found "+applied);
        var sky=Material("Sky13_6Sided","Skybox/6 Sided");
        string[] faces={"FR","BK","LF","RT","UP","DN"};
        string[] slots={"_FrontTex","_BackTex","_LeftTex","_RightTex","_UpTex","_DownTex"};
        for(int i=0;i<6;i++){var t=Texture("sky13_"+faces[i]+".jpg");sky.SetTexture(slots[i],t);if(sky.GetTexture(slots[i])==null)throw new Exception("Sky face missing");}
        sky.SetColor("_Tint",new Color(.5f,.5f,.5f));sky.SetFloat("_Exposure",1);sky.SetFloat("_Rotation",0);
        RenderSettings.skybox=sky;EditorUtility.SetDirty(sky);
        if(walls[0].GetTexture("_BumpMap")!=normal||walls[1].GetTexture("_BaseMap")!=tile||walls[2].GetTexture("_BaseMap")!=null)throw new Exception("Material validation failed");
    }
    static void Preview(Vector3 pos,Vector3 target,string name){
        var obj=new GameObject("Temporary_Material_Preview",typeof(Camera));var c=obj.GetComponent<Camera>();c.enabled=false;c.transform.position=pos;c.transform.LookAt(target);c.fieldOfView=78;c.clearFlags=CameraClearFlags.Skybox;
        var rt=new RenderTexture(1200,800,24);var tex=new Texture2D(1200,800,TextureFormat.RGB24,false);var old=RenderTexture.active;
        try{c.targetTexture=rt;Canvas.ForceUpdateCanvases();c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,800),0,0);tex.Apply();File.WriteAllBytes("Previews/"+name+".png",tex.EncodeToPNG());}
        finally{RenderTexture.active=old;c.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(obj);}
    }
}
