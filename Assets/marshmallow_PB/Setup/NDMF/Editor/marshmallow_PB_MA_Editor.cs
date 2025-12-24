using UnityEngine;
using VRC.SDKBase;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.Dynamics;
using wataameya.marshmallow_PB.ndmf;


namespace wataameya.marshmallow_PB.ndmf.editor
{
    [CustomEditor(typeof(marshmallow_PB_MA))]
    internal sealed class marshmallow_PB_MA_Editor : Editor
    {
        private static readonly string _AvatarPreset_GUID = "29c1bc0f7153d444398aaa9dd003d315";
        private static readonly string _PhysbonePreset_GUID = "18dede178f315874a956b0e70c5deb50";
        private static readonly string _LocalizeCSV_GUID = "3b4c0c8ca80cf444e8adecbbfac1ec96";
        private bool _initialized1 = false;
        private bool _initialized2 = false;
        private bool _reinitializing = false;
        private List<Dictionary<string, string>> _texts = new List<Dictionary<string, string>>();
        private List<int> _preset_indexs;
        private List<string> _preset_names;
        private List<int> _pbpreset_indexs;
        private List<string> _pbpreset_names;
        private string pbinputstr = "";

        private string[] _immobile_type_keys =
        {
            "_Immobile_type_AllMotion",
            "_Immobile_type_World",
        };

        private string[] _allow_type_keys =
        {
            "_Allow_None",
            "_Allow_Self_Others",
            "_Allow_Self",
            "_Allow_Others",
        };

        public marshmallow_PB_MA marshmallow;

        // MARK:GUI
        public override void OnInspectorGUI()
        {
            marshmallow = target as marshmallow_PB_MA;
            serializedObject.Update();
            Undo.RecordObject(marshmallow, "marshmallow");

            // if (marshmallow._lang_number != 100) marshmallow._version = "1.0"; // ver2.0以前のバージョン検知用

            if (_reinitializing)
            {
                Debug.Log("marshmallowPB is initialized!");
                var temp_lang = marshmallow._lang;
                var temp_index = marshmallow._index;
                var temp_breast_blendshape = marshmallow._breast_blendshape;
                var temp_Breast_L = marshmallow._Breast_L;
                var temp_Breast_R = marshmallow._Breast_R;
                var temp_breast_scale = marshmallow._breast_scale;

                Unsupported.SmartReset(marshmallow);
                marshmallow._lang = temp_lang;
                marshmallow._index = temp_index;
                marshmallow._breast_blendshape = temp_breast_blendshape;
                marshmallow._Breast_L = temp_Breast_L;
                marshmallow._Breast_R = temp_Breast_R;
                marshmallow._breast_scale = temp_breast_scale;

                _reinitializing = false;
                // marshmallow._lang_number = 100;
                marshmallow._version = "2.0";
            }

            if (_texts.Count == 0 || marshmallow._presets.Length == 0)
            {
                _initialized1 = false;
                _initialized2 = false;
            }

            if (!_initialized1)
            {
                InitializeAvatarPreset();
                InitializePhysbonePreset();
                _initialized1 = true;
            }

            if (!_initialized2)
            {
                _texts = Localize();
                _initialized2 = true;
            }


            float viewWidth = EditorGUIUtility.currentViewWidth;

            float prevLabelWidth = EditorGUIUtility.labelWidth;
            float prevFieldWidth = EditorGUIUtility.fieldWidth;

            // ------------ 幅を好きな割合に設定 ------------
            // 例：ラベル30％、数値入力15％、残り(55％)がスライダー
            EditorGUIUtility.labelWidth = viewWidth * 0.45f;
            EditorGUIUtility.fieldWidth = viewWidth * 0.10f;

            marshmallow._lang = EditorGUILayout.Popup("Language", (int)marshmallow._lang, new[] { 0, 1, 2, 3 }.Select(k => _texts[k]["_Languages"]).ToArray());
            Dictionary<string, string> localized_texts = _texts[marshmallow._lang];

            ///メニュー
            GUIStyle textStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                richText = true
            };

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                richText = true,
                alignment = TextAnchor.MiddleLeft
            };

            var boldBig = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 32
            };


#if !USE_NDMF
                EditorGUILayout.LabelField("<color=red><size=25>"+localized_texts["_Error_PackageNotImported"]+"</size></color>",textStyle);
