using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.Dynamics;
using wataameya.marshmallow_PB.editor;

// Copyright (c) 2023 wataameya

namespace wataameya.marshmallow_PB.editor
{
    public class MarshmallowSetup : EditorWindow
    {
        [MenuItem("Tools/wataameya/marshmallow_PB_Setup")]
        static void Open()
        {
            var window = GetWindow<MarshmallowSetup>();
            window.titleContent = new GUIContent("marshmallow_PB_Setup");
            window.minSize = new Vector2(420, 500);
        }

        private static readonly string _Prefab_GUID = "e61ce1c57d17d264b9564f636092b278";
        private static readonly string _PrefabS_GUID = "1059f3b231748604f9d4d17c97c230e6";
        private static readonly string _PrefabMA_GUID = "5cb4e9bf8bbf6a04b870fa6d08eb2cab";
        private static readonly string _PrefabSMA_GUID = "ad3233347554ebc4fbe24945b8cb1999";
        private static readonly string _Additive_GUID = "8e38144c59407714baf61d7c0d05d72c";
        private static readonly string _AdditiveS_GUID = "febf7a7a72a463c42ba6f44e354f4292";
        private static readonly string _FX_GUID = "3e616288ee74ab7478b3d79455303e55";
        private static readonly string _Preset_GUID = "202bf923e2a737d47a8725ef0f38b54a";
        private static readonly string _LocalizeCSV_GUID = "3b4c0c8ca80cf444e8adecbbfac1ec96";
        private static readonly string _Setup_GUID = "9932b11e8a452d7458310d83020cb5ac";

        private int _lang = 0;
        private int _lang_number = 0;
        private List<List<string>> _texts = new List<List<string>>();

        private Vector2 _scrollpos = Vector2.zero;

        private int _index = 0;
        private int _previndex = -1;

        private float _breast_blendshape = 0f;
        private float _prevbreast_blendshape = -1f;

        private bool _initialized = false;
        private List<MarshmallowPreset> _presets;
        private List<string> _preset_names;

        private VRCAvatarDescriptor _descriptor = null;
        private VRCAvatarDescriptor _descriptor_copy;
        private GameObject _avatar;
        private GameObject _prevavatar = null;
        private GameObject _avatar_copy;
        private GameObject _mpb;
        private GameObject _mpb_prefab = null;
        private GameObject _mpb_instance = null;
        private GameObject _Breast_L;
        private GameObject _Breast_R;

        private bool _isOpen1;
        private bool _isOpen2;
        private bool _isOpen3;
        private VRCPhysBoneColliderBase[] _PhysBone_collider = new VRCPhysBoneColliderBase[5];

        //private bool _copy=true;
        // private bool _grab=true;
        // private bool _limit_velocity=true;
        private bool _floor = true;
        private bool _writedefaults = true;
        private bool _interference = true;

#if USE_MA
    private bool _modularavatar=true;
#else
        private bool _modularavatar = false;
#endif

#if USE_SQUISH
    private bool _squishPB=true;
#else
        private bool _squishPB = false;
#endif

        private bool _interference_squishPB = false;


        private float _breast_scale = 1.0f;
        private float _limit_collider_position_z = 0.135f;
        private float _breast_collider_radius = 0.1f;
        private float _rotation_constraint_weight = 0.8f;
        private float _scale_constraint_weight = 1.0f;


        private int _PhysBone_index = 2;
        private int _prevPhysBone_index = -1;
        float[,] _PhysBone_preset = new float[5, 6]
        {
        {0.08f,0.00f,0.08f,0.30f,1.00f,0.20f},
        {0.10f,0.30f,0.25f,0.30f,1.00f,0.25f},
        {0.10f,0.50f,0.25f,0.30f,1.00f,0.50f},
        {0.15f,0.50f,0.30f,0.30f,1.00f,0.60f},
        {0.20f,0.50f,0.30f,0.30f,1.00f,0.75f},
        };


        private float _PhysBone_Pull = 0.1f;
        private float _PhysBone_Momentum = 0.5f;
        private float _PhysBone_Stiffness = 0.25f;
        private float _PhysBone_Gravity = 0.02f;
        private float _PhysBone_GravityFalloff = 1f;
        private float _PhysBone_Immobile = 0.5f;
        private float _PhysBone_Limit_Angle = 40f;
        private float _PhysBone_Collision_Radius = 4.0f;

        private float[] _PhysBone_setting = new float[4];

