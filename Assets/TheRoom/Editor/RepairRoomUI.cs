using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Authoring repair only. Buttons intentionally have no gameplay listeners.
public static class RepairRoomUI
{
    const string Request = "Temp/room-pod-repair.request";
    [InitializeOnLoadMethod]
    static void OnReload()
    {
        if (File.Exists(Request)) EditorApplication.delayCall += ApplyPending;
    }
    static void ApplyPending()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += ApplyPending; return; }
        if (!File.Exists(Request)) return;
        File.Move(Request, Request + ".running");
        try { Repair(); File.Move(Request + ".running", Request + ".done"); }
        catch (Exception e) { Debug.LogException(e); File.WriteAllText("Previews/ui-repair-error.txt", e.ToString()); }
    }

    [MenuItem("TheRoom/Repair UI Button components")]
    public static void Repair()
    {
        var scene = SceneManager.GetSceneByPath("Assets/TheRoom/Scenes/TheRoom.unity");
        if (!scene.IsValid() || !scene.isLoaded) scene = EditorSceneManager.OpenScene("Assets/TheRoom/Scenes/TheRoom.unity", OpenSceneMode.Additive);
        Directory.CreateDirectory("Previews");
        string backup = "Previews/TheRoom.before-ui-repair." + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity";
        File.Copy(scene.path, backup);
        foreach (var path in new[] {"Assets/TheRoom/Prefabs/Lighting_Console.prefab", "Assets/TheRoom/Prefabs/Nature_Console.prefab", "Assets/TheRoom/Prefabs/Workstation.prefab", "Assets/TheRoom/Prefabs/VR_Pod.prefab", "Assets/TheRoom/Prefabs/AR_Pod.prefab"})
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try { ConfigureRoot(root, null); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        ConfigureScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Validate(scene);
        Debug.Log("THEROOM_UI_REPAIR_COMPLETE: 10 real Buttons including two unified equipment buttons; OnClick left unbound.");
    }

    public static void ConfigureScene(Scene scene)
    {
        Camera camera = null;
        foreach (var root in scene.GetRootGameObjects())
            foreach (var c in root.GetComponentsInChildren<Camera>(true)) if (c.CompareTag("MainCamera")) camera = c;
        EventSystem events = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            ConfigureRoot(root, camera);
            var es = root.GetComponentInChildren<EventSystem>(true);
            if (es != null) events = es;
        }
        if (events == null)
        {
            var obj = new GameObject("UI EventSystem", typeof(EventSystem));
            SceneManager.MoveGameObjectToScene(obj, scene);
            events = obj.GetComponent<EventSystem>();
        }
        if (events.GetComponent<BaseInputModule>() == null)
        {
            var input = events.gameObject.AddComponent<InputSystemUIInputModule>();
            input.AssignDefaultActions();
        }
    }

    static void ConfigureRoot(GameObject root, Camera camera)
    {
        foreach(var tr in root.GetComponentsInChildren<Transform>(true))
            if(tr.name == "Equipment_Cluster_SINGLE_FUTURE_HOTSPOT") ConfigureEquipment(tr);
        foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
        {
            bool computer = canvas.name.StartsWith("Screen_Only_");
            bool exit = canvas.name.StartsWith("Exit_Flat_");
            bool console = canvas.name == "Flat_2D_UI_VISUAL_ONLY" || canvas.name == "ControlPanelCanvas";
            bool equipment = canvas.name == "EquipmentCanvas";
            if (!computer && !exit && !console && !equipment) continue;
            if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.gameObject.AddComponent<GraphicRaycaster>();
            if (camera != null) canvas.worldCamera = camera;
            if (equipment)
            {
                MakeButton(canvas.transform.Find("EquipmentButton").gameObject,"EquipmentButton");
            }
            else if (computer || exit)
            {
                var image = canvas.transform.Find("Background") ?? canvas.transform.Find(computer ? "ComputerScreenButton" : "ExitButton");
                MakeButton(image.gameObject, computer ? "ComputerScreenButton" : "ExitButton");
            }
            else
            {
                bool nature = canvas.transform.Find("Return_Button_Visual") != null || canvas.transform.Find("ReturnToLabButton") != null;
                var first = canvas.transform.Find("Flat_Button_Visual") ?? canvas.transform.Find(nature ? "ShootingToggleButton" : "LightColorButton");
                MakeButton(first.gameObject, nature ? "ShootingToggleButton" : "LightColorButton");
                var back = canvas.transform.Find("Return_Button_Visual") ?? canvas.transform.Find("ReturnToLabButton");
                if (back != null) MakeButton(back.gameObject, "ReturnToLabButton");
                canvas.name = "ControlPanelCanvas";
            }
            foreach (var text in canvas.GetComponentsInChildren<Text>(true)) text.raycastTarget = false;
        }
    }

    // One actual uGUI Button owns the three 3D models and the square hit area.
    // Canvas scale is 1: dimensions are meters and the models keep their world transforms.
    static void ConfigureEquipment(Transform dock)
    {
        if(dock.Find("EquipmentCanvas")!=null) return;
        var parts=new List<Transform>();
        foreach(Transform child in dock)
            if(child.name=="Headset_Decoration" || child.name=="Controller_Decoration") parts.Add(child);
        if(parts.Count!=3) throw new Exception("Expected headset and two controllers in " + dock.name);
        var canvasObject=new GameObject("EquipmentCanvas",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(dock,false);
        canvasObject.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
        canvasObject.GetComponent<RectTransform>().sizeDelta=new Vector2(.85f,.85f);
        var buttonObject=new GameObject("EquipmentButton",typeof(RectTransform),typeof(Image));
        buttonObject.transform.SetParent(canvasObject.transform,false);
        var rect=buttonObject.GetComponent<RectTransform>();
        rect.localPosition=new Vector3(0,1.08f,-.18f);
        rect.sizeDelta=new Vector2(.85f,.85f);
        buttonObject.GetComponent<Image>().color=new Color(.1f,.7f,.85f,.035f);
        MakeButton(buttonObject,"EquipmentButton");
        var button=buttonObject.GetComponent<Button>();
        // The normal panel remains transparent; hover/press tints the same square.
        var palette=button.colors;
        palette.normalColor=new Color(1,1,1,.25f);
        palette.highlightedColor=new Color(1,1,1,1);
        palette.selectedColor=palette.highlightedColor;
        palette.pressedColor=new Color(.6f,.9f,1,1);
        button.colors=palette;
        var models=new GameObject("EquipmentModels").transform;
        models.SetParent(rect,false);
        foreach(var part in parts)
        {
            part.name=part.name=="Headset_Decoration"?"Headset":part.localPosition.x<0?"LeftController":"RightController";
            part.SetParent(models,true);
        }
        for(int i=0;i<4;i++)
        {
            var edge=new GameObject("SquareBorder_"+i,typeof(RectTransform),typeof(Image));
            edge.transform.SetParent(rect,false);
            var r=edge.GetComponent<RectTransform>();
            r.sizeDelta=i<2?new Vector2(.85f,.005f):new Vector2(.005f,.85f);
            r.anchoredPosition=i<2?new Vector2(0,i==0?-.425f:.425f):new Vector2(i==2?-.425f:.425f,0);
            var img=edge.GetComponent<Image>();img.color=new Color(.1f,.7f,.85f,.45f);img.raycastTarget=false;
        }
    }

    static void MakeButton(GameObject obj, string name)
    {
        obj.name = name;
        var image = obj.GetComponent<Image>(); image.raycastTarget = true;
        var button = obj.GetComponent<Button>();
        if (button == null)
        {
            button = obj.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(.78f, .94f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(.5f, .72f, .84f);
            colors.fadeDuration = .08f;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
        }
        if (button.targetGraphic == null) button.targetGraphic = image;
    }

    static void Validate(Scene scene)
    {
        var lines = new List<string>(); int count = 0;
        foreach (var root in scene.GetRootGameObjects()) foreach (var b in root.GetComponentsInChildren<Button>(true))
        {
            count++;
            if (b.targetGraphic == null || !b.targetGraphic.raycastTarget || b.GetComponentInParent<GraphicRaycaster>() == null)
                throw new Exception("UI not raycastable: " + b.name);
            lines.Add(b.name + " | interactable=" + b.interactable + " | persistent listeners=" + b.onClick.GetPersistentEventCount());
        }
        if(count != 10) throw new Exception("Expected 10 Buttons, got " + count);
        foreach(var root in scene.GetRootGameObjects()) foreach(var c in root.GetComponentsInChildren<Canvas>(true))
        {
            if(c.name!="EquipmentCanvas")continue;
            var b=c.transform.Find("EquipmentButton");
            if(b.Find("EquipmentModels").childCount!=3 || c.GetComponentsInChildren<Button>(true).Length!=1)
                throw new Exception("Pod must have exactly three models and one Button");
            if(b.GetComponent<RectTransform>().sizeDelta!=new Vector2(.85f,.85f))throw new Exception("Pod button must be square");
        }
        var probe = new GameObject("Temporary_UI_Validation_Camera", typeof(Camera));
        var camera = probe.GetComponent<Camera>(); camera.enabled = false;
        var target = new RenderTexture(800,600,16); camera.targetTexture = target;
        try
        {
            foreach(var root in scene.GetRootGameObjects()) foreach(var button in root.GetComponentsInChildren<Button>(true))
            {
                var canvas = button.GetComponentInParent<Canvas>();
                var originalCamera = canvas.worldCamera;
                canvas.worldCamera = camera;
                try
                {
                    var rect = button.GetComponent<RectTransform>();
                    Vector3 center = rect.TransformPoint(rect.rect.center);
                    camera.transform.position = center - rect.forward * 1.5f;
                    camera.transform.rotation = Quaternion.LookRotation(rect.forward, rect.up);
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    var pointer = new PointerEventData(EventSystem.current);
                    pointer.position = camera.WorldToScreenPoint(center);
                    var hits = new List<RaycastResult>();
                    canvas.GetComponent<GraphicRaycaster>().Raycast(pointer,hits);
                    if(!hits.Exists(h => h.gameObject == button.gameObject)) throw new Exception("Raycast missed " + button.name);
                    // Test only the unbound buttons; never execute an existing user's gameplay callback.
                    if(button.onClick.GetPersistentEventCount()==0)
                    {
                        int clicks=0;
                        UnityEngine.Events.UnityAction callback = () => clicks++;
                        button.onClick.AddListener(callback);
                        try { ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler); }
                        finally { button.onClick.RemoveListener(callback); }
                        if(clicks!=1) throw new Exception("Pointer click did not dispatch once: " + button.name);
                    }
                    lines.Add(button.name + " | raycast hit and pointer-click dispatch PASS");
                }
                finally { canvas.worldCamera = originalCamera; }
            }
        }
        finally { camera.targetTexture=null; target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(probe); }
        File.WriteAllLines("Previews/ui-repair-validation.txt", lines);
    }
}

