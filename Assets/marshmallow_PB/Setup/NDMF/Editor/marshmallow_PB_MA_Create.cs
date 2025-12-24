using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using VRC.SDK3.Avatars.Components;

namespace wataameya.marshmallow_PB.ndmf.editor
{
    public sealed class marshmallow_PB_MA_Create : EditorWindow
    {
        private const string _MenuPath = "GameObject/wataameya/MarshmallowPB";
        private const int ContextMenuPriority = 25;


        [MenuItem(_MenuPath, true, ContextMenuPriority)]
        public static bool ValidateApplytoAvatar() => Selection.gameObjects.Any(ValidateCore);

        [MenuItem(_MenuPath, false, ContextMenuPriority)]
        public static void ApplytoAvatar()
        {
            List<GameObject> objectToCreated = new List<GameObject>();
            foreach (var x in Selection.gameObjects)
            {
                if (!ValidateCore(x))
                    continue;

                var prefab = GeneratePrefab(x.transform);

                objectToCreated.Add(prefab);
            }
            if (objectToCreated.Count == 0)
                return;

            EditorGUIUtility.PingObject(objectToCreated[0]);
            Selection.objects = objectToCreated.ToArray();
        }
        private static bool ValidateCore(GameObject obj) => obj != null && obj.GetComponent<VRCAvatarDescriptor>() != null && obj.GetComponentInChildren<marshmallow_PB_MA>() == null;
        private static GameObject GeneratePrefab(Transform parent = null)
        {
            const string PrefabGUID = "de8d01d793cb1ec4facf5b6c151ae0d3";
            var prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(PrefabGUID));
            var prefab = PrefabUtility.InstantiatePrefab(prefabObj, parent) as GameObject;
            Undo.RegisterCreatedObjectUndo(prefab, "Apply MarshmallowPB");

            var marshmallow = prefab.GetComponent<marshmallow_PB_MA>();
            marshmallow._version = "2.0";

            return prefab;
        }
    }
}