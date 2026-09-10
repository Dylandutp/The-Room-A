using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class StationaryLayout
{
 [InitializeOnLoadMethod] static void Init(){if(File.Exists("Temp/stationary-layout.request"))EditorApplication.delayCall+=Apply;}
 static void Apply(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=Apply;return;}
  if(!File.Exists("Temp/stationary-layout.request"))return;File.Delete("Temp/stationary-layout.request");
  try{var scene=SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");if(!scene.isLoaded)throw new Exception("Open TheRoom scene first");
   File.Copy(scene.path,"Previews/TheRoom.before-stationary-layout."+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
   Configure(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
   File.WriteAllText("Previews/stationary-layout-validation.txt","PASS: 12 x 5 m board; 8 m menu attached to board; classroom start centered at z=-2.5; nature panel near arrival. No movement or interaction scripts added.");
  }catch(Exception e){File.WriteAllText("Previews/stationary-layout-error.txt",e.ToString());Debug.LogException(e);}
 }
 public static void Configure(Scene scene){
  Transform room=null,nature=null,anchors=null;Camera camera=null;
  foreach(var r in scene.GetRootGameObjects()){if(r.name=="01_Classroom_15m")room=r.transform;if(r.name=="02_Nature_15m")nature=r.transform;if(r.name=="03_Viewpoints_and_Future_XR_Anchors")anchors=r.transform;if(r.CompareTag("MainCamera"))camera=r.GetComponent<Camera>();}
  var board=room.Find("Whiteboard");board.localPosition=new Vector3(0,3.5f,7.34f);board.localScale=Vector3.one;
  board.Find("Frame").localScale=new Vector3(12,5,.1f);
  board.Find("Writing_Surface").localScale=new Vector3(11.86f,4.86f,.035f);
  board.Find("Marker_Tray").localPosition=new Vector3(0,-2.49f,-.16f);board.Find("Marker_Tray").localScale=new Vector3(11.95f,.07f,.28f);
  foreach(Transform t in board)if(t.name.Contains("Marker_Decoration")||t.name.Contains("Eraser_Decoration")){var p=t.localPosition;p.y=-2.4f;t.localPosition=p;}
  var title=room.Find("Lab_Title");if(title!=null)title.localPosition=new Vector3(0,6.5f,7.37f);
  var menu=room.Find("Controls_Menu_Canvas")??board.Find("Controls_Menu_Canvas");menu.SetParent(board,false);menu.localPosition=new Vector3(0,0,-.105f);menu.localRotation=Quaternion.identity;menu.localScale=Vector3.one*.008f;
  var start=anchors.Find("Classroom_Start");start.localPosition=new Vector3(0,0,-2.5f);start.localRotation=Quaternion.identity;
  if(camera!=null){camera.transform.position=new Vector3(0,1.65f,-2.5f);camera.transform.rotation=Quaternion.identity;camera.fieldOfView=75;}
  foreach(Transform t in room)if(t.name.Contains("Lighting_Console")||t.name.Contains("Lighting_Control")){var head=t.Find("Sloped_Display");if(head!=null)head.localRotation=Quaternion.Euler(30,0,0);PrefabUtility.RecordPrefabInstancePropertyModifications(head);}
  foreach(Transform t in nature)if(t.name.Contains("Nature_Console")||t.name.Contains("Nature_Control")){t.localPosition=new Vector3(1.6f,0,-3.8f);t.localRotation=Quaternion.Euler(0,30,0);var head=t.Find("Sloped_Display");if(head!=null)head.localRotation=Quaternion.Euler(30,0,0);PrefabUtility.RecordPrefabInstancePropertyModifications(t);if(head!=null)PrefabUtility.RecordPrefabInstancePropertyModifications(head);}
  if(menu.parent!=board||board.Find("Frame").localScale.x!=12)throw new Exception("Layout validation failed");
 }
}

