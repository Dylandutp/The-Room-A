using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public static class SetupRoomXR
{
 [InitializeOnLoadMethod] static void Init(){if(File.Exists("Temp/setup-room-xr.request"))EditorApplication.delayCall+=Apply;}
 static Type Find(string name){foreach(var t in TypeCache.GetTypesDerivedFrom<Component>())if(t.Name==name)return t;throw new Exception("Missing XR component "+name);}
 static void Apply(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=Apply;return;}
  if(!File.Exists("Temp/setup-room-xr.request"))return;File.Delete("Temp/setup-room-xr.request");
  try{
   var scene=SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");if(!scene.IsValid()||!scene.isLoaded)scene=EditorSceneManager.OpenScene("Assets/TheRoom/Scenes/TheRoom.unity",OpenSceneMode.Additive);
   File.Copy(scene.path,"Previews/TheRoom.before-xr."+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab");if(prefab==null)throw new Exception("Starter Assets XR prefab missing");
   Camera oldMain=null,oldNature=null;Transform anchors=null;
   foreach(var root in scene.GetRootGameObjects()){
    if(root.name=="03_Viewpoints_and_Future_XR_Anchors")anchors=root.transform;
    foreach(var c in root.GetComponentsInChildren<Camera>(true)){if(c.name=="Nature Preview Camera")oldNature=c;else if(c.name=="Main Camera"&&c.transform.parent==null)oldMain=c;}
   }
   if(oldMain==null||oldNature==null)throw new Exception("Expected original two cameras; stop to avoid duplicates");
   var main=Create(prefab,scene,"XR Origin - Classroom",anchors.Find("Classroom_Start").position,true);
   var nature=Create(prefab,scene,"XR Origin - Nature",anchors.Find("Nature_Arrival").position,false);
   var mainCamera=main.GetComponentInChildren<Camera>(true);var natureCamera=nature.GetComponentInChildren<Camera>(true);
   int canvases=0;
   foreach(var root in scene.GetRootGameObjects())foreach(var canvas in root.GetComponentsInChildren<Canvas>(true)){
    // Use the active classroom rig for all scene UI; the inactive nature rig is a prepared alternative.
    canvas.worldCamera=mainCamera;
    if(canvas.GetComponent<GraphicRaycaster>()!=null){if(canvas.GetComponent(Find("TrackedDeviceGraphicRaycaster"))==null)canvas.gameObject.AddComponent(Find("TrackedDeviceGraphicRaycaster"));canvases++;}
   }
   var es=UnityEngine.Object.FindFirstObjectByType<EventSystem>();if(es==null){var g=new GameObject("UI EventSystem",typeof(EventSystem));SceneManager.MoveGameObjectToScene(g,scene);es=g.GetComponent<EventSystem>();}
   foreach(var module in es.GetComponents<BaseInputModule>())UnityEngine.Object.DestroyImmediate(module);
   es.gameObject.AddComponent(Find("XRUIInputModule"));
   if(UnityEngine.Object.FindFirstObjectByType(Find("XRInteractionManager"))==null){var manager=new GameObject("XR Interaction Manager");SceneManager.MoveGameObjectToScene(manager,scene);manager.AddComponent(Find("XRInteractionManager"));}
   UnityEngine.Object.DestroyImmediate(oldMain.gameObject);UnityEngine.Object.DestroyImmediate(oldNature.gameObject);
   int activeCameras=0,listeners=0,missing=0;
   foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true)){
    missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
    var c=t.GetComponent<Camera>();if(c!=null&&c.isActiveAndEnabled)activeCameras++;
    var a=t.GetComponent<AudioListener>();if(a!=null&&a.isActiveAndEnabled)listeners++;
   }
   if(activeCameras!=1||listeners!=1||missing!=0)throw new Exception("XR validation: cameras="+activeCameras+" listeners="+listeners+" missing="+missing);
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
   File.WriteAllText("Previews/xr-setup-validation.txt","PASS: two Starter Assets XR Origin prefab instances; classroom active, nature inactive. One active camera and listener; no missing scripts. XR UI canvases: "+canvases+". Hardware tracking not tested.");
  }catch(Exception e){File.WriteAllText("Previews/xr-setup-error.txt",e.ToString());Debug.LogException(e);}
 }
 static GameObject Create(GameObject prefab,Scene scene,string name,Vector3 pos,bool active){
  var rig=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);rig.transform.position=pos;rig.transform.rotation=Quaternion.identity;
  rig.SetActive(active);return rig;
 }
}

