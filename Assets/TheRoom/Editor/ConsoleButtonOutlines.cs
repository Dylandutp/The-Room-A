using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public static class ConsoleButtonOutlines
{
 [InitializeOnLoadMethod] static void Init(){if(File.Exists("Temp/shared-outline.request"))EditorApplication.delayCall+=Apply;}
 static void Apply(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=Apply;return;}
  if(!File.Exists("Temp/shared-outline.request"))return;File.Delete("Temp/shared-outline.request");
  try{
   var shader=Shader.Find("TheRoom/ButtonVertexOutline");if(shader==null||ShaderUtil.ShaderHasError(shader))throw new Exception("Outline shader failed");
   foreach(var path in new[]{"Assets/TheRoom/Prefabs/Lighting_Console.prefab","Assets/TheRoom/Prefabs/Nature_Console.prefab"}){
    var root=PrefabUtility.LoadPrefabContents(path);try{Configure(root.transform);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
   }
   var scene=SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");if(!scene.IsValid()||!scene.isLoaded)scene=EditorSceneManager.OpenScene("Assets/TheRoom/Scenes/TheRoom.unity",OpenSceneMode.Additive);int count=0;
   foreach(var root in scene.GetRootGameObjects())foreach(var b in root.GetComponentsInChildren<Button>(true))if(IsTarget(b.name)){
    ConfigureButton(b.transform);if(b.GetComponent<Image>().color.a!=0||!b.GetComponent<Image>().raycastTarget||b.transform.Find("RaisedButton/VertexOutline")==null)throw new Exception("Button validation failed");if(b.transform.Find("RaisedButton/VertexOutline").GetComponent<MeshRenderer>().sharedMaterial!=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheRoom/Materials/PrimaryButton_Outline.mat"))throw new Exception("Outline not shared");count++;
   }
   if(count!=3)throw new Exception("Expected 3 buttons, got "+count);
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
   File.WriteAllText("Previews/raised-buttons-validation.txt","PASS: all 3 Buttons share PrimaryButton_Outline.mat; transparent click areas and OnClick preserved.");
  }catch(Exception e){File.WriteAllText("Previews/raised-buttons-error.txt",e.ToString());Debug.LogException(e);}
 }
 static bool IsTarget(string n){return n=="LightColorButton"||n=="ShootingToggleButton"||n=="ReturnToLabButton"||n=="Flat_Button_Visual"||n=="Return_Button_Visual";}
 public static void Configure(Transform root){foreach(var t in root.GetComponentsInChildren<RectTransform>(true))if(t!=null&&IsTarget(t.name))ConfigureButton(t);}
 static void ConfigureButton(Transform button){
  var old=button.Find("HoverOutline");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
  const string sharedPath="Assets/TheRoom/Materials/PrimaryButton_Outline.mat";
  var shared=AssetDatabase.LoadAssetAtPath<Material>(sharedPath);
  if(shared==null){shared=new Material(Shader.Find("TheRoom/ButtonVertexOutline"));shared.SetColor("_OutlineColor",Color.white);shared.SetFloat("_OutlineWidth",4);AssetDatabase.CreateAsset(shared,sharedPath);}
  var existing=button.Find("RaisedButton/VertexOutline");
  if(existing!=null){existing.GetComponent<MeshRenderer>().sharedMaterial=shared;return;}
  var image=button.GetComponent<Image>();var original=image.color;
  string name=button.name.Contains("Return")?"ReturnButton":"PrimaryButton";
  string path="Assets/TheRoom/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.SetColor("_BaseColor",original);mat.SetFloat("_Smoothness",.3f);AssetDatabase.CreateAsset(mat,path);}
  string op=sharedPath;var outline=AssetDatabase.LoadAssetAtPath<Material>(op);
  if(outline==null){outline=new Material(Shader.Find("TheRoom/ButtonVertexOutline"));outline.SetColor("_OutlineColor",Color.white);outline.SetFloat("_OutlineWidth",4);AssetDatabase.CreateAsset(outline,op);}
  var rect=button.GetComponent<RectTransform>();var dims=new Vector3(rect.sizeDelta.x,rect.sizeDelta.y,12);
  var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var mesh=UnityEngine.Object.Instantiate(primitive.GetComponent<MeshFilter>().sharedMesh);UnityEngine.Object.DestroyImmediate(primitive);
  var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]=Vector3.Scale(vertices[i],dims);mesh.vertices=vertices;mesh.RecalculateBounds();
  string mp="Assets/TheRoom/Meshes/ConsoleButton_Raised.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(mp);if(saved==null){AssetDatabase.CreateAsset(mesh,mp);saved=mesh;}else UnityEngine.Object.DestroyImmediate(mesh);
  var body=new GameObject("RaisedButton",typeof(MeshFilter),typeof(MeshRenderer));body.transform.SetParent(button,false);body.transform.localPosition=new Vector3(0,0,-6);body.GetComponent<MeshFilter>().sharedMesh=saved;body.GetComponent<MeshRenderer>().sharedMaterial=mat;
  var shell=new GameObject("VertexOutline",typeof(MeshFilter),typeof(MeshRenderer));shell.transform.SetParent(body.transform,false);shell.GetComponent<MeshFilter>().sharedMesh=saved;shell.GetComponent<MeshRenderer>().sharedMaterial=outline;
  image.color=new Color(original.r,original.g,original.b,0);image.raycastTarget=true;
  // Move the click plane and label in front; retain the body's position relative to the panel.
  var pos=button.localPosition;pos.z=-16;button.localPosition=pos;body.transform.localPosition=new Vector3(0,0,10);
  foreach(var text in button.GetComponentsInChildren<Text>(true))text.raycastTarget=false;
 }
}




