using UnityEditor;
using UnityEngine;

// Hanya mengisi project baru bila scene Pertemuan 2 belum tersedia.
// Tidak mengubah pekerjaan pengguna yang sudah tersimpan.
[InitializeOnLoad]
public static class ARPraktikumAutoSetup
{
    static ARPraktikumAutoSetup()
    {
        EditorApplication.delayCall += GenerateOnFirstOpen;
    }

    static void GenerateOnFirstOpen()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += GenerateOnFirstOpen;
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(
                "Assets/Scenes/Pertemuan02_Diorama.unity") == null)
        {
            ARPraktikumBuilder.GenerateAll();
        }
    }
}