#endif

            EditorGUILayout.Space(5);

            DrawWebButton(localized_texts["_Link_ManualTitle"], localized_texts["_Link_ManualURL"]);
            DrawWebButton("綿飴屋 Wataameya BOOTH", "https://wataame89.booth.pm/items/4511536");

            // EditorGUILayout.LabelField("<color=orange><size=15>" + "β版につき、必ずアバターのバックアップを取ってからご試用下さい。 Please make a backup before trying the beta version." + "</size></color>", textStyle);
            EditorGUILayout.LabelField("<color=orange><size=15>" + localized_texts["_Message_Make_Backup"] + "</size></color>", textStyle);
            // EditorGUILayout.Space(5);
            // EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // EditorGUILayout.LabelField("↓ぜひましゅまろPBのご意見・ご感想をお聞かせください！ Feedback please!", textStyle);

            // if (GUILayout.Button("X(Twitter)でポスト(ツイート)する！ Post (Tweet)!", boldBig))
            // {
            //     Application.OpenURL("https://twitter.com/intent/tweet?text=%E3%80%90%E3%81%BE%E3%81%97%E3%82%85%E3%81%BE%E3%82%8DPB(ver2.0.0%20%CE%B2%E7%89%88)%E3%82%92%E5%85%88%E8%A1%8C%E4%BD%93%E9%A8%93%E4%B8%AD%EF%BC%81%E3%80%91%0A%0A%23%E3%81%BE%E3%81%97%E3%82%85%E3%81%BE%E3%82%8DPB%20%23%E3%81%BE%E3%81%97%E3%82%85%E3%81%BE%E3%82%8DPB_ver2");
            // }
            // if (GUILayout.Button("ご意見フォーム Feedback Form", boldBig))
            // {
            //     Application.OpenURL("https://forms.gle/1F8861tseN9Wy7eD9");
            // }


            if (!_preset_indexs.Contains(marshmallow._index))
            {
                EditorGUILayout.LabelField("<color=orange><size=15>" + localized_texts["_Warning_Invalid_Index"] + "</size></color>", textStyle);
                return;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


            EditorGUILayout.LabelField("<color=orange><size=15>" + localized_texts["_Warning_UpdatingToVer2.0.0"] + "</size></color>", textStyle);
            if (GUILayout.Button(localized_texts["_Label_ReInitializing"], boldBig))
            {
                _reinitializing = true;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


#if !USE_NDMF
                return;
#endif

            // MARK:Basic
            EditorGUILayout.LabelField(localized_texts["_Section_BasicSettings"], headerStyle);
            EditorGUILayout.LabelField(localized_texts["_Text_SetupTitle"], textStyle);

            marshmallow._avatar = marshmallow.transform.parent?.gameObject;

            EditorGUI.BeginChangeCheck();

            if (marshmallow._avatar != null && marshmallow._avatar.GetComponent<VRCAvatarDescriptor>())
            {
                EditorGUILayout.LabelField(localized_texts["_Message_SelectPreset"], textStyle);

                marshmallow._index = EditorGUILayout.IntPopup(localized_texts["_Label_Preset"], marshmallow._index, _preset_names.ToArray(), _preset_indexs.ToArray());

                marshmallow._Breast_L = (GameObject)EditorGUILayout.ObjectField(localized_texts["_Label_BreastBoneLeft"], marshmallow._Breast_L, typeof(GameObject), true);
                marshmallow._Breast_R = (GameObject)EditorGUILayout.ObjectField(localized_texts["_Label_BreastBoneRight"], marshmallow._Breast_R, typeof(GameObject), true);

                if (marshmallow._index != 0)
                {
                    marshmallow._breast_blendshape = EditorGUILayout.Slider(localized_texts["_Label_BreastSizeBlendshape"], marshmallow._breast_blendshape, 0f, 100f);
                    marshmallow._breast_scale = FloatFieldCheck(localized_texts["_Label_BreastBoneScaleY"], marshmallow._breast_scale, 0.1f, 10f);
                }
                else EditorGUILayout.Space(40);
                EditorGUILayout.Space(20);

                // MARK:PhysBone
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_PhysBoneSettings"], headerStyle);

                marshmallow._PhysBone_index = EditorGUILayout.IntPopup(localized_texts["_Label_PBPreset"], marshmallow._PhysBone_index, _pbpreset_names.ToArray(), _pbpreset_indexs.ToArray());

                marshmallow._PhysBone_Pull = EditorGUILayout.Slider(localized_texts["_PhysBone_Pull"], marshmallow._PhysBone_Pull, 0f, 1f);
                marshmallow._PhysBone_Momentum = EditorGUILayout.Slider(localized_texts["_PhysBone_Momentum"], marshmallow._PhysBone_Momentum, 0f, 1f);
                marshmallow._PhysBone_Stiffness = EditorGUILayout.Slider(localized_texts["_PhysBone_Stiffness"], marshmallow._PhysBone_Stiffness, 0f, 1f);
                marshmallow._PhysBone_Gravity = EditorGUILayout.Slider(localized_texts["_PhysBone_Gravity"], marshmallow._PhysBone_Gravity, 0f, 1f);
                marshmallow._PhysBone_GravityFalloff = EditorGUILayout.Slider(localized_texts["_PhysBone_GravityFalloff"], marshmallow._PhysBone_GravityFalloff, 0f, 1f);
                marshmallow._PhysBone_Immobile = EditorGUILayout.Slider(localized_texts["_PhysBone_Immobile"], marshmallow._PhysBone_Immobile, 0f, 1f);
                marshmallow._PhysBone_Immobile_type = EditorGUILayout.Popup(localized_texts["_PhysBone_Immobile_type"], marshmallow._PhysBone_Immobile_type, _immobile_type_keys.Select(k => localized_texts[k]).ToArray());
                marshmallow._PhysBone_Limit_Angle = EditorGUILayout.Slider(localized_texts["_PhysBone_Limit_Angle"], marshmallow._PhysBone_Limit_Angle, 0f, 180f);
                marshmallow._PhysBone_Limit_Rotation = EditorGUILayout.Vector3Field(localized_texts["_PhysBone_Limit_Rotation"], marshmallow._PhysBone_Limit_Rotation);
                marshmallow._PhysBone_Collision_Radius = EditorGUILayout.Slider(localized_texts["_PhysBone_Collision_Radius"], marshmallow._PhysBone_Collision_Radius, 0f, 0.1f);
                marshmallow._PhysBone_AllowCollision = EditorGUILayout.Popup(localized_texts["_PhysBone_AllowCollision"], marshmallow._PhysBone_AllowCollision, _allow_type_keys.Select(k => localized_texts[k]).ToArray());
                marshmallow._PhysBone_Stretch_Motion = EditorGUILayout.Slider(localized_texts["_PhysBone_Stretch_Motion"], marshmallow._PhysBone_Stretch_Motion, 0f, 1f);
                marshmallow._PhysBone_Max_Stretch = EditorGUILayout.Slider(localized_texts["_PhysBone_Max_Stretch"], marshmallow._PhysBone_Max_Stretch, 0f, 1f);
                marshmallow._PhysBone_Max_Squish = EditorGUILayout.Slider(localized_texts["_PhysBone_Max_Squish"], marshmallow._PhysBone_Max_Squish, 0f, 1f);
                marshmallow._PhysBone_AllowGrabbing = EditorGUILayout.Popup(localized_texts["_PhysBone_AllowGrabbing"], marshmallow._PhysBone_AllowGrabbing, _allow_type_keys.Select(k => localized_texts[k]).ToArray());
                marshmallow._PhysBone_AllowPosing = EditorGUILayout.Popup(localized_texts["_PhysBone_AllowPosing"], marshmallow._PhysBone_AllowPosing, _allow_type_keys.Select(k => localized_texts[k]).ToArray());
                marshmallow._PhysBone_Grab_Movement = EditorGUILayout.Slider(localized_texts["_PhysBone_Grab_Movement"], marshmallow._PhysBone_Grab_Movement, 0f, 1f);
                marshmallow._PhysBone_SnapToHand = EditorGUILayout.Toggle(localized_texts["_PhysBone_SnapToHand"], marshmallow._PhysBone_SnapToHand);

                marshmallow._isOpen_Collider = EditorGUILayout.Foldout(marshmallow._isOpen_Collider, localized_texts["_Label_Collider"]);
                if (marshmallow._isOpen_Collider)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        marshmallow._PhysBone_collider[i] = (VRCPhysBoneColliderBase)EditorGUILayout.ObjectField(localized_texts["_Label_Element"] + i, marshmallow._PhysBone_collider[i], typeof(VRCPhysBoneColliderBase), true);
                    }
                }

                EditorGUILayout.Space(20);


                // MARK:Inertia
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_InertiaSettings"], headerStyle);
                marshmallow._inertia_enabled = EditorGUILayout.Toggle(localized_texts["_inertia_enabled"], marshmallow._inertia_enabled);
                marshmallow._inertia_Pull = EditorGUILayout.Slider(localized_texts["_inertia_Pull"], marshmallow._inertia_Pull, 0f, 1f);
                marshmallow._inertia_Momentum = EditorGUILayout.Slider(localized_texts["_inertia_Momentum"], marshmallow._inertia_Momentum, 0f, 1f);
                marshmallow._inertia_Stiffness = EditorGUILayout.Slider(localized_texts["_inertia_Stiffness"], marshmallow._inertia_Stiffness, 0f, 1f);
                marshmallow._inertia_Gravity = EditorGUILayout.Slider(localized_texts["_inertia_Gravity"], marshmallow._inertia_Gravity, 0f, 1f);
                marshmallow._inertia_GravityFalloff = EditorGUILayout.Slider(localized_texts["_inertia_GravityFalloff"], marshmallow._inertia_GravityFalloff, 0f, 1f);
                marshmallow._inertia_Immobile = EditorGUILayout.Slider(localized_texts["_inertia_Immobile"], marshmallow._inertia_Immobile, 0f, 1f);
                marshmallow._inertia_Immobile_type = EditorGUILayout.Popup(localized_texts["_inertia_Immobile_type"], marshmallow._inertia_Immobile_type, _immobile_type_keys.Select(k => localized_texts[k]).ToArray());
                marshmallow._inertia_LimitAngle = EditorGUILayout.Slider(localized_texts["_inertia_LimitAngle"], marshmallow._inertia_LimitAngle, 0f, 180f);
                marshmallow._inertia_LimitRotation = EditorGUILayout.Vector3Field(localized_texts["_inertia_LimitRotation"], marshmallow._inertia_LimitRotation);
                marshmallow._inertia_StretchMotion = EditorGUILayout.Slider(localized_texts["_inertia_StretchMotion"], marshmallow._inertia_StretchMotion, 0f, 1f);
                marshmallow._inertia_MaxStretch = EditorGUILayout.Slider(localized_texts["_inertia_MaxStretch"], marshmallow._inertia_MaxStretch, 0f, 1f);
                marshmallow._inertia_MaxSquish = EditorGUILayout.Slider(localized_texts["_inertia_MaxSquish"], marshmallow._inertia_MaxSquish, 0f, 1f);

                EditorGUILayout.Space(20);

                // MARK:Parallel Bone
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_ParallelBoneFunction"], headerStyle);
                marshmallow._parallelBone_enabled = EditorGUILayout.Toggle(localized_texts["_parallelBone_enabled"], marshmallow._parallelBone_enabled);
                marshmallow._parallelBone_strengthX = EditorGUILayout.Slider(localized_texts["_parallelBone_strengthX"], marshmallow._parallelBone_strengthX, 0f, 1f);
                marshmallow._parallelBone_strengthY = EditorGUILayout.Slider(localized_texts["_parallelBone_strengthY"], marshmallow._parallelBone_strengthY, 0f, 1f);
                marshmallow._parallelBone_squishStrengthX = EditorGUILayout.Slider(localized_texts["_parallelBone_squishStrengthX"], marshmallow._parallelBone_squishStrengthX, 0f, 1f);
                marshmallow._parallelBone_squishStrengthY = EditorGUILayout.Slider(localized_texts["_parallelBone_squishStrengthY"], marshmallow._parallelBone_squishStrengthY, 0f, 1f);

                EditorGUILayout.Space(20);

                // MARK:Gravity
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_GravityFunction"], headerStyle);
                marshmallow._gravity_enabled = EditorGUILayout.Toggle(localized_texts["_gravity_enabled"], marshmallow._gravity_enabled);
                marshmallow._gravity_squish = EditorGUILayout.Slider(localized_texts["_gravity_squish"], marshmallow._gravity_squish, 0f, 1f);
                marshmallow._gravity_sag = EditorGUILayout.Slider(localized_texts["_gravity_sag"], marshmallow._gravity_sag, 0f, 1f);
                marshmallow._gravity_squishAngle = EditorGUILayout.Slider(localized_texts["_gravity_squishAngle"], marshmallow._gravity_squishAngle, 0f, 30f);
                marshmallow._gravity_sagAngle = EditorGUILayout.Slider(localized_texts["_gravity_sagAngle"], marshmallow._gravity_sagAngle, 0f, 30f);

                EditorGUILayout.Space(20);

                // MARK:Interference / Grab / Squish
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_InterferenceGrabSquish"], headerStyle);
                marshmallow._squishAnimationStrength = EditorGUILayout.Slider(localized_texts["_squishAnimationStrength"], marshmallow._squishAnimationStrength, 0f, 1f);
                marshmallow._breastInterference_AnimationStrength = EditorGUILayout.Slider(localized_texts["_breastInterference_AnimationStrength"], marshmallow._breastInterference_AnimationStrength, 0f, 1f);
                marshmallow._interference = EditorGUILayout.Toggle(localized_texts["_interference"], marshmallow._interference);
                marshmallow._breast_collider_radius = EditorGUILayout.Slider(localized_texts["_breast_collider_radius"], marshmallow._breast_collider_radius, 0f, 0.1f);
                marshmallow._playerInteractions = EditorGUILayout.Toggle(localized_texts["_playerInteractions"], marshmallow._playerInteractions);
                marshmallow._floor = EditorGUILayout.Toggle(localized_texts["_floor"], marshmallow._floor);

                EditorGUILayout.Space(20);

                // MARK:Anti-Penetration
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_AntiPenetrationFunction"], headerStyle);
                marshmallow._buffer_limit_colider_position = EditorGUILayout.Slider(localized_texts["_buffer_limit_colider_position"], marshmallow._buffer_limit_colider_position, 0f, 1f);
                marshmallow._breastInterference_BreakPreventionCollider = EditorGUILayout.Toggle(localized_texts["_breastInterference_BreakPreventionCollider"], marshmallow._breastInterference_BreakPreventionCollider);
                marshmallow._nosquish = EditorGUILayout.Toggle(localized_texts["_nosquish"], marshmallow._nosquish);
                marshmallow._breastInterference_BreakPreventionRotation = EditorGUILayout.Toggle(localized_texts["_breastInterference_BreakPreventionRotation"], marshmallow._breastInterference_BreakPreventionRotation);
                // marshmallow._breastInterference_BreakPreventionAnimation = EditorGUILayout.Toggle(localized_texts["_breastInterference_BreakPreventionAnimation"], marshmallow._breastInterference_BreakPreventionAnimation);

                EditorGUILayout.Space(20);

                // MARK:MenuSetting
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_MenuSetting"], headerStyle);
                marshmallow._marshmallowPBEnabled = EditorGUILayout.Toggle(localized_texts["_marshmallowPBEnabled"], marshmallow._marshmallowPBEnabled);
                marshmallow._installTargetMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(localized_texts["_installTargetMenu"], marshmallow._installTargetMenu, typeof(VRCExpressionsMenu), true);
                EditorGUILayout.Space(20);

                // MARK:Advanced Setting
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_AdvancedSettings"], headerStyle);
                marshmallow._is_chestbase = EditorGUILayout.Toggle(localized_texts["_is_chestbase"], marshmallow._is_chestbase);
                marshmallow._use_transfrom_offset = EditorGUILayout.Toggle(localized_texts["_use_transfrom_offset"], marshmallow._use_transfrom_offset);
                marshmallow._delete_all_PB = EditorGUILayout.Toggle(localized_texts["_delete_all_PB"], marshmallow._delete_all_PB);
                marshmallow._onlysquish = EditorGUILayout.Toggle(localized_texts["_onlysquish"], marshmallow._onlysquish);

                EditorGUILayout.Space(20);
                // MARK:PBPreset Import/Export
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(localized_texts["_Section_PBPreset"], headerStyle);
                EditorGUILayout.LabelField(localized_texts["_pb_preset_explanation"]);
                EditorGUILayout.Space(5);

                // 入力
                EditorGUILayout.LabelField(localized_texts["_pb_preset_import"]);
                EditorGUILayout.BeginHorizontal();
                pbinputstr = EditorGUILayout.TextField(pbinputstr, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                marshmallowPreset_PB pbinput = ScriptableObject.CreateInstance<marshmallowPreset_PB>();
                bool is_import = false;
                if (GUILayout.Button("Import", GUILayout.Width(60)))
                {
                    string result = pbinput.ConvertStringtoPreset(pbinputstr);
                    if (result == "Import successful!") is_import = true;
                    EditorUtility.DisplayDialog("Result", result, "OK");
                }
                EditorGUILayout.EndHorizontal();


                // 出力
                marshmallowPreset_PB pboutput = ScriptableObject.CreateInstance<marshmallowPreset_PB>();

                pboutput._PhysBone_Pull = marshmallow._PhysBone_Pull;
                pboutput._PhysBone_Momentum = marshmallow._PhysBone_Momentum;
                pboutput._PhysBone_Stiffness = marshmallow._PhysBone_Stiffness;
                pboutput._PhysBone_Gravity = marshmallow._PhysBone_Gravity;
                pboutput._PhysBone_GravityFalloff = marshmallow._PhysBone_GravityFalloff;
                pboutput._PhysBone_Immobile = marshmallow._PhysBone_Immobile;
                pboutput._PhysBone_Immobile_type = marshmallow._PhysBone_Immobile_type;
                pboutput._PhysBone_Stretch_Motion = marshmallow._PhysBone_Stretch_Motion;
                pboutput._PhysBone_Max_Stretch = marshmallow._PhysBone_Max_Stretch;
                pboutput._PhysBone_Max_Squish = marshmallow._PhysBone_Max_Squish;

                pboutput._inertia_enabled = marshmallow._inertia_enabled;
                pboutput._inertia_Pull = marshmallow._inertia_Pull;
                pboutput._inertia_Momentum = marshmallow._inertia_Momentum;
                pboutput._inertia_Stiffness = marshmallow._inertia_Stiffness;
                pboutput._inertia_Gravity = marshmallow._inertia_Gravity;
                pboutput._inertia_GravityFalloff = marshmallow._inertia_GravityFalloff;
                pboutput._inertia_Immobile = marshmallow._inertia_Immobile;
                pboutput._inertia_Immobile_type = marshmallow._inertia_Immobile_type;
                pboutput._inertia_StretchMotion = marshmallow._inertia_StretchMotion;
                pboutput._inertia_MaxStretch = marshmallow._inertia_MaxStretch;
                pboutput._inertia_MaxSquish = marshmallow._inertia_MaxSquish;

                pboutput._parallelBone_enabled = marshmallow._parallelBone_enabled;
                pboutput._parallelBone_strengthX = marshmallow._parallelBone_strengthX;
                pboutput._parallelBone_strengthY = marshmallow._parallelBone_strengthY;
                pboutput._parallelBone_squishStrengthX = marshmallow._parallelBone_squishStrengthX;
                pboutput._parallelBone_squishStrengthY = marshmallow._parallelBone_squishStrengthY;

                EditorGUILayout.LabelField(localized_texts["_pb_preset_expoort"]);
                EditorGUILayout.BeginHorizontal();
                string pboutputstr = pboutput.ConvertPeresettoString();
                EditorGUILayout.SelectableLabel(pboutputstr, EditorStyles.textField, GUILayout.Height(18));
                if (GUILayout.Button("Copy", GUILayout.Width(60)))
                {
                    EditorGUIUtility.systemCopyBuffer = pboutputstr;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);


                // 保存
                EditorGUILayout.LabelField(localized_texts["_pb_preset_save"]);
                if (GUILayout.Button("Save"))
                {
                    string path = EditorUtility.SaveFilePanel("Save PhysBone Preset", AssetDatabase.GUIDToAssetPath(_PhysbonePreset_GUID), "NewPreset.asset", "asset");
                    if (path.Length != 0)
                    {
                        path = FileUtil.GetProjectRelativePath(path);
                        // プリセット名を設定
                        pboutput.pb_preset_name = Path.GetFileNameWithoutExtension(path);
                        AssetDatabase.CreateAsset(pboutput, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        _initialized1 = false;
                        _initialized2 = false;
                    }
                }




                // アバタープリセット読み込み(blendshapeごと)
                var preset = marshmallow._presets[marshmallow._index];

                if (marshmallow._index != marshmallow._previndex)
                {
                    if (marshmallow._index != 0)
                    {
                        if (!marshmallow._Breast_L) marshmallow._Breast_L = CheckGameObject(marshmallow._avatar.transform, preset.path_breast_L);
                        if (!marshmallow._Breast_R) marshmallow._Breast_R = CheckGameObject(marshmallow._avatar.transform, preset.path_breast_R);
                        for (int t = 0; t < 10; t++)
                        {
                            marshmallow._PhysBone_collider[t] = CheckPhysBoneCollider(marshmallow._avatar.transform, preset.path_collider[t]);
                        }
                    }
                    marshmallow._prevbreast_blendshape = -1f;
                }

                marshmallow._previndex = marshmallow._index;


                if (marshmallow._breast_blendshape != marshmallow._prevbreast_blendshape)
                {
                    var blendshape_value = marshmallow._breast_blendshape * 0.01f;
                    marshmallow._PhysBone_Limit_Angle = Mathf.Lerp(preset.limit_angle_0, preset.limit_angle_100, blendshape_value);
                    marshmallow._PhysBone_Collision_Radius = Mathf.Lerp(preset.physbone_collision_radius_0, preset.physbone_collision_radius_100, blendshape_value);
                    if (marshmallow._PhysBone_Collision_Radius > 1) marshmallow._PhysBone_Collision_Radius *= 0.01f;
                    // marshmallow._limit_collider_radius = Mathf.Lerp(preset.limit_collider_radius_0, preset.limit_collider_radius_100, blendshape_value);
                    // marshmallow._limit_collider_position_z = Mathf.Lerp(preset.limit_collider_position_z_0, preset.limit_collider_position_z_100, blendshape_value);
                    marshmallow._breast_collider_radius = Mathf.Lerp(preset.breast_collider_radius_0, preset.breast_collider_radius_100, blendshape_value);
                    marshmallow._buffer_limit_colider_position = Mathf.Lerp(preset.buffer_collider_position_0, preset.buffer_collider_position_100, blendshape_value);
                }
                marshmallow._prevbreast_blendshape = marshmallow._breast_blendshape;

                // PBプリセット読み込み
                var pbpreset = marshmallow._pbpresets[marshmallow._PhysBone_index];
                if (is_import) pbpreset = pbinput;

                if (marshmallow._PhysBone_index != marshmallow._prevPhysBone_index || is_import)
                {
                    marshmallow._PhysBone_Pull = pbpreset._PhysBone_Pull;
                    marshmallow._PhysBone_Momentum = pbpreset._PhysBone_Momentum;
                    marshmallow._PhysBone_Stiffness = pbpreset._PhysBone_Stiffness;
                    marshmallow._PhysBone_Gravity = pbpreset._PhysBone_Gravity;
                    marshmallow._PhysBone_GravityFalloff = pbpreset._PhysBone_GravityFalloff;
                    marshmallow._PhysBone_Immobile = pbpreset._PhysBone_Immobile;
                    marshmallow._PhysBone_Immobile_type = pbpreset._PhysBone_Immobile_type;
                    marshmallow._PhysBone_Stretch_Motion = pbpreset._PhysBone_Stretch_Motion;
                    marshmallow._PhysBone_Max_Stretch = pbpreset._PhysBone_Max_Stretch;
                    marshmallow._PhysBone_Max_Squish = pbpreset._PhysBone_Max_Squish;

                    marshmallow._inertia_enabled = pbpreset._inertia_enabled;
                    marshmallow._inertia_Pull = pbpreset._inertia_Pull;
                    marshmallow._inertia_Momentum = pbpreset._inertia_Momentum;
                    marshmallow._inertia_Stiffness = pbpreset._inertia_Stiffness;
                    marshmallow._inertia_Gravity = pbpreset._inertia_Gravity;
                    marshmallow._inertia_GravityFalloff = pbpreset._inertia_GravityFalloff;
                    marshmallow._inertia_Immobile = pbpreset._inertia_Immobile;
                    marshmallow._inertia_Immobile_type = pbpreset._inertia_Immobile_type;
                    marshmallow._inertia_StretchMotion = pbpreset._inertia_StretchMotion;
                    marshmallow._inertia_MaxStretch = pbpreset._inertia_MaxStretch;
                    marshmallow._inertia_MaxSquish = pbpreset._inertia_MaxSquish;

                    marshmallow._parallelBone_enabled = pbpreset._parallelBone_enabled;
                    marshmallow._parallelBone_strengthX = pbpreset._parallelBone_strengthX;
                    marshmallow._parallelBone_strengthY = pbpreset._parallelBone_strengthY;
                    marshmallow._parallelBone_squishStrengthX = pbpreset._parallelBone_squishStrengthX;
                    marshmallow._parallelBone_squishStrengthY = pbpreset._parallelBone_squishStrengthY;
                }
                marshmallow._prevPhysBone_index = marshmallow._PhysBone_index;

            }
            else EditorGUILayout.LabelField("<color=red>" + localized_texts["_Warning_InsertIntoAvatar"] + "</color>", textStyle);

            PrefabUtility.RecordPrefabInstancePropertyModifications(marshmallow);
        }

        // MARK:Local Function
        private string GetFullPath(GameObject obj)
        {
            return GetFullPath(obj.transform);
        }

        private string GetFullPath(Transform t)
        {
            string path = t.name;
            var parent = t.parent;
            while (parent)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }
            return path;
        }
        private GameObject CheckGameObject(Transform trans, string str)
        {
            Transform target = trans.Find(str);
            if (target && target != trans) return target.gameObject;
            return null;
        }

        private VRCPhysBoneBase CheckPhysBone(Transform trans, string str)
        {
            Transform target = trans.Find(str);
            if (target && target != trans) return target.gameObject.GetComponent<VRCPhysBoneBase>();
            return null;
        }

        private VRCPhysBoneColliderBase CheckPhysBoneCollider(Transform trans, string str)
        {
            Transform target = trans.Find(str);
            if (target && target != trans) return target.gameObject.GetComponent<VRCPhysBoneColliderBase>();
            return null;
        }

        private void InitializeAvatarPreset()
        {
            // アバタープリセット読み込み
            string[] filespaths = Directory.GetFiles(AssetDatabase.GUIDToAssetPath(_AvatarPreset_GUID), "*.asset", SearchOption.AllDirectories);

            filespaths = filespaths
                .Where(path => Regex.IsMatch(path, @"[\\\/]\d{3}\.asset$", RegexOptions.IgnoreCase))
                .ToArray();
            // Debug.Log(filespaths.Length);

            int avatar_sum = 200;
            marshmallow._presets = new MarshmallowPreset_ver2[avatar_sum];
            _preset_indexs = new List<int>();
            _preset_names = new List<string>();

            foreach (string filepath in filespaths)
            {
                MarshmallowPreset_ver2 preset = AssetDatabase.LoadAssetAtPath<MarshmallowPreset_ver2>(filepath);
                if (!preset) continue;
                string filename = Path.GetFileNameWithoutExtension(filepath);
                // Debug.Log(filepath);

                if (int.TryParse(filename, out int i))
                {
                    marshmallow._presets[i] = preset;
                    _preset_indexs.Add(i);
                    _preset_names.Add(preset.avatar_name);
                }
                else
                {
                    continue;
                }
            }
        }

        private void InitializePhysbonePreset()
        {
            // PBプリセット読み込み
            string[] filespaths = Directory.GetFiles(AssetDatabase.GUIDToAssetPath(_PhysbonePreset_GUID), "*.asset", SearchOption.AllDirectories);

            filespaths = filespaths.ToArray();
            // Debug.Log(filespaths.Length);

            int pb_sum = 50;
            marshmallow._pbpresets = new marshmallowPreset_PB[pb_sum];
            _pbpreset_indexs = new List<int>();
            _pbpreset_names = new List<string>();

            int i = 0;
            foreach (string filepath in filespaths)
            {
                marshmallowPreset_PB preset = AssetDatabase.LoadAssetAtPath<marshmallowPreset_PB>(filepath);
                if (!preset) continue;
                marshmallow._pbpresets[i] = preset;
                _pbpreset_indexs.Add(i);
                _pbpreset_names.Add(preset.pb_preset_name);
                i++;
            }
        }
        private List<Dictionary<string, string>> Localize()
        {
            List<Dictionary<string, string>> texts = new List<Dictionary<string, string>>();
            StreamReader sr = new StreamReader(AssetDatabase.GUIDToAssetPath(_LocalizeCSV_GUID));
            bool n = false;
            int _lang_number = 4;

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] values = line.Split(',');
                if (!n)
                {
                    for (int i = 0; i < _lang_number; i++)
                    {
                        texts.Add(new Dictionary<string, string>());
                    }

                    n = true;
                }

                for (int j = 0; j < _lang_number; j++)
                {
                    if (values[0] == "") break;
                    texts[j].Add(values[0], values[j + 1]);
                }
            }
            return texts;
        }

        private float FloatFieldCheck(string text, float fl, float min, float max)
        {
            float x = EditorGUILayout.FloatField(text, fl);
            if (x < min) x = min;
            if (max < x) x = max;
            return x;
        }
        private static void DrawWebButton(string text, string URL)
        {
            var position = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            var icon = EditorGUIUtility.IconContent("BuildSettings.Web.Small");
            icon.text = text;

            var textStyle = new GUIStyle(EditorStyles.label) { padding = new RectOffset() };
            textStyle.normal.textColor = textStyle.focused.textColor;
            textStyle.hover.textColor = textStyle.focused.textColor;
            if (GUI.Button(position, icon, textStyle))
            {
                Application.OpenURL(URL);
            }
        }
    }

}