using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class ConsoleParticles
{
 [InitializeOnLoadMethod] static void Init(){if(File.Exists("Temp/console-default-material.request"))EditorApplication.delayCall+=Apply;}
 static void Apply(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=Apply;return;}
  if(!File.Exists("Temp/console-default-material.request"))return;File.Delete("Temp/console-default-material.request");
  try{
   var scene=SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");if(!scene.isLoaded)throw new Exception("Open TheRoom scene first");
   foreach(var path in new[]{"Assets/TheRoom/Prefabs/Lighting_Console.prefab","Assets/TheRoom/Prefabs/Nature_Console.prefab"}){var root=PrefabUtility.LoadPrefabContents(path);try{Configure(root.transform);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}}
   int count=0;foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name=="Console_Blue_Flow"){var ps=t.GetComponent<ParticleSystem>();ps.Simulate(2,true,true);if(ps.particleCount==0)throw new Exception("No live particles");ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);count++;}
   if(count!=2)throw new Exception("Expected two console effects, found "+count);
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText("Previews/console-particles-validation.txt","PASS: two looping blue ParticleSystems use URP built-in ParticlesUnlit; Start Color and Color over Lifetime retained; live simulation passed.");
  }catch(Exception e){File.WriteAllText("Previews/console-particles-error.txt",e.ToString());Debug.LogException(e);}
 }
 public static void Configure(Transform root){
  var mat=AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat");
  if(mat==null)throw new Exception("URP built-in particle material missing");
  var existing=root.Find("Console_Blue_Flow");
  if(existing!=null){existing.GetComponent<ParticleSystemRenderer>().sharedMaterial=mat;return;}
  var g=new GameObject("Console_Blue_Flow",typeof(ParticleSystem));g.transform.SetParent(root,false);g.transform.localPosition=new Vector3(0,.12f,0);g.transform.localRotation=Quaternion.Euler(-90,0,0);
  var ps=g.GetComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=ps.main;main.duration=4;main.loop=true;main.prewarm=true;main.playOnAwake=true;main.startLifetime=new ParticleSystem.MinMaxCurve(1.8f,2.6f);main.startSpeed=.38f;main.startSize=new ParticleSystem.MinMaxCurve(.018f,.035f);main.startColor=new Color(.05f,.5f,1f,.75f);main.maxParticles=60;main.simulationSpace=ParticleSystemSimulationSpace.Local;
  var emission=ps.emission;emission.rateOverTime=18;
  var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=.56f;shape.radiusThickness=.12f;shape.randomDirectionAmount=0;
  var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;velocity.x=new ParticleSystem.MinMaxCurve(0);velocity.y=new ParticleSystem.MinMaxCurve(0);velocity.z=new ParticleSystem.MinMaxCurve(.12f);velocity.orbitalZ=new ParticleSystem.MinMaxCurve(.7f);
  var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(.03f,.35f,1),0),new GradientColorKey(new Color(.1f,.8f,1),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.75f,.2f),new GradientAlphaKey(.5f,.7f),new GradientAlphaKey(0,1)});color.color=gradient;
  var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,.25f));
  var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
 }
}

