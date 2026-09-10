using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Editor-only scene authoring. No runtime behaviour is added to the scene.
public static class BuildTheRoom
{
    const string A = "Assets/TheRoom";
    static Material white, porcelain, dark, metal, floor, wallA, wallB, cyan, blue, ink, grass, stone, moonMat, orange;
    static Font font;
    static int meshId;
    static Transform room, nature;
    static GameObject ceiling, southWall;
    static readonly List<string> checks = new List<string>();

    [MenuItem("TheRoom/Build static environments")]
    public static void Build()
    {
        foreach (var p in new[]{"Materials","Meshes","Prefabs","Scenes","Textures","Previews"}) Directory.CreateDirectory(A+"/"+p);
        AssetDatabase.Refresh();
        meshId=0;
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        white=Mat("Porcelain_White",new Color(.91f,.94f,.96f),.1f,.3f);
        porcelain=Mat("Warm_White",new Color(.79f,.84f,.87f),.05f,.25f);
        dark=Mat("Equipment_Graphite",new Color(.045f,.065f,.09f),.25f,.38f);
        metal=Mat("Brushed_Aluminium",new Color(.43f,.53f,.61f),.7f,.4f);
        ink=Mat("Screen_Ink",new Color(.025f,.055f,.09f),0,.1f);
        cyan=Mat("Accent_Cyan",new Color(.12f,.7f,.85f),.1f,.35f,true);
        blue=Mat("Planet_Blue",new Color(.12f,.4f,.65f),.05f,.15f);
        moonMat=Mat("Moon_Stone",new Color(.61f,.65f,.7f),0,.15f);
        orange=Mat("Comet_Warm",new Color(.98f,.62f,.22f),0,.1f,true);
        grass=Mat("Meadow_Sage",new Color(.29f,.43f,.17f),0,.05f);
        stone=Mat("Berm_Moss",new Color(.23f,.34f,.13f),0,.05f);
        floor=Mat("Floor_Ceramic",new Color(.75f,.79f,.81f),.05f,.25f);
        wallA=Mat("Wall_01_Fine_Panels",new Color(.86f,.91f,.94f),.25f,.3f);
        wallB=Mat("Wall_02_Wide_Panels",new Color(.91f,.93f,.94f),.08f,.65f);
        var tile=TileTexture(); var normal=NormalTexture();
        foreach(var m in new[]{floor,wallA,wallB}) { m.SetTexture("_BaseMap",tile);m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.25f); }
        floor.SetTextureScale("_BaseMap",new Vector2(15,15));floor.SetTextureScale("_BumpMap",new Vector2(15,15));
        wallA.SetTextureScale("_BaseMap",new Vector2(8,8));wallA.SetTextureScale("_BumpMap",new Vector2(8,8));
        wallB.SetTextureScale("_BaseMap",new Vector2(4,4));wallB.SetTextureScale("_BumpMap",new Vector2(4,4));
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        scene.name="TheRoom";
        room=Root("01_Classroom_15m");nature=Root("02_Nature_15m");nature.position=new Vector3(40,0,0);
        Room();Nature();
        var spawns=Root("03_Viewpoints_and_Future_XR_Anchors");
        Empty("Classroom_Start",spawns,new Vector3(0,0,-6.2f));
        var vrAnchor=Empty("VR_Return_Anchor",spawns,new Vector3(-4,0,0));vrAnchor.localRotation=Quaternion.Euler(0,90,0);
        Empty("Nature_Arrival",spawns,new Vector3(40,0,-5.5f));
        var cam=CameraAt("Main Camera",new Vector3(0,1.65f,-6.3f),new Vector3(0,1.5f,1),true);
        cam.gameObject.tag="MainCamera";cam.gameObject.AddComponent<AudioListener>();
        var nc=CameraAt("Nature Preview Camera",new Vector3(40,1.65f,-6.5f),new Vector3(40,3.3f,.5f),false);
        nc.gameObject.transform.SetParent(spawns);
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.65f,.71f,.8f);
        RenderSettings.ambientEquatorColor=new Color(.48f,.5f,.53f);
        RenderSettings.ambientGroundColor=new Color(.25f,.29f,.32f);
        RenderSettings.reflectionIntensity=.3f;
        var sky=new Material(Shader.Find("TheRoom/GradientSky"));sky.name="Dusk_Gradient";
        sky=Save(sky,A+"/Materials/Dusk_Gradient.mat");RenderSettings.skybox=sky;
        var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        if(rp!=null){rp.renderScale=1;rp.msaaSampleCount=4;rp.shadowDistance=30;var rpSettings=new SerializedObject(rp);rpSettings.FindProperty("m_AdditionalLightsRenderingMode").intValue=1;rpSettings.FindProperty("m_AdditionalLightShadowsSupported").boolValue=true;rpSettings.FindProperty("m_SoftShadowsSupported").boolValue=true;rpSettings.ApplyModifiedPropertiesWithoutUndo();rp.maxAdditionalLightsCount=8;GraphicsSettings.defaultRenderPipeline=rp;QualitySettings.renderPipeline=rp;EditorUtility.SetDirty(rp);}
        PlayerSettings.productName="TheRoom";PlayerSettings.companyName="CS417";
        PlayerSettings.colorSpace=ColorSpace.Linear;
        RepairRoomUI.ConfigureScene(scene);
        ApplyCourseMaterials.Configure(scene);
        AddControlsMenu.Configure(scene);
        StationaryLayout.Configure(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,A+"/Scenes/TheRoom.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(A+"/Scenes/TheRoom.unity",true)};
        AssetDatabase.SaveAssets();
        Validate(scene);
        ShaderUtil.allowAsyncCompilation=false;
        Canvas.ForceUpdateCanvases();
        Preview(cam,"Classroom_Player",new Vector3(0,1.7f,-6.4f),new Vector3(0,1.6f,1.5f),78);
        Preview(nc,"Nature_Player",new Vector3(40,1.65f,-6.8f),new Vector3(40,3.1f,.7f),76);
        ceiling.SetActive(false);southWall.SetActive(false);
        Preview(cam,"Classroom_Layout",new Vector3(0,12.8f,-9.8f),new Vector3(0,.3f,0),76);
        ceiling.SetActive(true);southWall.SetActive(true);
        cam.transform.position=new Vector3(0,1.65f,-2.5f);cam.transform.rotation=Quaternion.identity;cam.fieldOfView=75;cam.enabled=true;
        nc.enabled=false;
        EditorSceneManager.SaveScene(scene,A+"/Scenes/TheRoom.unity");AssetDatabase.SaveAssets();
        Debug.Log("THEROOM_COMPLETE: static scene, materials, prefabs and previews saved.");
    }

    static Material Mat(string name,Color color,float metallic,float smooth,bool emissive=false)
    {var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);if(emissive){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*.7f);}return Save(m,A+"/Materials/"+name+".mat");}
    static T Save<T>(T o,string path) where T:UnityEngine.Object
    {var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old!=null){EditorUtility.CopySerialized(o,old);UnityEngine.Object.DestroyImmediate(o);return old;}AssetDatabase.CreateAsset(o,path);return o;}
    static Texture2D TileTexture()
    {var t=new Texture2D(128,128);for(int y=0;y<128;y++)for(int x=0;x<128;x++){float c=(x<2||y<2)?.77f:1f;t.SetPixel(x,y,new Color(c,c,c));}t.Apply();File.WriteAllBytes(A+"/Textures/Panel_Base.png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(A+"/Textures/Panel_Base.png");return AssetDatabase.LoadAssetAtPath<Texture2D>(A+"/Textures/Panel_Base.png");}
    static Texture2D NormalTexture()
    {var t=new Texture2D(128,128);for(int y=0;y<128;y++)for(int x=0;x<128;x++)t.SetPixel(x,y,new Color(x<3?.38f:.5f,y<3?.38f:.5f,1));t.Apply();File.WriteAllBytes(A+"/Textures/Panel_Normal.png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(A+"/Textures/Panel_Normal.png");var ti=(TextureImporter)AssetImporter.GetAtPath(A+"/Textures/Panel_Normal.png");ti.textureType=TextureImporterType.NormalMap;ti.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(A+"/Textures/Panel_Normal.png");}
    static Transform Root(string n){return new GameObject(n).transform;}
    static Transform Empty(string n,Transform p,Vector3 pos){var g=new GameObject(n).transform;g.SetParent(p,false);g.localPosition=pos;return g;}
    static GameObject Shape(string n,PrimitiveType type,Transform p,Vector3 pos,Vector3 size,Material m,bool collider=false)
    {var g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;}
    static GameObject Box(string n,Transform p,Vector3 pos,Vector3 size,Material m,bool col=false){return Shape(n,PrimitiveType.Cube,p,pos,size,m,col);}
    static GameObject Cylinder(string n,Transform p,Vector3 pos,float radius,float height,Material m,bool col=false){return Shape(n,PrimitiveType.Cylinder,p,pos,new Vector3(radius*2,height/2,radius*2),m,col);}
    static GameObject MeshObj(string n,Transform p,Mesh mesh,Material m,bool col=false)
    {mesh.name=n;mesh=Save(mesh,A+"/Meshes/"+(meshId++).ToString("D3")+"_"+n+".asset");var g=new GameObject(n,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(p,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=m;if(col)g.AddComponent<MeshCollider>().sharedMesh=mesh;g.isStatic=true;return g;}
    static void Tube(string name,Transform p,float radius,float thickness,float y,Material mat,float start=0,float end=360)
    {var v=new List<Vector3>();var tr=new List<int>();int steps=Mathf.CeilToInt((end-start)/5);int sides=8;for(int i=0;i<=steps;i++){float a=Mathf.Lerp(start,end,(float)i/steps)*Mathf.Deg2Rad;for(int j=0;j<sides;j++){float b=j*2*Mathf.PI/sides;v.Add(new Vector3(Mathf.Sin(a)*(radius+Mathf.Cos(b)*thickness),y+Mathf.Sin(b)*thickness,Mathf.Cos(a)*(radius+Mathf.Cos(b)*thickness)));}}for(int i=0;i<steps;i++)for(int j=0;j<sides;j++){int a=i*sides+j,b=i*sides+(j+1)%sides,c=a+sides,d=b+sides;tr.AddRange(new[]{a,b,c,b,d,c});}var me=new Mesh();me.SetVertices(v);me.SetTriangles(tr,0);me.RecalculateNormals();MeshObj(name,p,me,mat);}
    static GameObject Facet(string n,Transform p,Vector3 pos,float radius,Material mat,int subdivision=2)
    {var faces=new List<Vector3[]>();Vector3 u=Vector3.up,d=Vector3.down;Vector3[] e={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};for(int i=0;i<4;i++){faces.Add(new[]{u,e[i],e[(i+1)%4]});faces.Add(new[]{d,e[(i+1)%4],e[i]});}for(int k=0;k<subdivision;k++){var f=new List<Vector3[]>();foreach(var t in faces){var a=(t[0]+t[1]).normalized;var b=(t[1]+t[2]).normalized;var c=(t[2]+t[0]).normalized;f.Add(new[]{t[0],a,c});f.Add(new[]{a,t[1],b});f.Add(new[]{c,b,t[2]});f.Add(new[]{a,b,c});}faces=f;}var vs=new List<Vector3>();var ts=new List<int>();foreach(var f in faces){int j=vs.Count;vs.Add(f[0]*radius);vs.Add(f[1]*radius);vs.Add(f[2]*radius);ts.AddRange(new[]{j,j+1,j+2});}var mesh=new Mesh();mesh.SetVertices(vs);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();var g=MeshObj(n,p,mesh,mat);g.transform.localPosition=pos;return g;}
    static Transform CanvasPanel(string n,Transform p,Vector3 pos,Vector2 size,Quaternion rotation,Color background)
    {var g=new GameObject(n,typeof(RectTransform),typeof(Canvas));g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localRotation=rotation;g.transform.localScale=Vector3.one*.001f;var r=g.GetComponent<RectTransform>();r.sizeDelta=size*1000;g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;Rect("Background",r,Vector2.zero,size*1000,background);return r;}
    static RectTransform Rect(string n,Transform p,Vector2 pos,Vector2 size,Color col)
    {var g=new GameObject(n,typeof(RectTransform),typeof(Image));g.transform.SetParent(p,false);var rt=g.GetComponent<RectTransform>();rt.sizeDelta=size;rt.anchoredPosition=pos;var im=g.GetComponent<Image>();im.color=col;im.raycastTarget=false;return rt;}
    static void Text(string n,Transform p,string words,Vector2 pos,Vector2 size,int fontSize,Color c,TextAnchor align=TextAnchor.MiddleCenter)
    {var g=new GameObject(n,typeof(RectTransform),typeof(UnityEngine.UI.Text));g.transform.SetParent(p,false);var rt=g.GetComponent<RectTransform>();rt.sizeDelta=size;rt.anchoredPosition=pos;var tx=g.GetComponent<UnityEngine.UI.Text>();tx.font=font;tx.text=words;tx.fontSize=fontSize;tx.color=c;tx.alignment=align;tx.raycastTarget=false;tx.horizontalOverflow=HorizontalWrapMode.Wrap;tx.verticalOverflow=VerticalWrapMode.Truncate;}
    static readonly Color Navy=new Color(.035f,.085f,.13f), Teal=new Color(.12f,.65f,.76f), Paper=new Color(.9f,.95f,.96f);
    static void Label(Transform p,string n,string words,Vector3 pos,Vector2 size,Quaternion rot,int fs=80)
    {var c=CanvasPanel(n,p,pos,size,rot,Color.clear);Text("Text",c,words,Vector2.zero,size*1000,fs,Navy);}

    static void Room()
    {
        var arch=Empty("Architecture",room,Vector3.zero);
        Box("Floor_15x15",arch,new Vector3(0,-.1f,0),new Vector3(15,.2f,15),floor,true);
        ceiling=Box("Ceiling_15m",arch,new Vector3(0,15.1f,0),new Vector3(15,.2f,15),white,true);
        Box("North_Whiteboard_Wall",arch,new Vector3(0,7.5f,7.6f),new Vector3(15.4f,15,.2f),white,true);
        Box("West_Fine_Tile_Wall",arch,new Vector3(-7.6f,7.5f,0),new Vector3(.2f,15,15),wallA,true);
        Box("East_Wide_Tile_Wall",arch,new Vector3(7.6f,7.5f,0),new Vector3(.2f,15,15),wallB,true);
        southWall=Empty("South_Wall_With_Door",arch,Vector3.zero).gameObject;
        Box("South_Left",southWall.transform,new Vector3(-4.2f,7.5f,-7.6f),new Vector3(6.6f,15,.2f),white,true);
        Box("South_Right",southWall.transform,new Vector3(4.2f,7.5f,-7.6f),new Vector3(6.6f,15,.2f),white,true);
        Box("South_Header",southWall.transform,new Vector3(0,8.75f,-7.6f),new Vector3(1.8f,12.5f,.2f),white,true);
        for(int side=-1;side<=1;side+=2){Box("Wall_Base_Trim",arch,new Vector3(side*7.45f,.16f,0),new Vector3(.08f,.3f,15),porcelain);Box("Wall_Light_Rail",arch,new Vector3(side*7.44f,3.1f,0),new Vector3(.025f,.035f,15),cyan);for(int z=-6;z<=6;z+=3)Box("Wall_Pilaster",arch,new Vector3(side*7.43f,1.55f,z),new Vector3(.1f,3.1f,.06f),porcelain);}
        var door=Empty("Entry_Door",southWall.transform,new Vector3(0,0,-7.48f));door.localRotation=Quaternion.Euler(0,180,0);
        Box("Door_Leaf",door,new Vector3(0,1.23f,0),new Vector3(1.68f,2.46f,.1f),porcelain,true);
        for(int s=-1;s<=1;s+=2)Box("Jamb",door,new Vector3(s*.91f,1.28f,0),new Vector3(.12f,2.56f,.19f),metal);
        Box("Header",door,new Vector3(0,2.55f,0),new Vector3(1.94f,.12f,.19f),metal);
        Box("Handle",door,new Vector3(.59f,1,-.1f),new Vector3(.25f,.05f,.08f),metal);
        var exit=CanvasPanel("Exit_Flat_UI_VisualOnly",door,new Vector3(0,1.55f,-.058f),new Vector2(.65f,.27f),Quaternion.identity,Navy);
        Text("Exit",exit,"EXIT",Vector2.zero,new Vector2(600,220),80,Paper);
        var board=Empty("Whiteboard",room,new Vector3(0,2.05f,7.34f));
        Box("Frame",board,Vector3.zero,new Vector3(5.2f,2.1f,.1f),metal);
        Box("Writing_Surface",board,new Vector3(0,0,-.065f),new Vector3(5.06f,1.96f,.035f),white,true);
        Box("Marker_Tray",board,new Vector3(0,-1.04f,-.16f),new Vector3(5.15f,.07f,.28f),porcelain);
        for(int i=0;i<3;i++){var pen=Cylinder("Marker_Decoration",board,new Vector3(-1+i*.32f,-.985f,-.18f),.025f,.22f,i==0?cyan:i==1?dark:blue);pen.transform.localRotation=Quaternion.Euler(0,0,90);}
        Box("Eraser_Decoration",board,new Vector3(.9f,-.94f,-.17f),new Vector3(.24f,.1f,.12f),dark);
        Label(room,"Lab_Title","IMMERSIVE COMPUTING LAB",new Vector3(0,3.7f,7.37f),new Vector2(6,.45f),Quaternion.identity,105);
        var stations=Empty("Workstations_Grid_1_3_7_9",room,Vector3.zero);
        var template=Workstation();
        Vector3[] spots={new Vector3(-4,0,4),new Vector3(4,0,4),new Vector3(-4,0,-4),new Vector3(4,0,-4)};
        for(int i=0;i<spots.Length;i++){var g=(GameObject)PrefabUtility.InstantiatePrefab(template);g.name="PC_0"+(i+1)+"_Grid_"+new[]{1,3,7,9}[i];g.transform.SetParent(stations,false);g.transform.localPosition=spots[i];g.transform.localRotation=Quaternion.LookRotation(spots[i].normalized);}
        Pod("VR_Pod_Grid4_Entry_West",new Vector3(-4,0,0),90,"VR","ENTER NATURE",room);
        Pod("AR_Pod_Grid6_Entry_East",new Vector3(4,0,0),-90,"AR","AUGMENTED REALITY",room);
        Lectern(room,Vector3.zero,false);
        LightAt("Ceiling_Central_Point",room,new Vector3(0,14.8f,0),Color.white,100,25,true);
        Cylinder("Ceiling_Luminaire",arch,new Vector3(0,14.89f,0),.7f,.12f,white);
        Cylinder("Ceiling_Diffuser",arch,new Vector3(0,14.81f,0),.61f,.03f,cyan);
        foreach(int x in new[]{-4,4})foreach(int z in new[]{-4,4})LightAt("Soft_Work_Light",room,new Vector3(x,4.2f,z),new Color(.87f,.95f,1),10,10,false);
    }

    static GameObject Workstation()
    {
        var t=Root("Workstation");
        Box("Desktop",t,new Vector3(0,.76f,0),new Vector3(1.65f,.075f,.8f),white,true);
        foreach(float x in new[]{-.7f,.7f}){Box("Leg",t,new Vector3(x,.37f,.05f),new Vector3(.055f,.74f,.62f),metal);Box("Foot",t,new Vector3(x,.035f,.05f),new Vector3(.11f,.055f,.72f),dark);}
        Box("Cable_Rail",t,new Vector3(0,.58f,.29f),new Vector3(1.45f,.1f,.055f),porcelain);
        Box("Monitor_Base",t,new Vector3(-.12f,.817f,.15f),new Vector3(.38f,.035f,.24f),dark);
        Box("Monitor_Stand",t,new Vector3(-.12f,.96f,.22f),new Vector3(.07f,.3f,.06f),metal);
        Box("Monitor",t,new Vector3(-.12f,1.17f,.21f),new Vector3(.86f,.51f,.06f),dark,true);
        var screen=CanvasPanel("Screen_Only_Interaction_Placeholder",t,new Vector3(-.12f,1.17f,.175f),new Vector2(.8f,.45f),Quaternion.identity,Navy);
        Rect("Accent",screen,new Vector2(-370,0),new Vector2(6,370),Teal);
        Text("Heading",screen,"LAB / WORKSTATION",new Vector2(0,118),new Vector2(690,65),43,Paper);
        Text("Main",screen,"IMMERSIVE\nCOMPUTING",new Vector2(0,12),new Vector2(700,155),64,Paper);
        Text("Status",screen,"STANDBY",new Vector2(0,-145),new Vector2(690,50),30,Teal);
        Box("Keyboard",t,new Vector3(-.2f,.82f,-.21f),new Vector3(.53f,.026f,.19f),dark);
        for(int row=0;row<4;row++)for(int k=0;k<12;k++)Box("Key",t,new Vector3(-.44f+k*.043f,.837f,-.273f+row*.042f),new Vector3(.032f,.008f,.028f),metal);
        Shape("Mouse",PrimitiveType.Sphere,t,new Vector3(.24f,.834f,-.21f),new Vector3(.09f,.045f,.145f),dark);
        Box("Computer_Tower",t,new Vector3(.63f,1.03f,.13f),new Vector3(.23f,.46f,.38f),porcelain,true);
        Box("Tower_Front",t,new Vector3(.63f,1.03f,-.066f),new Vector3(.2f,.42f,.014f),dark);
        Box("Power_Led",t,new Vector3(.63f,1.18f,-.077f),new Vector3(.09f,.014f,.007f),cyan);
        var chair=Empty("Chair",t,new Vector3(0,0,-.93f));
        Cylinder("Seat_Post",chair,new Vector3(0,.24f,0),.04f,.44f,metal);
        Box("Seat",chair,new Vector3(0,.46f,0),new Vector3(.5f,.095f,.48f),dark,true);
        Box("Back",chair,new Vector3(0,.77f,-.23f),new Vector3(.5f,.52f,.075f),dark,true);
        for(int i=0;i<5;i++){var foot=Empty("Spoke",chair,Vector3.zero);foot.localRotation=Quaternion.Euler(0,i*72,0);Box("Leg",foot,new Vector3(0,.09f,.17f),new Vector3(.045f,.045f,.36f),metal);Shape("Caster",PrimitiveType.Sphere,foot,new Vector3(0,.055f,.33f),Vector3.one*.09f,dark);}
        for(int s=-1;s<=1;s+=2){Box("Arm_Support",chair,new Vector3(s*.29f,.54f,.05f),new Vector3(.025f,.22f,.025f),metal);Box("Armrest",chair,new Vector3(s*.29f,.66f,.02f),new Vector3(.065f,.04f,.27f),dark);}
        var prefab=PrefabUtility.SaveAsPrefabAsset(t.gameObject,A+"/Prefabs/Workstation.prefab");UnityEngine.Object.DestroyImmediate(t.gameObject);return prefab;
    }
    static void Pod(string name,Vector3 pos,float yaw,string title,string subtitle,Transform parent)
    {
        var p=Root(name);
        Cylinder("Low_Base",p,new Vector3(0,.035f,0),1.3f,.07f,porcelain,true);
        Cylinder("Standing_Surface",p,new Vector3(0,.075f,0),1.21f,.014f,white);
        Tube("Base_Accent",p,1.24f,.018f,.09f,cyan);
        Tube("Crown",p,1.24f,.045f,2.45f,white);
        Tube("Crown_Light",p,1.24f,.012f,2.40f,cyan);
        Tube("Rear_Safety_Rail",p,1.24f,.035f,.87f,metal,-110,110);
        foreach(float a in new[]{-105f,0,105f}){float ar=a*Mathf.Deg2Rad;Cylinder("Rear_Support",p,new Vector3(Mathf.Sin(ar)*1.24f,1.26f,Mathf.Cos(ar)*1.24f),.035f,2.4f,white,true);}
        var dock=Empty("Equipment_Cluster_SINGLE_FUTURE_HOTSPOT",p,new Vector3(0,0,.68f));
        Box("Dock_Pedestal",dock,new Vector3(0,.49f,0),new Vector3(.55f,.84f,.34f),white,true);
        Box("Dock_Top",dock,new Vector3(0,.93f,0),new Vector3(.76f,.06f,.43f),metal);
        var head=Empty("Headset_Decoration",dock,new Vector3(0,1.11f,0));
        Shape("Headset_Shell",PrimitiveType.Sphere,head,Vector3.zero,new Vector3(.29f,.16f,.2f),white);
        Shape("Visor",PrimitiveType.Sphere,head,new Vector3(0,0,-.07f),new Vector3(.255f,.125f,.08f),dark);
        var strap=Empty("Headband",head,new Vector3(0,.04f,.045f));Tube("Headband_Ring",strap,.12f,.016f,0,dark);
        for(int s=-1;s<=1;s+=2){var ctrl=Empty("Controller_Decoration",dock,new Vector3(s*.27f,1.02f,-.01f));ctrl.localRotation=Quaternion.Euler(-20,0,s*-15);Cylinder("Grip",ctrl,Vector3.zero,.029f,.15f,white);Shape("Control_Face",PrimitiveType.Sphere,ctrl,new Vector3(0,.076f,0),new Vector3(.082f,.025f,.105f),dark);Shape("Thumbstick",PrimitiveType.Sphere,ctrl,new Vector3(0,.096f,.01f),Vector3.one*.019f,metal);}
        Label(p,"Pod_Identity",title+"  /  "+subtitle,new Vector3(0,.14f,-.57f),new Vector2(1.55f,.3f),Quaternion.Euler(90,0,0),55);
        var prefab=PrefabUtility.SaveAsPrefabAsset(p.gameObject,A+"/Prefabs/"+title+"_Pod.prefab");UnityEngine.Object.DestroyImmediate(p.gameObject);
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab);instance.name=name;instance.transform.SetParent(parent,false);instance.transform.localPosition=pos;instance.transform.localRotation=Quaternion.Euler(0,yaw,0);
    }
    static void Lectern(Transform parent,Vector3 pos,bool outdoors)
    {
        var p=Root(outdoors?"Nature_Control_Panel":"Lighting_Control_Grid5");
        Box("Foot",p,new Vector3(0,.04f,0),new Vector3(.72f,.08f,.6f),porcelain,true);
        Box("Pedestal",p,new Vector3(0,.52f,.08f),new Vector3(.4f,.96f,.32f),white,true);
        var head=Empty("Sloped_Display",p,new Vector3(0,1.13f,0));head.localRotation=Quaternion.Euler(60,0,0);
        Box("Screen_Housing",head,Vector3.zero,new Vector3(.83f,.62f,.07f),white,true);
        var c=CanvasPanel("Flat_2D_UI_VISUAL_ONLY",head,new Vector3(0,0,-.04f),new Vector2(.75f,.54f),Quaternion.identity,Navy);
        Text("Title",c,outdoors?"ORBIT LAB":"ROOM LIGHTING",new Vector2(0,191),new Vector2(650,60),46,Paper);
        Rect("Divider",c,new Vector2(0,145),new Vector2(625,2),Teal);
        var b=Rect("Flat_Button_Visual",c,new Vector2(0,outdoors?56:5),new Vector2(625,110),new Color(.08f,.38f,.53f));
        Text("Button_Label",b,outdoors?"SHOOTING: OFF / ON":"LIGHT COLOR",Vector2.zero,new Vector2(600,100),43,Paper);
        if(outdoors){var ret=Rect("Return_Button_Visual",c,new Vector2(0,-85),new Vector2(625,110),new Color(.16f,.3f,.35f));Text("Return_Label",ret,"RETURN TO LAB",Vector2.zero,new Vector2(600,100),43,Paper);}
        Text("Hint",c,outdoors?"A : FIRE":"WHITE  /  CYAN  /  VIOLET  /  AMBER",new Vector2(0,-204),new Vector2(650,55),outdoors?34:23,Teal);
        ConsoleParticles.Configure(p);
        ConsoleButtonOutlines.Configure(p);
        var prefab=PrefabUtility.SaveAsPrefabAsset(p.gameObject,A+"/Prefabs/"+(outdoors?"Nature_Console":"Lighting_Console")+".prefab");UnityEngine.Object.DestroyImmediate(p.gameObject);
        var inst=(GameObject)PrefabUtility.InstantiatePrefab(prefab);inst.transform.SetParent(parent,false);inst.transform.localPosition=pos;if(outdoors)inst.transform.localRotation=Quaternion.Euler(0,-25,0);
    }
    static void LightAt(string n,Transform p,Vector3 pos,Color color,float intensity,float range,bool shadows)
    {var t=Empty(n,p,pos);var l=t.gameObject.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=shadows?LightShadows.Soft:LightShadows.None;l.shadowBias=.03f;l.shadowNormalBias=.2f;}
    static void Nature()
    {
        var rnd=new System.Random(417);var vs=new List<Vector3>();var ts=new List<int>();var colors=new List<Color>();int count=15;
        for(int z=0;z<count;z++)for(int x=0;x<count;x++){float h=0;Vector3 a=new Vector3(x-7.5f,h,z-7.5f),b=a+Vector3.right,c=a+Vector3.forward,d=a+Vector3.right+Vector3.forward;foreach(var f in new[]{new[]{a,c,b},new[]{b,c,d}}){int j=vs.Count;float t=(float)rnd.NextDouble()*.16f+.85f;foreach(var v in f){vs.Add(v);colors.Add(new Color(t,t,t,1));}ts.AddRange(new[]{j,j+1,j+2});}}
        // Face variation uses a vertex-color material, keeping the scene texture-free.
        var gm=new Material(Shader.Find("TheRoom/VertexLit"));gm.name="Meadow_Facets";gm.SetColor("_BaseColor",new Color(.36f,.48f,.19f));gm=Save(gm,A+"/Materials/Meadow_Facets.mat");
        var ground=new Mesh();ground.SetVertices(vs);ground.SetTriangles(ts,0);ground.SetColors(colors);ground.RecalculateNormals();MeshObj("Grass_15x15",nature,ground,gm,true);
        var berm=Empty("Low_Boundary_Berms",nature,Vector3.zero); GameObject bermTemplate=null;
        for(int s=0;s<4;s++){var edge=Empty("Boundary_"+s,berm,Vector3.zero);edge.localRotation=Quaternion.Euler(0,s*90,0);for(int i=0;i<6;i++){GameObject g; if(bermTemplate==null){g=Facet("Berm",edge,Vector3.zero,1,stone,1);bermTemplate=g;}else{g=UnityEngine.Object.Instantiate(bermTemplate,edge,false);g.name="Berm";meshId++;}g.transform.localPosition=new Vector3(-6.25f+i*2.5f,.15f,6.8f);g.transform.localScale=new Vector3(1.8f,.7f+(float)rnd.NextDouble()*.5f,.8f);}}
        foreach(int s in new[]{-1,1}){var g=Box("Boundary_Collider",nature,new Vector3(s*7.5f,1,0),new Vector3(.1f,2,15),stone,true);g.GetComponent<Renderer>().enabled=false;g=Box("Boundary_Collider",nature,new Vector3(0,1,s*7.5f),new Vector3(15,2,.1f),stone,true);g.GetComponent<Renderer>().enabled=false;}
        var system=Empty("Celestial_System_STATIC",nature,new Vector3(0,5,1));
        var planet=Facet("Planet",system,Vector3.zero,1.28f,blue,3);
        var orbitRotation=Quaternion.Euler(10,0,12);
        var moon=Facet("Moon_CHILD_OF_PLANET",planet.transform,orbitRotation*new Vector3(3.2f,0,0),.43f,moonMat,2);
        meshId++; // Reserve the former orbit mesh ID for stable asset paths.
        var stable=Empty("Stable_Orbit_Pivot",system,Vector3.zero);stable.localRotation=Quaternion.Euler(-20,0,-18);meshId++;
        Facet("Stable_Orbit_Body_STATIC",stable,new Vector3(-2.1f,0,1.05f),.13f,orange,1);
        var comet=Facet("Comet_STATIC",system,new Vector3(-3.6f,1.3f,-.2f),.11f,orange,1);
        var tail=new GameObject("Comet_Trail_STATIC",typeof(LineRenderer));tail.transform.SetParent(comet.transform,false);var line=tail.GetComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=3;line.SetPositions(new[]{Vector3.zero,new Vector3(-.6f,.25f,0),new Vector3(-1.3f,.6f,0)});line.startWidth=.07f;line.endWidth=0;line.sharedMaterial=cyan;line.numCapVertices=3;
        Lectern(nature,new Vector3(3,0,-3.2f),true);
        LightAt("Dusk_Warm_Key",nature,new Vector3(-5,8,-3),new Color(1,.74f,.52f),90,22,true);
        LightAt("Sky_Fill",nature,new Vector3(4,7,4),new Color(.55f,.69f,1),35,22,false);
    }
    static Camera CameraAt(string name,Vector3 pos,Vector3 target,bool enabled)
    {var g=new GameObject(name,typeof(Camera));g.transform.position=pos;g.transform.LookAt(target);var c=g.GetComponent<Camera>();c.fieldOfView=75;c.nearClipPlane=.05f;c.farClipPlane=150;c.clearFlags=CameraClearFlags.Skybox;c.enabled=enabled;g.AddComponent<UniversalAdditionalCameraData>();return c;}
    static void Preview(Camera camera,string name,Vector3 pos,Vector3 target,float fov)
    {camera.transform.position=pos;camera.transform.LookAt(target);camera.fieldOfView=fov;var rt=new RenderTexture(1600,1000,24);rt.antiAliasing=4;camera.targetTexture=rt;foreach(var txt in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>())if(txt.font!=null)txt.font.RequestCharactersInTexture(txt.text,txt.fontSize,txt.fontStyle);Canvas.ForceUpdateCanvases();for(int i=0;i<4;i++)camera.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new UnityEngine.Rect(0,0,1600,1000),0,0);tex.Apply();Directory.CreateDirectory("Previews");File.WriteAllBytes("Previews/"+name+".png",tex.EncodeToPNG());RenderTexture.active=prev;camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
    static void Validate(Scene scene)
    {
        int scripts=0,missing=0,materials=0,renderers=0;
        foreach(var root in scene.GetRootGameObjects())foreach(var tr in root.GetComponentsInChildren<Transform>(true)){
            missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(tr.gameObject);
            foreach(var mb in tr.GetComponents<MonoBehaviour>())if(mb!=null&&mb.GetType().Assembly.GetName().Name=="Assembly-CSharp")scripts++;
            foreach(var rr in tr.GetComponents<Renderer>()){renderers++;foreach(var m in rr.sharedMaterials)if(m==null||m.shader==null||m.shader.name=="Hidden/InternalErrorShader")materials++;}
        }
        if(missing>0||materials>0||scripts>0)throw new Exception("Scene validation failed: missing="+missing+" badMaterials="+materials+" runtimeScripts="+scripts);
        if(room.Find("Workstations_Grid_1_3_7_9").childCount!=4)throw new Exception("Wrong workstation count");
        Directory.CreateDirectory("Previews");File.WriteAllText("Previews/validation.txt","Scene: "+scene.name+"\nWorkstations: 4\nPods: 2 (outward entry)\nRoom: 15 x 15 x 15 m\nNature ground: 15 x 15 m\nMissing scripts: "+missing+"\nInvalid materials: "+materials+"\nCustom runtime behaviours: "+scripts+"\nRenderers: "+renderers+"\nStatic visual UI only; no XR rig, motion or gameplay implemented.\nPanel textures are project-generated placeholders, not course-supplied tile maps.\n");
    }

    // Reopen the serialized asset to verify references survive an editor restart.
    public static void ValidateSaved()
    {
        ShaderUtil.allowAsyncCompilation=false;
        var scene=EditorSceneManager.OpenScene(A+"/Scenes/TheRoom.unity");
        room=GameObject.Find("01_Classroom_15m").transform;
        nature=GameObject.Find("02_Nature_15m").transform;
        Validate(scene);
        var stations=room.Find("Workstations_Grid_1_3_7_9");
        foreach(Transform pc in stations)
        {
            var towardCenter=-pc.localPosition.normalized;
            if(Vector3.Dot(-pc.forward,towardCenter)<.99f)throw new Exception("Workstation not facing center: "+pc.name);
        }
        if(Vector3.Dot(-room.Find("VR_Pod_Grid4_Entry_West").forward,Vector3.left)<.99f)throw new Exception("VR entry direction");
        if(Vector3.Dot(-room.Find("AR_Pod_Grid6_Entry_East").forward,Vector3.right)<.99f)throw new Exception("AR entry direction");
        var camera=GameObject.Find("Main Camera").GetComponent<Camera>();
        Preview(camera,"Lighting_UI_Closeup",new Vector3(0,1.72f,-1.4f),new Vector3(0,1.13f,0),50);
        Preview(camera,"Workstation_Closeup",new Vector3(-2.4f,1.65f,2.4f),new Vector3(-4,1.1f,4),58);
        Preview(camera,"Nature_UI_Closeup",new Vector3(40.7f,1.7f,-5.6f),new Vector3(43,1.13f,-3.2f),48);
        Preview(camera,"Classroom_Player",new Vector3(0,2.5f,-6.7f),new Vector3(0,1.2f,1),80);
        File.AppendAllText("Previews/validation.txt","Saved scene reopened successfully.\nAll four workstation fronts face grid center.\nBoth pod openings face outward.\n");
        Debug.Log("THEROOM_REOPEN_VALIDATED");
    }
}