        private GameObject _test;


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void OnGUI()
        {
            Initialize();
            if (_texts.Count == 0) Localize();
            _scrollpos = EditorGUILayout.BeginScrollView(_scrollpos);
            EditorGUIUtility.labelWidth = position.size.x / 2;

            List<string> languages = new List<string>();

            for (int i = 0; i < _lang_number; i++)
            {
                languages.Add(_texts[i][1]);
            }

            _lang = EditorGUILayout.Popup("Language", (int)_lang, languages.ToArray());

            ///メニュー
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.wordWrap = true;
            style.richText = true;

            EditorGUILayout.LabelField($"<color=red><size=15>" + _texts[_lang][81] + "</size></color>", style);
            DrawWebButton(_texts[_lang][56], _texts[_lang][57]);
            DrawWebButton("booth", "https://wataame89.booth.pm/items/4511536");

            EditorGUILayout.LabelField(_texts[_lang][2], style);

            _avatar = (GameObject)EditorGUILayout.ObjectField(_texts[_lang][3], _avatar, typeof(GameObject), true);

            if (_avatar != null)
            {
                EditorGUILayout.LabelField(_texts[_lang][4], style);
                _index = EditorGUILayout.Popup(_texts[_lang][5], (int)_index, _preset_names.ToArray());

                if (_avatar != _prevavatar | _index != _previndex)
                {
                    if (_avatar != _prevavatar | _index != 0)
                    {
                        _Breast_L = CheckGameObject(_avatar.transform.Find(_presets[_index].path_breast_L));
                        _Breast_R = CheckGameObject(_avatar.transform.Find(_presets[_index].path_breast_R));
                        for (int t = 0; t < 5; t++)
                        {
                            _PhysBone_collider[t] = CheckPhysBoneCollider(_avatar.transform.Find(_presets[_index].path_collider[t]));
                        }
                    }
                    _prevbreast_blendshape = -1f;
                }
                _prevavatar = _avatar;
                _previndex = _index;

                _Breast_L = (GameObject)EditorGUILayout.ObjectField(_texts[_lang][6], _Breast_L, typeof(GameObject), true);
                _Breast_R = (GameObject)EditorGUILayout.ObjectField(_texts[_lang][7], _Breast_R, typeof(GameObject), true);

                if ((int)_index == 0)
                {
                    _mpb = (GameObject)EditorGUILayout.ObjectField(_texts[_lang][8], _mpb, typeof(GameObject), true);
                }
                else
                {
                    _breast_blendshape = EditorGUILayout.Slider(_texts[_lang][9], _breast_blendshape, 0f, 100f);
                }

                if (_prevbreast_blendshape != _breast_blendshape)
                {
                    _PhysBone_Limit_Angle = Mathf.Lerp(_presets[_index].limit_angle_0, _presets[_index].limit_angle_100, _breast_blendshape * 0.01f);
                    _PhysBone_Collision_Radius = Mathf.Lerp(_presets[_index].collision_radius_0, _presets[_index].collision_radius_100, _breast_blendshape * 0.01f);
                    _limit_collider_position_z = Mathf.Lerp(_presets[_index].limit_collider_position_z_0, _presets[_index].limit_collider_position_z_100, _breast_blendshape * 0.01f);
                    _breast_collider_radius = Mathf.Lerp(_presets[_index].breast_collider_radius_0, _presets[_index].breast_collider_radius_100, _breast_blendshape * 0.01f);
                }
                _prevbreast_blendshape = _breast_blendshape;

                if (_index != 0)
                {
                    _breast_scale = FloatFieldCheck(_texts[_lang][10], _breast_scale, 0.1f, 10f);
                }
                else
                {
                    EditorGUILayout.Space(20);
                }

                EditorGUILayout.Space(10);

                _PhysBone_index = EditorGUILayout.Popup(_texts[_lang][12], (int)_PhysBone_index, new string[5] { _texts[_lang][13], _texts[_lang][14], _texts[_lang][15], _texts[_lang][16], _texts[_lang][17] });

                if (_PhysBone_index != _prevPhysBone_index)
                {
                    _PhysBone_Pull = _PhysBone_preset[_PhysBone_index, 0];
                    _PhysBone_Momentum = _PhysBone_preset[_PhysBone_index, 1];
                    _PhysBone_Stiffness = _PhysBone_preset[_PhysBone_index, 2];
                    _PhysBone_Gravity = _PhysBone_preset[_PhysBone_index, 3];
                    _PhysBone_GravityFalloff = _PhysBone_preset[_PhysBone_index, 4];
                    _PhysBone_Immobile = _PhysBone_preset[_PhysBone_index, 5];
                }

                _prevPhysBone_index = _PhysBone_index;

                _isOpen1 = EditorGUILayout.Foldout(_isOpen1, _texts[_lang][11]);
                if (_isOpen1)
                {
                    _PhysBone_Pull = EditorGUILayout.Slider(_texts[_lang][20], _PhysBone_Pull, 0f, 1.0f);
                    _PhysBone_Momentum = EditorGUILayout.Slider(_texts[_lang][21], _PhysBone_Momentum, 0f, 1.0f);
                    _PhysBone_Stiffness = EditorGUILayout.Slider(_texts[_lang][22], _PhysBone_Stiffness, 0f, 1.0f);
                    _PhysBone_Gravity = EditorGUILayout.Slider(_texts[_lang][23], _PhysBone_Gravity, 0f, 1.0f);
                    _PhysBone_GravityFalloff = EditorGUILayout.Slider(_texts[_lang][24], _PhysBone_GravityFalloff, 0f, 1.0f);
                    _PhysBone_Immobile = EditorGUILayout.Slider(_texts[_lang][25], _PhysBone_Immobile, 0f, 1.0f);

                    _PhysBone_Limit_Angle = FloatFieldCheck(_texts[_lang][26], _PhysBone_Limit_Angle, 0f, 180f);
                    _PhysBone_Collision_Radius = FloatFieldCheck(_texts[_lang][27], _PhysBone_Collision_Radius, 0f, 10f);
                    _limit_collider_position_z = FloatFieldCheck(_texts[_lang][28], _limit_collider_position_z, 0f, 0.24f);

                    _isOpen2 = EditorGUILayout.Foldout(_isOpen2, _texts[_lang][29]);
                    if (_isOpen2)
                    {
                        for (int i = 0; i <= 4; i++)
                        {
                            _PhysBone_collider[i] = (VRCPhysBoneColliderBase)EditorGUILayout.ObjectField(_texts[_lang][30] + i, _PhysBone_collider[i], typeof(VRCPhysBoneColliderBase), true);
                        }
                    }
                }

                EditorGUILayout.Space(20);
                _floor = EditorGUILayout.Toggle(_texts[_lang][41], _floor);
                //_limit_velocity = EditorGUILayout.Toggle(_texts[_lang][42], _limit_velocity);
                if (_interference = EditorGUILayout.Toggle(_texts[_lang][43], _interference))
                {
                    _breast_collider_radius = FloatFieldCheck(_texts[_lang][44], _breast_collider_radius, 0f, 0.24f);
                }
                else
                {
                    EditorGUILayout.Space(20);
                }

                EditorGUILayout.Space(10);

                _writedefaults = EditorGUILayout.Toggle(_texts[_lang][46], _writedefaults);
                _modularavatar = EditorGUILayout.Toggle(_texts[_lang][47], _modularavatar);
                if (_squishPB = EditorGUILayout.Toggle(_texts[_lang][48], _squishPB))
                {
                    _rotation_constraint_weight = EditorGUILayout.Slider(_texts[_lang][49], _rotation_constraint_weight, 0f, 1f);
                    _scale_constraint_weight = EditorGUILayout.Slider(_texts[_lang][50], _scale_constraint_weight, 0f, 1f);
                    _interference_squishPB = EditorGUILayout.Toggle(_texts[_lang][51], _interference_squishPB);
                }
                else
                {
                    EditorGUILayout.Space(40);
                }
                EditorGUILayout.Space(0);

                _isOpen3 = EditorGUILayout.Foldout(_isOpen3, _texts[_lang][45]);
                if (_isOpen3)
                {
                    if (GUILayout.Button(_texts[_lang][58]))
                    {
                        string result = Prefab();
                        if (result == "success")
                        {
                            EditorUtility.DisplayDialog("Result", _texts[_lang][59], "OK");
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error", result, "OK");
                            DestroyImmediate(_mpb_instance);
                        }
                    }
                }
            }

            EditorGUILayout.Space(20);

            ///アバター設定
            if (GUILayout.Button(_texts[_lang][60]))
            {
                string result = Setup();
                if (result == "success")
                {
                    EditorUtility.DisplayDialog("Result", _texts[_lang][61], "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", result, "OK");
                    DestroyImmediate(_avatar_copy);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private string Setup()
        {

            if (!_avatar | !_Breast_L | !_Breast_R) return _texts[_lang][62];
            if (_index == 0 & !_mpb) return _texts[_lang][63];

            //初期化
            _avatar_copy = null;

            //アバター複製
            _avatar_copy = GameObject.Instantiate(_avatar) as GameObject;
            _avatar_copy.transform.name = _avatar.transform.name + "_MPB";

            //元アバター非表示
            _avatar.SetActive(false);
            _avatar_copy.SetActive(true);

            _mpb_prefab = null;
            _mpb_instance = null;

            Animator _animator_copy = _avatar_copy.GetComponent<Animator>();
            if (!_animator_copy) return _texts[_lang][66];

            Transform _Chest_copy_transform = _animator_copy.GetBoneTransform(HumanBodyBones.Chest);
            Transform _Hips_copy_transform = _animator_copy.GetBoneTransform(HumanBodyBones.Hips);
            Transform _Armature_copy_transform = _Hips_copy_transform.parent.gameObject.transform;

            if (_index == 0)
            {
                _mpb_prefab = _mpb;
            }
            else
            {
                if (_modularavatar)
                {
                    if (_squishPB) _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_PrefabSMA_GUID));
                    else _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_PrefabMA_GUID));
                }
                else
                {
                    if (_squishPB) _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_PrefabS_GUID));
                    else _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_Prefab_GUID));
                }

                if (!_mpb_prefab) return _texts[_lang][64];
            }

            if (_squishPB & !_mpb_prefab.name.Contains("_squish")) return _texts[_lang][70];

            _mpb_instance = GameObject.Instantiate(_mpb_prefab) as GameObject;
            _mpb_instance.transform.parent = _avatar_copy.transform;
            _mpb_instance.name = "marshmallow_PB";

            if (_index == 0) _mpb.SetActive(false);
            _mpb_instance.SetActive(true);

            //階層取得
            GameObject _mpb_D = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)").gameObject;
            GameObject _mpb_D_L = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_L").gameObject;
            GameObject _mpb_D_R = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_R").gameObject;
            GameObject _mpb_L = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_L/Adjust_Breast_L/Breast_L").gameObject;
            GameObject _mpb_R = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_R/Adjust_Breast_R/Breast_R").gameObject;

            if (_index != 0)
            {
                //位置設定
                _mpb_instance.transform.localPosition = new Vector3(0, 0, 0);
                _mpb_instance.transform.localScale = _Armature_copy_transform.localScale;
                Vector3 breast_position = new Vector3(0, 0, 0);
                Vector3 breast_scale = new Vector3(1, 1, 1);

                if (_presets[_index].mpb_breast_L_position_0 == new Vector3(0, 0, 0)) return _texts[_lang][65];

                breast_position = Vector3.Lerp(_presets[_index].mpb_breast_L_position_0, _presets[_index].mpb_breast_L_position_100, _breast_blendshape * 0.01f);
                if (_presets[_index].mpb_breast_L_position_100 == new Vector3(0, 0, 0)) breast_position = _presets[_index].mpb_breast_L_position_0;

                float ColScaleFactor = 12 / (12 + _PhysBone_Collision_Radius);

                breast_scale = Vector3.Lerp(_presets[_index].mpb_breast_scale_0, _presets[_index].mpb_breast_scale_100, _breast_blendshape * 0.01f) * _breast_scale * ColScaleFactor * 1.25f;

                _mpb_D_L.transform.localPosition = breast_position;
                breast_position.x *= -1f;
                _mpb_D_R.transform.localPosition = breast_position;

                _mpb_D_L.transform.localScale = breast_scale;
                _mpb_D_R.transform.localScale = breast_scale;
            }

            GameObject _Breast_L_copy = _avatar_copy.transform.Find(GetFullPath(_Breast_L).Replace(GetFullPath(_avatar) + "/", "")).gameObject;
            GameObject _Breast_R_copy = _avatar_copy.transform.Find(GetFullPath(_Breast_R).Replace(GetFullPath(_avatar) + "/", "")).gameObject;

            VRCPhysBoneBase[] PhysBones = new VRCPhysBoneBase[6];
            PhysBones[0] = _mpb_instance.transform.Find("PhysBone_L_a").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[1] = _mpb_instance.transform.Find("PhysBone_L_b").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[2] = _mpb_instance.transform.Find("PhysBone_R_a").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[3] = _mpb_instance.transform.Find("PhysBone_R_b").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[4] = _mpb_instance.transform.Find("Grabbing_L").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[5] = _mpb_instance.transform.Find("Grabbing_R").gameObject.GetComponent<VRCPhysBoneBase>();

            VRCPhysBoneColliderBase Grabbing_L_Collider = _mpb_instance.transform.Find("Grabbing_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Grabbing_R_Collider = _mpb_instance.transform.Find("Grabbing_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();

            VRCPhysBoneColliderBase Limit_PhysBone_L = _mpb_instance.transform.Find("System/Limit_PhysBone_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Limit_PhysBone_R = _mpb_instance.transform.Find("System/Limit_PhysBone_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase BreastCollider_L = _mpb_instance.transform.Find("System/BreastCollider_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase BreastCollider_R = _mpb_instance.transform.Find("System/BreastCollider_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase FloorCollider = _mpb_instance.transform.Find("System/Floor").gameObject.GetComponent<VRCPhysBoneColliderBase>();

            //PB削除
            VRCPhysBoneBase _avatar_PB_L = _Breast_L_copy.gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase _avatar_PB_R = _Breast_R_copy.gameObject.GetComponent<VRCPhysBoneBase>();

            VRCPhysBoneBase _avatar_PB_L_Comp = CheckPhysBone(_avatar_copy.transform.Find(_presets[_index].path_PB[0]));
            VRCPhysBoneBase _avatar_PB_R_Comp = CheckPhysBone(_avatar_copy.transform.Find(_presets[_index].path_PB[1]));

            DestroyImmediate(_avatar_PB_L);
            DestroyImmediate(_avatar_PB_R);

            if (_avatar_PB_L_Comp) _avatar_PB_L_Comp.enabled = false;
            if (_avatar_PB_R_Comp) _avatar_PB_R_Comp.enabled = false;

            //階層移動
            _mpb_D.transform.parent = _Chest_copy_transform;
            _mpb_L.transform.parent = _Breast_L_copy.transform.parent;
            _mpb_R.transform.parent = _Breast_R_copy.transform.parent;

            _mpb_D_L.transform.parent = _Breast_L_copy.transform.parent;
            _mpb_D_R.transform.parent = _Breast_R_copy.transform.parent;

            if (_mpb_D.transform.parent == _mpb_D_L.transform.parent & _mpb_D.transform.parent == _mpb_D_R.transform.parent)
            {
                _mpb_D_L.transform.parent = _mpb_D.transform;
                _mpb_D_R.transform.parent = _mpb_D.transform;
            }

            _Breast_L_copy.transform.parent = _mpb_L.transform;
            _Breast_R_copy.transform.parent = _mpb_R.transform;

            _mpb_L.name = _Breast_L_copy.name;
            _mpb_R.name = _Breast_R_copy.name;

            //PB設定
            foreach (VRCPhysBoneBase p in PhysBones)
            {
                p.pull = _PhysBone_Pull;
                p.spring = _PhysBone_Momentum;
                p.stiffness = _PhysBone_Stiffness;
                p.gravity = _PhysBone_Gravity;
                p.gravityFalloff = _PhysBone_GravityFalloff;
                p.immobile = _PhysBone_Immobile;
                p.maxAngleX = _PhysBone_Limit_Angle;
                p.radius = (_squishPB) ? _PhysBone_Collision_Radius * 0.01f : _PhysBone_Collision_Radius;

                for (int i = 0; i <= 4; i++)
                {
                    if (_PhysBone_collider[i] & p.colliders.Count >= i + 4)
                    {
                        p.colliders[i + 4] = CheckGameObject(_avatar_copy.transform.Find(GetFullPath(_PhysBone_collider[i].gameObject).Replace(GetFullPath(_avatar) + "/", ""))).GetComponent<VRCPhysBoneColliderBase>();
                    }
                }
            }

            PhysBones[4].immobile = 1.0f;
            PhysBones[5].immobile = 1.0f;

            //Grabbing用PhysBoneColliderの設定
            Grabbing_L_Collider.radius = _PhysBone_Collision_Radius;
            Grabbing_R_Collider.radius = _PhysBone_Collision_Radius;

            //PhysBoneの制限コライダー設定
            Limit_PhysBone_L.position.z = _limit_collider_position_z;
            Limit_PhysBone_R.position.z = _limit_collider_position_z;

            Limit_PhysBone_L.radius = 0.075f + _PhysBone_Collision_Radius * 0.01f;
            Limit_PhysBone_R.radius = 0.075f + _PhysBone_Collision_Radius * 0.01f;

            //相互干渉コライダー設定
            BreastCollider_L.enabled = _interference;
            BreastCollider_R.enabled = _interference;

            BreastCollider_L.radius = _breast_collider_radius;
            BreastCollider_R.radius = _breast_collider_radius;

            //床コライダー設定
            FloorCollider.enabled = _floor;

            //Squishコライダー設定
            if (_squishPB)
            {
                VRCPhysBoneColliderBase Squish_Collider_L = _mpb_instance.transform.Find("System/Squish_Collider_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
                VRCPhysBoneColliderBase Squish_Collider_R = _mpb_instance.transform.Find("System/Squish_Collider_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
                Squish_Collider_L.enabled = _interference_squishPB;
                Squish_Collider_R.enabled = _interference_squishPB;
                Squish_Collider_L.position.z = _PhysBone_Collision_Radius * 0.01f * 1.08f * _mpb_D_L.transform.localScale.z;
                Squish_Collider_R.position.z = _PhysBone_Collision_Radius * 0.01f * 1.08f * _mpb_D_R.transform.localScale.z;
            }

            //SquishPBのConstraint設定
            if (_squishPB)
            {
                PositionConstraint PC_L = _mpb_L.GetComponent<PositionConstraint>();
                PositionConstraint PC_R = _mpb_R.GetComponent<PositionConstraint>();
                RotationConstraint RC_L = _mpb_L.GetComponent<RotationConstraint>();
                RotationConstraint RC_R = _mpb_R.GetComponent<RotationConstraint>();
                ScaleConstraint SC_L = _mpb_L.GetComponent<ScaleConstraint>();
                ScaleConstraint SC_R = _mpb_R.GetComponent<ScaleConstraint>();
                RC_L.weight = _rotation_constraint_weight;
                RC_R.weight = _rotation_constraint_weight;
                SC_L.weight = _scale_constraint_weight;
                SC_R.weight = _scale_constraint_weight;
            }

            //FX設定
            if (!_modularavatar)
            {
                _descriptor = _avatar.gameObject.GetComponent<VRCAvatarDescriptor>();

                if (!_descriptor) return _texts[_lang][67];

                _descriptor_copy = _avatar_copy.gameObject.GetComponent<VRCAvatarDescriptor>();
                AnimatorController _avatar_fx = _descriptor.baseAnimationLayers[4].animatorController as AnimatorController;

                if (!_avatar_fx) return _texts[_lang][68];

                string _newFxPath = AssetDatabase.GUIDToAssetPath(_FX_GUID) + "/" + _avatar.name + "_MPB.controller";

                if (_avatar_fx.name.Substring(Math.Max(_avatar_fx.name.Length - 4, 0)) == "_MPB") return _texts[_lang][69];

                if (!AssetDatabase.IsValidFolder(AssetDatabase.GUIDToAssetPath(_FX_GUID)))
                {
                    AssetDatabase.CreateFolder(AssetDatabase.GUIDToAssetPath(_Setup_GUID), "FX");
                }

                AnimatorController _avatar_fx_copy = AssetDatabase.LoadAssetAtPath<AnimatorController>(_newFxPath);
                AnimatorController _additive_fx;
                if (_squishPB) _additive_fx = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(_AdditiveS_GUID));
                else _additive_fx = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(_Additive_GUID));

                if (!_avatar_fx_copy)
                {
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(_avatar_fx), _newFxPath);
                    _avatar_fx_copy = AssetDatabase.LoadAssetAtPath<AnimatorController>(_newFxPath);
                }
                else
                {
                    int length_layers = _avatar_fx_copy.layers.Length;
                    for (int i = 0; i < length_layers; i++)
                    {
                        _avatar_fx_copy.RemoveLayer(length_layers - 1 - i);
                    }

                    int length_parameters = _avatar_fx_copy.parameters.Length;
                    for (int j = 0; j < length_parameters; j++)
                    {
                        _avatar_fx_copy.RemoveParameter(length_parameters - 1 - j);
                    }
                    AnimatorControllerUtility.CombineAnimatorController(_avatar_fx, _avatar_fx_copy);
                }

                AnimatorControllerUtility.CombineAnimatorController(_additive_fx, _avatar_fx_copy);

                if (!_writedefaults)
                {
                    foreach (var layer in _avatar_fx_copy.layers)
                    {
                        WriteDefaultOff(layer.stateMachine);
                    }
                }

                _descriptor_copy.baseAnimationLayers[4].animatorController = _avatar_fx_copy;
            }
            if (_squishPB)
            {
                PhysBones[0].gameObject.name = "PhysBone_L";
                PhysBones[2].gameObject.name = "PhysBone_R";
                DestroyImmediate(PhysBones[1].gameObject);
                DestroyImmediate(PhysBones[3].gameObject);
                DestroyImmediate(PhysBones[4].gameObject);
                DestroyImmediate(PhysBones[5].gameObject);
            }
            return "success";
        }

        private string Prefab()
        {
            _mpb_prefab = null;
            _mpb_instance = null;

            if (_modularavatar)
            {
                if (_squishPB) _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_PrefabSMA_GUID));
                else _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_PrefabMA_GUID));
            }
            else
            {
                if (_squishPB) _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_PrefabS_GUID));
                else _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_Prefab_GUID));
            }

            if (!_mpb_prefab) return _texts[_lang][63];

            _mpb_instance = GameObject.Instantiate(_mpb_prefab) as GameObject;
            _mpb_instance.name = "marshmallow_PB";

            //階層取得
            GameObject _mpb_D = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)").gameObject;
            GameObject _mpb_D_L = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_L").gameObject;
            GameObject _mpb_D_R = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_R").gameObject;
            GameObject _mpb_L = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_L/Adjust_Breast_L/Breast_L").gameObject;
            GameObject _mpb_R = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_R/Adjust_Breast_R/Breast_R").gameObject;

            //位置設定
            _mpb_instance.transform.localPosition = new Vector3(0, 0, 0);
            _mpb_instance.transform.localScale = new Vector3(1, 1, 1);
            Vector3 breast_position = new Vector3(0, 0, 0);
            Vector3 breast_scale = new Vector3(1, 1, 1);

            if (_presets[_index].mpb_breast_L_position_0 == new Vector3(0, 0, 0)) return _texts[_lang][65];

            breast_position = Vector3.Lerp(_presets[_index].mpb_breast_L_position_0, _presets[_index].mpb_breast_L_position_100, _breast_blendshape * 0.01f);
            if (_presets[_index].mpb_breast_L_position_100 == new Vector3(0, 0, 0)) breast_position = _presets[_index].mpb_breast_L_position_0;

            float ColScaleFactor = 12 / (12 + _PhysBone_Collision_Radius);

            breast_scale = Vector3.Lerp(_presets[_index].mpb_breast_scale_0, _presets[_index].mpb_breast_scale_100, _breast_blendshape * 0.01f) * _breast_scale * ColScaleFactor * 1.25f;

            _mpb_D_L.transform.localPosition = breast_position;
            breast_position.x *= -1f;
            _mpb_D_R.transform.localPosition = breast_position;

            _mpb_D_L.transform.localScale = breast_scale;
            _mpb_D_R.transform.localScale = breast_scale;

            VRCPhysBoneBase[] PhysBones = new VRCPhysBoneBase[6];
            PhysBones[0] = _mpb_instance.transform.Find("PhysBone_L_a").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[1] = _mpb_instance.transform.Find("PhysBone_L_b").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[2] = _mpb_instance.transform.Find("PhysBone_R_a").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[3] = _mpb_instance.transform.Find("PhysBone_R_b").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[4] = _mpb_instance.transform.Find("Grabbing_L").gameObject.GetComponent<VRCPhysBoneBase>();
            PhysBones[5] = _mpb_instance.transform.Find("Grabbing_R").gameObject.GetComponent<VRCPhysBoneBase>();

            VRCPhysBoneColliderBase Grabbing_L_Collider = _mpb_instance.transform.Find("Grabbing_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Grabbing_R_Collider = _mpb_instance.transform.Find("Grabbing_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();

            VRCPhysBoneColliderBase Limit_PhysBone_L = _mpb_instance.transform.Find("System/Limit_PhysBone_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Limit_PhysBone_R = _mpb_instance.transform.Find("System/Limit_PhysBone_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase BreastCollider_L = _mpb_instance.transform.Find("System/BreastCollider_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase BreastCollider_R = _mpb_instance.transform.Find("System/BreastCollider_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase FloorCollider = _mpb_instance.transform.Find("System/Floor").gameObject.GetComponent<VRCPhysBoneColliderBase>();

            //PB設定
            foreach (VRCPhysBoneBase p in PhysBones)
            {
                p.pull = _PhysBone_Pull;
                p.spring = _PhysBone_Momentum;
                p.stiffness = _PhysBone_Stiffness;
                p.gravity = _PhysBone_Gravity;
                p.gravityFalloff = _PhysBone_GravityFalloff;
                p.immobile = _PhysBone_Immobile;
                p.maxAngleX = _PhysBone_Limit_Angle;
                p.radius = (_squishPB) ? _PhysBone_Collision_Radius * 0.01f : _PhysBone_Collision_Radius;

                for (int i = 0; i <= 4; i++)
                {
                    if (_PhysBone_collider[i] & p.colliders.Count >= i + 4)
                    {
                        //p.colliders[i+4] = CheckGameObject(_avatar_copy.transform.Find(GetFullPath(_PhysBone_collider[i].gameObject).Replace(GetFullPath(_avatar) + "/", ""))).GetComponent<VRCPhysBoneColliderBase>();
                    }
                }
            }

            PhysBones[4].immobile = 1.0f;
            PhysBones[5].immobile = 1.0f;

            //Grabbing用PhysBoneColliderの設定
            Grabbing_L_Collider.radius = _PhysBone_Collision_Radius;
            Grabbing_R_Collider.radius = _PhysBone_Collision_Radius;

            //PhysBoneの制限コライダー設定

            Limit_PhysBone_L.position.z = _limit_collider_position_z;
            Limit_PhysBone_R.position.z = _limit_collider_position_z;

            Limit_PhysBone_L.radius = 0.075f + _PhysBone_Collision_Radius * 0.01f;
            Limit_PhysBone_R.radius = 0.075f + _PhysBone_Collision_Radius * 0.01f;

            //相互干渉コライダー設定
            BreastCollider_L.enabled = _interference;
            BreastCollider_R.enabled = _interference;

            BreastCollider_L.radius = _breast_collider_radius;
            BreastCollider_R.radius = _breast_collider_radius;

            //床コライダー設定
            FloorCollider.enabled = _floor;

            //Squishコライダー設定
            if (_squishPB)
            {
                VRCPhysBoneColliderBase Squish_Collider_L = _mpb_instance.transform.Find("System/Squish_Collider_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
                VRCPhysBoneColliderBase Squish_Collider_R = _mpb_instance.transform.Find("System/Squish_Collider_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
                Squish_Collider_L.enabled = _interference_squishPB;
                Squish_Collider_R.enabled = _interference_squishPB;
                Squish_Collider_L.position.z = _PhysBone_Collision_Radius * 0.01f * 1.08f * _mpb_D_L.transform.localScale.z;
                Squish_Collider_R.position.z = _PhysBone_Collision_Radius * 0.01f * 1.08f * _mpb_D_R.transform.localScale.z;
            }

            //SquishPBのConstraint設定
            if (_squishPB)
            {
                PositionConstraint PC_L = _mpb_L.GetComponent<PositionConstraint>();
                PositionConstraint PC_R = _mpb_R.GetComponent<PositionConstraint>();
                RotationConstraint RC_L = _mpb_L.GetComponent<RotationConstraint>();
                RotationConstraint RC_R = _mpb_R.GetComponent<RotationConstraint>();
                ScaleConstraint SC_L = _mpb_L.GetComponent<ScaleConstraint>();
                ScaleConstraint SC_R = _mpb_R.GetComponent<ScaleConstraint>();
                RC_L.weight = _rotation_constraint_weight;
                RC_R.weight = _rotation_constraint_weight;
                SC_L.weight = _scale_constraint_weight;
                SC_R.weight = _scale_constraint_weight;
            }

            return "success";
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string GetFullPath(GameObject obj)
        {
            return GetFullPath(obj.transform);
        }

        public string GetFullPath(Transform t)
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

        public GameObject CheckGameObject(Transform t)
        {
            if (t)
            {
                if (t.name == _avatar.name) return null;
                return t.gameObject;
            }
            return null;
        }

        public VRCPhysBoneBase CheckPhysBone(Transform t)
        {
            if (t)
            {
                if (t.name == _avatar.name) return null;
                return t.gameObject.GetComponent<VRCPhysBoneBase>(); ;
            }
            return null;
        }
        public VRCPhysBoneColliderBase CheckPhysBoneCollider(Transform t)
        {
            if (t)
            {
                if (t.name == _avatar.name) return null;
                return t.gameObject.GetComponent<VRCPhysBoneColliderBase>(); ;
            }
            return null;
        }

        public void Localize()
        {
            _texts = new List<List<string>>();
            StreamReader sr = new StreamReader(AssetDatabase.GUIDToAssetPath(_LocalizeCSV_GUID));
            bool n = false;

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] values = line.Split(',');
                if (!n)
                {
                    _lang_number = values.Length;
                    for (int i = 0; i < _lang_number; i++)
                    {
                        _texts.Add(new List<string>());
                    }

                    n = true;
                    for (int j = 0; j < _lang_number; j++)
                    {
                        _texts[j].Add("");
                    }
                }

                for (int j = 0; j < _lang_number; j++)
                {
                    _texts[j].Add(values[j]);
                }
            }
        }

        public void Initialize()
        {
            if (_initialized) return;
            string[] files = Directory.GetFiles(AssetDatabase.GUIDToAssetPath(_Preset_GUID), "*.asset", SearchOption.AllDirectories);

            _presets = new List<MarshmallowPreset>();
            _preset_names = new List<string>();

            foreach (string i in files)
            {
                MarshmallowPreset preset = AssetDatabase.LoadAssetAtPath<MarshmallowPreset>(i);
                if (!preset) continue;
                _presets.Add(preset);
                _preset_names.Add(preset.avatar_name);

            }
            _initialized = true;
        }

        public void WriteDefaultOff(AnimatorStateMachine statemachine)
        {
            foreach (var childstate in statemachine.states)
            {
                childstate.state.writeDefaultValues = false;
            }

            foreach (var childstatemachine in statemachine.stateMachines)
            {
                WriteDefaultOff(childstatemachine.stateMachine);
            }
        }

        public float FloatFieldCheck(string text, float fl, float min, float max)
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

            var style = new GUIStyle(EditorStyles.label) { padding = new RectOffset() };
            style.normal.textColor = style.focused.textColor;
            style.hover.textColor = style.focused.textColor;
            if (GUI.Button(position, icon, style))
            {
                Application.OpenURL(URL);
            }
        }
    }
}