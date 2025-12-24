using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using wataameya.marshmallow_PB;

// Copyright (c) 2023 wataameya

namespace wataameya.marshmallow_PB.editor
{
    [Serializable]
    public class CreateMarshmallowPreset : ScriptableObject
    {
        [MenuItem("Assets/Create/MarshmallowPreset", false)]
        static void Create()
        {
            string[] path_selection = Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.TopLevel)
                .Select(x => AssetDatabase.GetAssetPath(x)).Where(x => AssetDatabase.IsValidFolder(x)).ToArray();
            if(path_selection.Length==0) return;
            int count = Selection.GetFiltered<MarshmallowPreset>(SelectionMode.DeepAssets).Count();
            string path = path_selection[0] + "/" + count + ".asset";

            MarshmallowPreset preset = CreateInstance<MarshmallowPreset>();

            EditorUtility.SetDirty(preset);
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}