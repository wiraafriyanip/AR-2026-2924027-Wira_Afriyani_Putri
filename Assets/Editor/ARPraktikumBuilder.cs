using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Generator ini berjalan di UNITY EDITOR, tidak di Android.
// Menghasilkan scene .unity, material .mat dan prefab .prefab asli yang EDITABLE.
// Menu: AR Praktikum > Generate / Refresh Scene 02-05
public static class ARPraktikumBuilder
{
    const string Scenes = "Assets/Scenes/";
    const string Prefabs = "Assets/Prefabs/";
    const string Materials = "Assets/Materials/";

    static Material grass, path, wood, leaf, bark, blue, stone, yellow, red, dark, mint;
    static AudioClip ambience, chime, bump;
    static GameObject treePrefab, lampPrefab, obstaclePrefab;

    [MenuItem("AR Praktikum/Generate / Refresh Scene 02-05")]
    public static void GenerateAll()
    {
        // Selalu minta izin sebelum mengganti scene yang sudah diedit pengguna.
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Scenes + "Pertemuan02_Diorama.unity") != null)
        {
            if (!EditorUtility.DisplayDialog("Regenerasi Praktikum AR?",
                "Ini akan membuat ulang scene 02-05 dan prefab yang ada. " +
                "Perubahan manual pada scene tersebut bisa tertimpa. Lanjutkan?",
                "Ya, buat ulang", "Batal")) return;
        }
        try
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureFolders();
            EnsureTags();
            PrepareAssets();
            CreateReusablePrefabs();
            BuildDioramaScene(2);
            BuildDioramaScene(3);
            BuildDioramaScene(4);
            BuildInteractionScene();
            EditorBuildSettings.scenes = new [] {
                new EditorBuildSettingsScene(Scenes + "Pertemuan05_Interaction.unity", true)
            };
            PlayerSettings.companyName = "KelasAR";
            PlayerSettings.productName = "P07ARPraktikum";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(Scenes + "Pertemuan02_Diorama.unity");
            Debug.Log("[AR Praktikum] Scene P02-P05 dibuat. Mulai dari Pertemuan02_Diorama di Assets/Scenes. " +
                      "Build Profiles memakai P05_Interaction sebagai scene utama.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[AR Praktikum] Generator gagal: " + ex);
            throw;
        }
    }

    static void EnsureFolders()
    {
        foreach (string name in new[] {"Scenes", "Materials", "Prefabs", "Audio", "Scripts"})
            if (!AssetDatabase.IsValidFolder("Assets/" + name))
                AssetDatabase.CreateFolder("Assets", name);
    }

    static void EnsureTags()
    {
        AddTagIfMissing("Target");
        AddTagIfMissing("Obstacle");
    }

    static void AddTagIfMissing(string name)
    {
        foreach (string t in InternalEditorUtility.tags) if (t == name) return;
        UnityEngine.Object tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        SerializedObject so = new SerializedObject(tagManager);
        SerializedProperty tags = so.FindProperty("tags");
        int i = tags.arraySize;
        tags.InsertArrayElementAtIndex(i);
        tags.GetArrayElementAtIndex(i).stringValue = name;
        so.ApplyModifiedProperties();
    }

    static Color Col(string html)
    {
        Color c;
        if (!ColorUtility.TryParseHtmlString(html, out c)) return Color.white;
        return c;
    }

    static Material MakeMat(string name, string color)
    {
        string pathName = Materials + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(pathName);
        Shader shader;
        if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        else shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        if (m == null)
        {
            if (shader == null) throw new Exception("Shader dasar Unity tidak tersedia.");
            m = new Material(shader);
            AssetDatabase.CreateAsset(m, pathName);
        }
        Color tint = Col(color);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", tint);
        if (m.HasProperty("_Color")) m.SetColor("_Color", tint);
        EditorUtility.SetDirty(m);
        return m;
    }

    static void PrepareAssets()
    {
        grass = MakeMat("Mat_Floor", "#82BB83");
        path = MakeMat("Mat_Path", "#E3D8BF");
        wood = MakeMat("Mat_Table", "#B2784B");
        leaf = MakeMat("Mat_Leaf", "#3A906B");
        bark = MakeMat("Mat_Bark", "#715949");
        blue = MakeMat("Mat_Player", "#4388D1");
        stone = MakeMat("Mat_Wall", "#8497A7");
        yellow = MakeMat("Mat_Target", "#F4C95D");
        red = MakeMat("Mat_Obstacle", "#D46B65");
        dark = MakeMat("Mat_DeepNavy", "#20394A");
        mint = MakeMat("Mat_Accent", "#9CDAC8");
        AssetDatabase.ImportAsset("Assets/Audio/ambience_taman.wav");
        AssetDatabase.ImportAsset("Assets/Audio/sfx_target.wav");
        AssetDatabase.ImportAsset("Assets/Audio/sfx_obstacle.wav");
        ambience = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ambience_taman.wav");
        chime = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_target.wav");
        bump = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_obstacle.wav");
    }

    static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 scale, Material m)
    { return Primitive(PrimitiveType.Cube, name, parent, pos, scale, m); }

    static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material m)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = scale;
        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null) rend.sharedMaterial = m;
        return go;
    }

    static GameObject Empty(string name, Transform parent = null, Vector3? pos = null)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos ?? Vector3.zero;
        return go;
    }

    static void CreateReusablePrefabs()
    {
        GameObject srcTree = Empty("Tree_Prefab");
        Primitive(PrimitiveType.Cylinder, "Trunk", srcTree.transform, new Vector3(0f, 0.75f, 0f), new Vector3(.25f,.75f,.25f), bark);
        Primitive(PrimitiveType.Sphere, "Crown_Lower", srcTree.transform, new Vector3(0f, 1.85f, 0f), new Vector3(1.7f,1.35f,1.7f), leaf);
        Primitive(PrimitiveType.Sphere, "Crown_Upper", srcTree.transform, new Vector3(0f, 2.5f, 0f), new Vector3(1.35f,1.1f,1.35f), leaf);
        treePrefab = PrefabUtility.SaveAsPrefabAsset(srcTree, Prefabs+"Tree_Prefab.prefab");
        UnityEngine.Object.DestroyImmediate(srcTree);

        GameObject srcLamp = Empty("Lamp_Prefab");
        Primitive(PrimitiveType.Cylinder, "Pillar_01", srcLamp.transform, new Vector3(0f, 1.25f, 0f), new Vector3(.19f,1.25f,.19f), dark);
        Primitive(PrimitiveType.Sphere, "Lamp_01", srcLamp.transform, new Vector3(0f, 2.55f, 0f), new Vector3(.65f,.65f,.65f), yellow);
        lampPrefab = PrefabUtility.SaveAsPrefabAsset(srcLamp, Prefabs+"Lamp_Prefab.prefab");
        UnityEngine.Object.DestroyImmediate(srcLamp);

        GameObject srcObs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        srcObs.name = "Obstacle_Prefab";
        srcObs.tag = "Obstacle";
        srcObs.GetComponent<Renderer>().sharedMaterial = red;
        obstaclePrefab = PrefabUtility.SaveAsPrefabAsset(srcObs, Prefabs+"Obstacle_Prefab.prefab");
        UnityEngine.Object.DestroyImmediate(srcObs);
    }

    static GameObject MakePrefabInstance(GameObject source, string name, Transform parent, Vector3 worldPos)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = name;
        if (parent != null) instance.transform.SetParent(parent, true);
        instance.transform.position = worldPos;
        return instance;
    }

    static Scene NewScene(string sceneName)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        Camera cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Col("#D6EAF1");
        cam.fieldOfView = 58f;
        camGo.transform.position = new Vector3(12f, 12f, -16f);
        camGo.transform.LookAt(new Vector3(0f,0.7f,0f));
        GameObject lightGo = new GameObject("Directional Light", typeof(Light));
        Light light = lightGo.GetComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1.15f;
        lightGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        return scene;
    }

    static void BuildDioramaScene(int meeting)
    {
        string sceneName = meeting == 2 ? "Pertemuan02_Diorama" :
                           meeting == 3 ? "Pertemuan03_CameraAudio" : "Pertemuan04_CSharp";
        Scene scene = NewScene(sceneName);
        GameObject env = Empty("Environment");
        GameObject props = Empty("Props");
        Primitive(PrimitiveType.Plane, "Ground", env.transform, Vector3.zero, new Vector3(2f,1f,2f), grass);
        Cube("Walkway_Central", env.transform, new Vector3(0f,.06f,0f), new Vector3(2.2f,.12f,18f), path);
        Cube("Wall_Main", env.transform, new Vector3(0f,.5f,9f), new Vector3(16f,1f,.6f), stone);
        GameObject table = Empty("Table_01", props.transform, new Vector3(-3f, 0f, .3f));
        Cube("Table_Top", table.transform, new Vector3(0f,.85f,0f), new Vector3(2.1f,.23f,1.1f), wood);
        Cube("Table_Leg_L", table.transform, new Vector3(-.72f,.4f,0f), new Vector3(.19f,.7f,.2f), bark);
        Cube("Table_Leg_R", table.transform, new Vector3(.72f,.4f,0f), new Vector3(.19f,.7f,.2f), bark);
        Vector3[] trees = { new Vector3(-7,0,-6), new Vector3(7,0,-6), new Vector3(-7,0,5), new Vector3(7,0,5) };
        for (int i=0;i<trees.Length;i++)
            MakePrefabInstance(treePrefab,"Tree_"+(i+1).ToString("00"),props.transform,trees[i]);
        GameObject lamp1 = MakePrefabInstance(lampPrefab,"Lamp_01_Instance",props.transform,new Vector3(-5,0,-1));
        GameObject lamp2 = MakePrefabInstance(lampPrefab,"Lamp_02_Instance",props.transform,new Vector3(5,0,-1));
        Primitive(PrimitiveType.Capsule, "Character_Test", props.transform, new Vector3(3.1f,1f,2f), new Vector3(.7f,1f,.7f), blue);
        GameObject ball = Primitive(PrimitiveType.Sphere, "Sphere_Physics_Test", props.transform,
                                   new Vector3(4.1f,3f,4f), new Vector3(.8f,.8f,.8f), yellow);
        Rigidbody falling = ball.AddComponent<Rigidbody>();
        falling.useGravity = true;

        if (meeting >= 3)
        {
            GameObject camGo = GameObject.Find("Main Camera");
            camGo.transform.position = new Vector3(0f,8f,-13f);
            camGo.transform.LookAt(new Vector3(0f,1.2f,0f));
            camGo.GetComponent<Camera>().fieldOfView = 65f;
            camGo.AddComponent<AudioCameraExplorer>().movementSpeed = 5f;
            GameObject manager = Empty("AudioManager");
            AudioSource ambient = manager.AddComponent<AudioSource>();
            ambient.clip = ambience;
            ambient.playOnAwake = true;
            ambient.loop = true;
            ambient.spatialBlend = 0f;
            ambient.volume = .18f;
            AudioSource s1 = lamp1.AddComponent<AudioSource>();
            s1.clip = chime; s1.playOnAwake = true; s1.loop = true;
            s1.spatialBlend = 1f; s1.minDistance = 1f; s1.maxDistance = 17f;
            s1.volume = .45f;
            AudioSource s2 = lamp2.AddComponent<AudioSource>();
            s2.clip = bump; s2.playOnAwake = true; s2.loop = true;
            s2.spatialBlend = 1f; s2.minDistance = 1f; s2.maxDistance = 16f;
            s2.volume = .30f;
        }
        if (meeting == 4)
        {
            GameObject rot = Primitive(PrimitiveType.Cube,"Spinner_01", props.transform,
                new Vector3(-3.0f,2.2f,3.2f),new Vector3(.85f,.85f,.85f),blue);
            rot.AddComponent<RotateObject>().speed = 50f;
            GameObject mover = Primitive(PrimitiveType.Sphere,"Mover_01", props.transform,
                new Vector3(0f,1.2f,3.2f),new Vector3(.8f,.8f,.8f),red);
            // Jangan diberi Rigidbody; contoh gerak Translate sesuai modul Pertemuan 4.
            var mc = mover.AddComponent<MoveObject>(); mc.speed=2f; mc.direction=1f;
            GameObject floater = Primitive(PrimitiveType.Capsule,"Floater_01", props.transform,
                new Vector3(3f,1.8f,3.2f),new Vector3(.65f,.65f,.65f),mint);
            var fc = floater.AddComponent<FloatObject>(); fc.amplitude=.55f; fc.speed=2f;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,Scenes+sceneName+".unity");
    }

    static void BuildInteractionScene()
    {
        Scene scene=NewScene("Pertemuan05_Interaction");
        Camera cam=GameObject.Find("Main Camera").GetComponent<Camera>();
        cam.orthographic=true; cam.orthographicSize=11.8f;
        cam.transform.position=new Vector3(0f,18f,-10f);
        cam.transform.LookAt(new Vector3(0f,0f,0f));
        GameObject arena=Empty("Arena");
        GameObject actors=Empty("Gameplay");
        Primitive(PrimitiveType.Plane,"Floor",arena.transform,Vector3.zero,new Vector3(2f,1f,2f),grass);
        Cube("Lane",arena.transform,new Vector3(0f,.055f,0f),new Vector3(8f,.1f,18f),path);
        for (int i=-1;i<=1;i++)
        {
            Cube("GuideStripe_"+(i+2),arena.transform,new Vector3(i*2.55f,.115f,-.7f),
                 new Vector3(.13f,.015f,13f),mint);
        }
        GameObject player=Primitive(PrimitiveType.Cube,"Player",actors.transform,
             new Vector3(0f,.56f,-7.5f),new Vector3(.9f,1.1f,.9f),blue);
        Rigidbody rb=player.AddComponent<Rigidbody>();
        rb.useGravity=true; rb.isKinematic=false;
        rb.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
        PlayerMovement movement=player.AddComponent<PlayerMovement>();
        movement.moveSpeed=3f;
        AudioSource feedback=player.AddComponent<AudioSource>();
        feedback.spatialBlend=0f; feedback.volume=.55f; feedback.playOnAwake=false;
        PlayerInteraction interaction=player.AddComponent<PlayerInteraction>();
        interaction.targetSound=chime; interaction.obstacleSound=bump;
        Vector3[] obstacles = {
            new Vector3(-3f,.55f,-3f), new Vector3(3f,.55f,-2f),
            new Vector3(-2f,.55f,1f), new Vector3(2f,.55f,3f),
            new Vector3(0f,.55f,0f)
        };
        for(int i=0;i<obstacles.Length;i++)
        {
            GameObject obs=MakePrefabInstance(obstaclePrefab,"Obstacle_"+(i+1).ToString("00"),actors.transform,obstacles[i]);
            obs.transform.localScale=new Vector3(1.45f,1.1f,1.45f);
        }
        Primitive(PrimitiveType.Cylinder,"Target_Base",arena.transform,
            new Vector3(0f,.07f,7f),new Vector3(1.4f,.07f,1.4f),dark);
        GameObject target=Primitive(PrimitiveType.Sphere,"Target",actors.transform,
             new Vector3(0f,.83f,7f),new Vector3(1.2f,1.2f,1.2f),yellow);
        target.tag="Target";
        SphereCollider targetCol=target.GetComponent<SphereCollider>();
        targetCol.isTrigger=true;
        GameObject audio=Empty("AudioManager");
        AudioSource ambient=audio.AddComponent<AudioSource>();
        ambient.clip=ambience; ambient.playOnAwake=true;
        ambient.loop=true; ambient.spatialBlend=0f; ambient.volume=.12f;
        GameHUD hud=CreateHud(movement);
        interaction.hud=hud;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene,Scenes+"Pertemuan05_Interaction.unity");
    }

    static Font GetFont()
    {
        Font f=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return f;
    }

    static GameObject UiObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name,typeof(RectTransform));
        go.transform.SetParent(parent,false);
        return go;
    }

    static void Anchors(RectTransform rt,Vector2 min,Vector2 max,Vector2 pivot,Vector2 position,Vector2 size)
    {
        rt.anchorMin=min; rt.anchorMax=max; rt.pivot=pivot;
        rt.anchoredPosition=position; rt.sizeDelta=size;
    }

    static Text TextUi(string name, Transform parent,string value,int fontSize,Color color,
                       Vector2 aMin,Vector2 aMax,Vector2 pivot,Vector2 pos,Vector2 size,TextAnchor align)
    {
        GameObject go=UiObject(name,parent);
        RectTransform rt=go.GetComponent<RectTransform>();
        Anchors(rt,aMin,aMax,pivot,pos,size);
        Text txt=go.AddComponent<Text>();
        txt.font=GetFont(); txt.fontSize=fontSize; txt.text=value;
        txt.color=color; txt.alignment=align;
        txt.raycastTarget=false; txt.horizontalOverflow=HorizontalWrapMode.Wrap;
        txt.verticalOverflow=VerticalWrapMode.Truncate;
        return txt;
    }

    static GameObject Panel(string name, Transform parent,Color col,
                            Vector2 min,Vector2 max,Vector2 pivot,Vector2 pos,Vector2 size)
    {
        GameObject go=UiObject(name,parent);
        Anchors(go.GetComponent<RectTransform>(),min,max,pivot,pos,size);
        Image im=go.AddComponent<Image>(); im.color=col;
        return go;
    }

    static GameHUD CreateHud(PlayerMovement player)
    {
        GameObject cg=new GameObject("Canvas_HUD",typeof(RectTransform),typeof(Canvas),
            typeof(CanvasScaler),typeof(GraphicRaycaster));
        Canvas canvas=cg.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=cg.GetComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1920f,1080f);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=.5f;
        Panel("Header",cg.transform,new Color(.09f,.18f,.24f,.85f),
            new Vector2(0f,1f),new Vector2(1f,1f),new Vector2(.5f,1f),Vector2.zero,new Vector2(0f,106f));
        TextUi("Title",cg.transform,"AR PRAKTIKUM  |  CARI TARGET",38,Color.white,
            new Vector2(0f,1f),new Vector2(0f,1f),new Vector2(0f,1f),new Vector2(35f,-18f),
            new Vector2(850f,66f),TextAnchor.MiddleLeft);
        TextUi("Instructions",cg.transform,"Keyboard: W A S D / panah     |     Android: tombol layar",
            27,Col("#D5ECF3"),new Vector2(0f,1f),new Vector2(0f,1f),new Vector2(0f,1f),
            new Vector2(38f,-117f),new Vector2(950f,55f),TextAnchor.MiddleLeft);
        Text status=TextUi("Status",cg.transform,"Arahkan player biru ke target kuning. Hindari balok merah.",
            28,Col("#243C4C"),new Vector2(.5f,0f),new Vector2(.5f,0f),
            new Vector2(.5f,0f),new Vector2(0f,24f),new Vector2(1300f,66f),TextAnchor.MiddleCenter);
        GameObject pad=UiObject("Touch_DPad",cg.transform);
        Anchors(pad.GetComponent<RectTransform>(),new Vector2(0f,0f),new Vector2(0f,0f),
            new Vector2(0f,0f),new Vector2(210f,195f),new Vector2(0f,0f));
        DpadKey(pad.transform,"Forward_W","W",new Vector2(0f,102f),Vector2.up,player);
        DpadKey(pad.transform,"Left_A","A",new Vector2(-104f,0f),Vector2.left,player);
        DpadKey(pad.transform,"Right_D","D",new Vector2(104f,0f),Vector2.right,player);
        DpadKey(pad.transform,"Back_S","S",new Vector2(0f,-102f),Vector2.down,player);

        GameObject rgo=Panel("Btn_Restart",cg.transform,Col("#255D72"),
              new Vector2(1f,1f),new Vector2(1f,1f),new Vector2(1f,1f),
              new Vector2(-35f,-18f),new Vector2(205f,66f));
        Button btn=rgo.AddComponent<Button>();
        btn.targetGraphic=rgo.GetComponent<Image>();
        TextUi("Label",rgo.transform,"ULANG",28,Color.white,
            Vector2.zero,Vector2.one,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero,TextAnchor.MiddleCenter);
        GameHUD hud=cg.AddComponent<GameHUD>(); hud.statusText=status; hud.restartButton=btn;
        GameObject eventSystem=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        return hud;
    }

    static void DpadKey(Transform parent,string name,string glyph,Vector2 xy,Vector2 vec,PlayerMovement p)
    {
        GameObject go=Panel(name,parent,new Color(.10f,.31f,.40f,.87f),
              new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(.5f,.5f),xy,new Vector2(92f,92f));
        MobileDirectionButton directional=go.AddComponent<MobileDirectionButton>();
        directional.player=p; directional.direction=vec;
        TextUi("Letter",go.transform,glyph,42,Color.white,Vector2.zero,Vector2.one,
               new Vector2(.5f,.5f),Vector2.zero,Vector2.zero,TextAnchor.MiddleCenter);
    }
}
