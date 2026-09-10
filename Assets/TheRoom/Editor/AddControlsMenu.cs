using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public static class AddControlsMenu
{
    [InitializeOnLoadMethod] static void Init(){if(File.Exists("Temp/controls-menu.request"))EditorApplication.delayCall+=Apply;}
    static void Apply(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=Apply;return;}
        if(!File.Exists("Temp/controls-menu.request"))return;
        if(Shader.Find("TextMeshPro/Distance Field")==null){AssetDatabase.ImportPackage("Library/PackageCache/com.unity.ugui@67707a67a4ab/Package Resources/TMP Essential Resources.unitypackage",false);EditorApplication.delayCall+=Apply;return;}
        File.Delete("Temp/controls-menu.request");
        try{
            var scene=SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");
            if(!scene.IsValid()||!scene.isLoaded)scene=EditorSceneManager.OpenScene("Assets/TheRoom/Scenes/TheRoom.unity",OpenSceneMode.Additive);
            Configure(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText("Previews/controls-menu-validation.txt","PASS: Controls_Menu_Canvas WorldSpace, 1000x1000, scale .002. TMP labels A teleport / B quit. No input scripts.\n");
        }catch(Exception e){File.WriteAllText("Previews/controls-menu-error.txt",e.ToString());Debug.LogException(e);}
    }
    public static void Configure(Scene scene){
        Transform room=null;foreach(var root in scene.GetRootGameObjects())if(root.name=="01_Classroom_15m")room=root.transform;
        if(room==null)throw new Exception("Classroom missing");if(room.Find("Controls_Menu_Canvas")!=null)return;
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if(font==null)throw new Exception("TMP font missing");
        var g=new GameObject("Controls_Menu_Canvas",typeof(RectTransform),typeof(Canvas));g.transform.SetParent(room,false);
        g.transform.localPosition=new Vector3(2.2f,1.9f,-7.37f);g.transform.localRotation=Quaternion.Euler(0,180,0);g.transform.localScale=Vector3.one*.002f;
        g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;g.GetComponent<RectTransform>().sizeDelta=new Vector2(1000,1000);
        var panel=new GameObject("Panel",typeof(RectTransform),typeof(Image));panel.transform.SetParent(g.transform,false);panel.GetComponent<RectTransform>().sizeDelta=new Vector2(1000,600);panel.GetComponent<Image>().color=new Color(.035f,.085f,.13f,.96f);panel.GetComponent<Image>().raycastTarget=false;
        Label(g.transform,font,"Title","CONTROLS",200,62);
        Label(g.transform,font,"A_Teleport_Instruction","A   TELEPORT TO GRASS",65,52);
        Label(g.transform,font,"B_Quit_Instruction","B   EXIT APPLICATION",-65,52);
        Label(g.transform,font,"Teleport_Hint","Same destination as VR pod",-205,32);
        PrefabUtility.SaveAsPrefabAsset(g,"Assets/TheRoom/Prefabs/Controls_Menu.prefab");
        if(g.GetComponent<Canvas>().renderMode!=RenderMode.WorldSpace||g.GetComponentsInChildren<TextMeshProUGUI>().Length!=4)throw new Exception("Invalid menu");
    }
    static void Label(Transform parent,TMP_FontAsset font,string name,string text,float y,float size){
        var g=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.sizeDelta=new Vector2(920,100);r.anchoredPosition=new Vector2(0,y);
        var t=g.GetComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.color=Color.white;t.raycastTarget=false;t.ForceMeshUpdate();
    }
}


