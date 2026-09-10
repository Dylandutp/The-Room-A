using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class RestoreXRDefaults
{
 [InitializeOnLoadMethod] static void Init(){if(File.Exists("Temp/restore-xr-defaults.request"))EditorApplication.delayCall+=Apply;}
 static void Apply(){
 if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=Apply;return;}
 if(!File.Exists("Temp/restore-xr-defaults.request"))return;File.Delete("Temp/restore-xr-defaults.request");
 try{
 var scene=SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");if(!scene.isLoaded)throw new Exception("Open TheRoom scene first");
 int count=0;foreach(var root in scene.GetRootGameObjects()){
 if(!root.name.StartsWith("XR Origin - "))continue;
 var pos=root.transform.position;var rot=root.transform.rotation;var active=root.activeSelf;
 // Revert individual property overrides without replacing objects or camera references.
 var mods=PrefabUtility.GetPropertyModifications(root);
 foreach(var mod in mods){
 if(mod.target==null)continue;
 foreach(var obj in root.GetComponentsInChildren<Transform>(true)){
 UnityEngine.Object instance=null;
 if(mod.target is GameObject && PrefabUtility.GetCorrespondingObjectFromSource(obj.gameObject)==mod.target)instance=obj.gameObject;
 else foreach(var component in obj.GetComponents<Component>())if(component!=null&&PrefabUtility.GetCorrespondingObjectFromSource(component)==mod.target){instance=component;break;}
 if(instance==null)continue;
 bool keep=instance==root.transform&&(mod.propertyPath.StartsWith("m_LocalPosition")||mod.propertyPath.StartsWith("m_LocalRotation")||mod.propertyPath.StartsWith("m_LocalEulerAnglesHint"));
 keep|=instance==root&&mod.propertyPath=="m_IsActive";
 if(!keep){var so=new SerializedObject(instance);var property=so.FindProperty(mod.propertyPath);if(property!=null)PrefabUtility.RevertPropertyOverride(property,InteractionMode.AutomatedAction);}
 break;
 }
 }
 root.transform.position=pos;root.transform.rotation=rot;root.SetActive(active);
 PrefabUtility.RecordPrefabInstancePropertyModifications(root.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(root);
 foreach(var mod in PrefabUtility.GetPropertyModifications(root)){
 if(mod.target==null)continue;
 if(mod.propertyPath.StartsWith("m_LocalPosition")||mod.propertyPath.StartsWith("m_LocalRotation")||mod.propertyPath.StartsWith("m_LocalEulerAnglesHint")||mod.propertyPath=="m_IsActive")continue;
 throw new Exception("Unexpected remaining override: "+mod.propertyPath);
 }
 count++;
 }
 if(count!=2)throw new Exception("Expected 2 XR rigs, got "+count);
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText("Previews/xr-defaults-validation.txt","PASS: 2 XR Origin instances restored to prefab defaults. Only root position, rotation and active state retained. Existing object/camera references preserved.");
 }catch(Exception e){File.WriteAllText("Previews/xr-defaults-error.txt",e.ToString());Debug.LogException(e);}
 }
}
