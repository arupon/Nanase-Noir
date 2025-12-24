#region

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using VRC.Dynamics;
using UnityEngine.Animations;
using Unity.Mathematics;
using wataameya.marshmallow_PB.ndmf.editor;
using System.Data;
using UnityEngine.UIElements;
using nadena.dev.modular_avatar.core.editor.plugin;
using System.CodeDom;
using VRC.Dynamics.ManagedTypes;
using VRC.SDK3.Avatars.ScriptableObjects;
using System;
using nadena.dev.modular_avatar.core;




#if USE_NDMF
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
#endif

#endregion

#if USE_NDMF
[assembly: ExportsPlugin(typeof(marshmallow_PB_MA_Plugin))]

namespace wataameya.marshmallow_PB.ndmf.editor
{
    public class marshmallow_PB_MA_Plugin : Plugin<marshmallow_PB_MA_Plugin>
    {
        /// <summary>
        /// This name is used to identify the plugin internally, and can be used to declare BeforePlugin/AfterPlugin
        /// dependencies. If not set, the full type name will be used.
        /// </summary>
        public override string QualifiedName => "wataameya.marshmallow_PB.ndmf";

        /// <summary>
        /// The plugin name shown in debug UIs. If not set, the qualified name will be shown.
        /// </summary>
        public override string DisplayName => "marshmallow_PB_MA";

        private static readonly string _Prefab_GUID = "239a236861d527346986477ffc4ea7cd";
        private static readonly string _Prefab_Menu_GUID = "4ee8d85ba1332e544956687bce6377f6";
        private static readonly string _LocalizeCSV_GUID = "3b4c0c8ca80cf444e8adecbbfac1ec96";
        private List<Dictionary<string, string>> _texts = new List<Dictionary<string, string>>();
        bool _is_error = false;

        protected override void Configure()
        {
            GameObject _mpb_instance = null;
            GameObject _mpb_menu_instance = null;
            marshmallow_PB_MA m;


            _ = InPhase(BuildPhase.Resolving).AfterPlugin("nadena.dev.modular-avatar").Run("genetate marshmallowPB", ctx =>
            {
                m = ctx.AvatarRootObject.GetComponentInChildren<marshmallow_PB_MA>();
                if (m != null)
                {
                    _texts = Localize();

                    m._avatar = ctx.AvatarRootObject;

                    if (m._avatar.transform.Find("marshmallow_PB")) ErrorMessage(m, "_Warning_RemoveOldPB");

                    if (!m._avatar | !m._Breast_L | !m._Breast_R) ErrorMessage(m, "_Error_BreastBoneNotSet");
                    if ((m._version != "2.0") || (!m._presets[m._index])) ErrorMessage(m, "_Error_NotInitialized");

                    GameObject _mpb_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_Prefab_GUID));
                    GameObject _mpb_prefab_menu = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_Prefab_Menu_GUID));
                    if (!_mpb_prefab | !_mpb_prefab_menu) ErrorMessage(m, "_Error_PrefabNotFound");

                    _mpb_instance = GameObject.Instantiate(_mpb_prefab);
                    _mpb_instance.transform.parent = m._avatar.transform;
                    _mpb_instance.name = "marshmallow_PB";

                    _mpb_menu_instance = GameObject.Instantiate(_mpb_prefab_menu);
                    _mpb_menu_instance.transform.parent = m._avatar.transform;
                    _mpb_menu_instance.name = "marshmallow_PB_Menu";
                }
            });

            InPhase(BuildPhase.Resolving).AfterPlugin("nadena.dev.modular-avatar").Run("set marshmallowPB", ctx =>
            {
                // ctx.GetState<GameObject>();
                // var maContext = ctx.Extension<ModularAvatarContext>().BuildContext;
                if (_is_error) return;
                m = ctx.AvatarRootObject.GetComponentInChildren<marshmallow_PB_MA>();
                if (m != null)
                {
                    // MenuオブジェクトはResolvingの段階で処理されるため、分ける
                    SetMarshmallowPBMenu(_mpb_menu_instance, m);
                    if (!m._onlysquish) SetMarshmallowPB(_mpb_instance, m);
                }
            });

            // この間に服ボーンの組み換え、アニメーションの設定が行われる

            InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar").Run("set marshmallowPB", ctx =>
            {
                // ctx.GetState<GameObject>();
                // var maContext = ctx.Extension<ModularAvatarContext>().BuildContext;
                if (_is_error) return;
                m = ctx.AvatarRootObject.GetComponentInChildren<marshmallow_PB_MA>();
                if (m != null)
                {
                    // 元のPBを使用する関係上、服ボーンの組み換え後に行うため、Transformingで設定
                    if (m._onlysquish) SetMarshmallowPB(_mpb_instance, m);
                }
            });
        }

        private void SetMarshmallowPB(GameObject _mpb_instance, marshmallow_PB_MA m)
        {
            // MARK:オブジェクト取得
            GameObject Breast_L = m._Breast_L;
            GameObject Breast_R = m._Breast_R;

            GameObject _mpb_D = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)").gameObject;
            GameObject _mpb_D_L = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_L").gameObject;
            GameObject _mpb_D_R = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_R").gameObject;
            GameObject _mpb_L = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_L/Adjust_Breast_L/Breast_L").gameObject;
            GameObject _mpb_R = _mpb_instance.transform.Find("marshmallow_PB(DummyBone)/marshmallow_PB_R/Adjust_Breast_R/Breast_R").gameObject;

            // PhysBone
            VRCPhysBoneBase PhysBone_L = _mpb_instance.transform.Find("PhysBone_L").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase PhysBone_R = _mpb_instance.transform.Find("PhysBone_R").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Inertia_L = _mpb_instance.transform.Find("Inertia_L").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Inertia_R = _mpb_instance.transform.Find("Inertia_R").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Gravity_L = _mpb_instance.transform.Find("Gravity_L").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Gravity_R = _mpb_instance.transform.Find("Gravity_R").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Buffer_L = _mpb_instance.transform.Find("Buffer_L").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Buffer_R = _mpb_instance.transform.Find("Buffer_R").gameObject.GetComponent<VRCPhysBoneBase>();

            // Collider
            VRCPhysBoneColliderBase Floor_Collider = _mpb_instance.transform.Find("Collider/Floor_Collider").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase PhysBone_Limit_L = _mpb_instance.transform.Find("Collider/PhysBone_Limit_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase PhysBone_Limit_R = _mpb_instance.transform.Find("Collider/PhysBone_Limit_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase PhysBone_Limit_for_Clipping_L = _mpb_instance.transform.Find("Collider/PhysBone_Limit_for_Clipping_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase PhysBone_Limit_for_Clipping_R = _mpb_instance.transform.Find("Collider/PhysBone_Limit_for_Clipping_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Buffer_Limit_L = _mpb_instance.transform.Find("Collider/Buffer_Limit_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Buffer_Limit_R = _mpb_instance.transform.Find("Collider/Buffer_Limit_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Inter_Breast_Collider_L = _mpb_instance.transform.Find("Collider/Inter_Breast_Collider_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Inter_Breast_Collider_R = _mpb_instance.transform.Find("Collider/Inter_Breast_Collider_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Gravity_Limit_L = _mpb_instance.transform.Find("Collider/Gravity_Limit_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Gravity_Limit_R = _mpb_instance.transform.Find("Collider/Gravity_Limit_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Clipping_Limit_Collider_L = _mpb_instance.transform.Find("Collider/Clipping_Limit_Collider_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneColliderBase Clipping_Limit_Collider_R = _mpb_instance.transform.Find("Collider/Clipping_Limit_Collider_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();

            // Constraint
            VRCConstraintBase Position_L = _mpb_instance.transform.Find("Constraint/Position_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Position_R = _mpb_instance.transform.Find("Constraint/Position_R").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Rotation_X_L = _mpb_instance.transform.Find("Constraint/Rotation_X_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Rotation_X_R = _mpb_instance.transform.Find("Constraint/Rotation_X_R").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Rotation_Y_L = _mpb_instance.transform.Find("Constraint/Rotation_Y_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Rotation_Y_R = _mpb_instance.transform.Find("Constraint/Rotation_Y_R").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Rotation_Z_L = _mpb_instance.transform.Find("Constraint/Rotation_Z_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Rotation_Z_R = _mpb_instance.transform.Find("Constraint/Rotation_Z_R").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Scale_L = _mpb_instance.transform.Find("Constraint/Scale_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Scale_R = _mpb_instance.transform.Find("Constraint/Scale_R").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Gravity_Rotation_L = _mpb_instance.transform.Find("Constraint/Gravity_Rotation_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Gravity_Rotation_R = _mpb_instance.transform.Find("Constraint/Gravity_Rotation_R").gameObject.GetComponent<VRCConstraintBase>();
            Transform Target_N = _mpb_instance.transform.Find("Constraint/Gravity_Rotation_Target/Target_N");
            Transform Target_L_Squish = _mpb_instance.transform.Find("Constraint/Gravity_Rotation_Target/Target_L_Squish");
            Transform Target_L_Sag = _mpb_instance.transform.Find("Constraint/Gravity_Rotation_Target/Target_L_Sag");
            Transform Target_R_Squish = _mpb_instance.transform.Find("Constraint/Gravity_Rotation_Target/Target_R_Squish");
            Transform Target_R_Sag = _mpb_instance.transform.Find("Constraint/Gravity_Rotation_Target/Target_R_Sag");
            VRCConstraintBase LookAt_L = _mpb_instance.transform.Find("Constraint/LookAt_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase LookAt_R = _mpb_instance.transform.Find("Constraint/LookAt_R").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Clipping_Limit_L = _mpb_instance.transform.Find("Constraint/Clipping_Limit_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCConstraintBase Clipping_Limit_R = _mpb_instance.transform.Find("Constraint/Clipping_Limit_R").gameObject.GetComponent<VRCConstraintBase>();

            // System
            VRCPhysBoneBase Default_PhysBone_L = _mpb_instance.transform.Find("System/Default_PhysBone_L").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Default_PhysBone_R = _mpb_instance.transform.Find("System/Default_PhysBone_R").gameObject.GetComponent<VRCPhysBoneBase>();
            // Component PhysBone_Target = _mpb_instance.transform.Find("System/PhysBone_Target").gameObject.GetComponent<Component>();
            VRCPhysBoneBase Squish_L = _mpb_instance.transform.Find("System/Squish_L").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Squish_R = _mpb_instance.transform.Find("System/Squish_R").gameObject.GetComponent<VRCPhysBoneBase>();
            ContactSender Breast_Contacts_Sender = _mpb_instance.transform.Find("System/Breast_Contacts").gameObject.GetComponent<ContactSender>();
            ContactReceiver Breast_Contacts_Receiver = _mpb_instance.transform.Find("System/Breast_Contacts").gameObject.GetComponent<ContactReceiver>();
            Transform Scale_for_Contacts = _mpb_instance.transform.Find("System/Scale_for_Contacts");
            Component Interaction_Target = _mpb_instance.transform.Find("System/Interaction_Target").gameObject.GetComponent<Component>();
            ContactSender Interaction_Sender_L = _mpb_instance.transform.Find("System/Interaction_Sender_L").gameObject.GetComponent<ContactSender>();
            ContactSender Interaction_Sender_R = _mpb_instance.transform.Find("System/Interaction_Sender_R").gameObject.GetComponent<ContactSender>();
            ContactReceiver Interaction_Receiver_1_L = _mpb_instance.transform.Find("System/Interaction_Receiver_1_L").gameObject.GetComponent<ContactReceiver>();
            ContactReceiver Interaction_Receiver_1_R = _mpb_instance.transform.Find("System/Interaction_Receiver_1_R").gameObject.GetComponent<ContactReceiver>();
            ContactReceiver Interaction_Receiver_2_L = _mpb_instance.transform.Find("System/Interaction_Receiver_2_L").gameObject.GetComponent<ContactReceiver>();
            ContactReceiver Interaction_Receiver_2_R = _mpb_instance.transform.Find("System/Interaction_Receiver_2_R").gameObject.GetComponent<ContactReceiver>();
            VRCConstraintBase Interaction_Squish_L_Constraint = _mpb_instance.transform.Find("System/Interaction_Squish_L").gameObject.GetComponent<VRCConstraintBase>();
            VRCPhysBoneColliderBase Interaction_Squish_L_Collider = _mpb_instance.transform.Find("System/Interaction_Squish_L").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCConstraintBase Interaction_Squish_R_Constraint = _mpb_instance.transform.Find("System/Interaction_Squish_R").gameObject.GetComponent<VRCConstraintBase>();
            VRCPhysBoneColliderBase Interaction_Squish_R_Collider = _mpb_instance.transform.Find("System/Interaction_Squish_R").gameObject.GetComponent<VRCPhysBoneColliderBase>();
            VRCPhysBoneBase Collision_L = _mpb_instance.transform.Find("System/Collision_L").gameObject.GetComponent<VRCPhysBoneBase>();
            VRCPhysBoneBase Collision_R = _mpb_instance.transform.Find("System/Collision_R").gameObject.GetComponent<VRCPhysBoneBase>();
            // // Menu
            // Transform Menu = _mpb_instance.transform.Find("Menu");
            // ModularAvatarMenuItem MPB_Menu = _mpb_instance.transform.Find("Menu/MPB").gameObject.GetComponent<ModularAvatarMenuItem>();
            // ModularAvatarMenuItem PB_OFF_Menu = _mpb_instance.transform.Find("Menu/MPB/PB_OFF").gameObject.GetComponent<ModularAvatarMenuItem>();
            // ModularAvatarMenuItem Squish_OFF_Menu = _mpb_instance.transform.Find("Menu/MPB/Squish_OFF").gameObject.GetComponent<ModularAvatarMenuItem>();
            // ModularAvatarMenuItem Player_Interaction_ON_Menu = _mpb_instance.transform.Find("Menu/MPB/Player_Interaction_ON").gameObject.GetComponent<ModularAvatarMenuItem>();
            // ModularAvatarMenuItem Limit_for_Clipping_ON_Menu = _mpb_instance.transform.Find("Menu/MPB/Limit_for_Clipping_ON").gameObject.GetComponent<ModularAvatarMenuItem>();


            // MARK:アバターボーン取得
            Animator _animator = m._avatar.GetComponent<Animator>();
            if (!_animator) ErrorMessage(m, "_Error_AnimatorMissing");

            Transform _Chest_transform = null;
            Transform _Hips_transform = null;
            Transform _Armature_transform = null;

            if (_animator.isHuman)
            {
                _Chest_transform = _animator.GetBoneTransform(HumanBodyBones.Chest);
                _Hips_transform = _animator.GetBoneTransform(HumanBodyBones.Hips);
                _Armature_transform = _Hips_transform.parent.gameObject.transform;
            }
            else
            {
                _Chest_transform = m._Breast_L.transform.parent;
            }

            // MARK:PB削除
            if (!m._onlysquish)
            {
                if (m._delete_all_PB)
                {
                    //PB全削除
                    VRCPhysBoneBase[] _avatar_PB_L = Breast_L.gameObject.GetComponentsInChildren<VRCPhysBoneBase>();
                    VRCPhysBoneBase[] _avatar_PB_R = Breast_R.gameObject.GetComponentsInChildren<VRCPhysBoneBase>();

                    foreach (var pb in _avatar_PB_L) { UnityEngine.Object.DestroyImmediate(pb); }
                    foreach (var pb in _avatar_PB_R) { UnityEngine.Object.DestroyImmediate(pb); }
                }
                else
                {
                    //胸ボーンのPBのみ削除
                    VRCPhysBoneBase _avatar_PB_L = Breast_L.gameObject.GetComponent<VRCPhysBoneBase>();
                    VRCPhysBoneBase _avatar_PB_R = Breast_R.gameObject.GetComponent<VRCPhysBoneBase>();

                    UnityEngine.Object.DestroyImmediate(_avatar_PB_L);
                    UnityEngine.Object.DestroyImmediate(_avatar_PB_R);
                }

                //アバター由来のコンストレイントをオフに
                IConstraint _avatar_Const_L = Breast_L.gameObject.GetComponent<IConstraint>();
                IConstraint _avatar_Const_R = Breast_R.gameObject.GetComponent<IConstraint>();
                if (_avatar_Const_L != null) _avatar_Const_L.constraintActive = false;
                if (_avatar_Const_R != null) _avatar_Const_R.constraintActive = false;

                //プリセット指定のPBを削除
                foreach (var path in m._presets[m._index].path_PB)
                {
                    VRCPhysBoneBase temp_PB = CheckPhysBone(m._avatar.transform, path);
                    if (temp_PB) UnityEngine.Object.DestroyImmediate(temp_PB);
                }
            }


            // MARK:インスタンス設定
            _mpb_instance.transform.localPosition = Vector3.zero;
            _mpb_instance.transform.localRotation = quaternion.identity;
            _mpb_instance.transform.localScale = Vector3.one;


            if (m._index != 0 && _Armature_transform) _mpb_instance.transform.localScale = _Armature_transform.localScale;
            if (m._use_transfrom_offset)
            {
                _mpb_instance.transform.localPosition = m.transform.localPosition;
                _mpb_instance.transform.localRotation = m.transform.localRotation;
                _mpb_instance.transform.localScale = m.transform.localScale;
            }


            if (m._presets[m._index].mpb_breast_L_position_0 == Vector3.zero)
                ErrorMessage(m, "_Error_PresetNotConfigured");


            // 胸位置計算用オブジェクト
            GameObject preset_breast_L = new GameObject("preset_breast_L");
            GameObject preset_breast_R = new GameObject("preset_breast_R");

            if (m._index != 0)
            {
                //Transform計算
                var breast_L_position =
                Vector3.Lerp
                (
                    m._presets[m._index].mpb_breast_L_position_0,
                    m._presets[m._index].mpb_breast_L_position_100,
                    m._breast_blendshape * 0.01f
                ) + new Vector3(0f, 1f, 0f);
                if (m._presets[m._index].mpb_breast_L_position_100 == Vector3.zero) breast_L_position = m._presets[m._index].mpb_breast_L_position_0;
                var breast_R_position = MirrorPositionX(breast_L_position);

                var breast_L_rotation =
                Quaternion.Euler(
                    Vector3.Lerp
                    (
                        m._presets[m._index].mpb_breast_L_rotation_0,
                        m._presets[m._index].mpb_breast_L_rotation_100,
                        m._breast_blendshape * 0.01f
                    )
                );
                var breast_R_rotation = MirrorQuaternionX(breast_L_rotation);

                var collider_scale_factor = (0.12f + 0.03f) / (0.12f + m._PhysBone_Collision_Radius);
                var breast_scale =
                Vector3.Lerp
                (
                    m._presets[m._index].mpb_breast_scale_0,
                    m._presets[m._index].mpb_breast_scale_100,
                    m._breast_blendshape * 0.01f
                )
                * m._breast_scale * collider_scale_factor;

                preset_breast_L.transform.localPosition = breast_L_position;
                preset_breast_L.transform.localRotation = breast_L_rotation;
                preset_breast_L.transform.localScale = breast_scale;

                preset_breast_R.transform.localPosition = breast_R_position;
                preset_breast_R.transform.localRotation = breast_R_rotation;
                preset_breast_R.transform.localScale = breast_scale;

                // Chestベース調整の場合
                if (m._is_chestbase)
                {
                    if (_animator.isHuman == false) ErrorMessage(m, "_Error_AnimatorIsHuman");

                    //計算用オブジェクトを生成し、座標を設定
                    GameObject preset_chest = new GameObject("preset_chest");
                    preset_chest.transform.localPosition = m._presets[m._index].avatar_chest_position;
                    preset_chest.transform.localRotation = Quaternion.Euler(m._presets[m._index].avatar_chest_rotation);
                    preset_chest.transform.localScale = m._presets[m._index].avatar_chest_scale;

                    preset_breast_L.transform.parent = preset_chest.transform;
                    preset_breast_R.transform.parent = preset_chest.transform;

                    CopyGrobalTransform(_Chest_transform, preset_chest.transform);

                    preset_breast_L.transform.parent = null;
                    preset_breast_R.transform.parent = null;

                    UnityEngine.Object.DestroyImmediate(preset_chest);
                }
            }
            else
            {
                if (!m.transform.Find("For Unsupported Avatar/marshmallow_PB_L").gameObject.activeSelf)
                    ErrorMessage(m, "_Warning_AdjustColliderPosition");

                GameObject marshmallow_PB_L = m.transform.Find("For Unsupported Avatar/marshmallow_PB_L").gameObject;
                if (!(marshmallow_PB_L.transform.localScale.x == marshmallow_PB_L.transform.localScale.y && marshmallow_PB_L.transform.localScale.x == marshmallow_PB_L.transform.localScale.z))
                    ErrorMessage(m, "_Warning_NonUniformScale");

                GameObject marshmallow_PB_R = new GameObject("marshmallow_PB_R");
                marshmallow_PB_R.transform.parent = marshmallow_PB_L.transform.parent;
                CopyGrobalTransform(marshmallow_PB_L.transform, marshmallow_PB_R.transform);
                marshmallow_PB_R.transform.localPosition = MirrorPositionX(marshmallow_PB_L.transform.localPosition);
                marshmallow_PB_R.transform.localRotation = MirrorQuaternionX(marshmallow_PB_L.transform.localRotation);

                CopyGrobalTransform(marshmallow_PB_L.transform, preset_breast_L.transform);
                CopyGrobalTransform(marshmallow_PB_R.transform, preset_breast_R.transform);


                var collider_scale_factor = (0.12f + 0.03f) / (0.12f + m._PhysBone_Collision_Radius);
                preset_breast_L.transform.localScale *= collider_scale_factor;
                preset_breast_R.transform.localScale *= collider_scale_factor;

                UnityEngine.Object.DestroyImmediate(marshmallow_PB_R);
            }


            CopyGrobalTransform(preset_breast_L.transform, _mpb_D_L.transform);
            CopyGrobalTransform(preset_breast_R.transform, _mpb_D_R.transform);


            UnityEngine.Object.DestroyImmediate(preset_breast_L);
            UnityEngine.Object.DestroyImmediate(preset_breast_R);


            // MARK:ボーン階層移動
            _mpb_D.transform.parent = _Chest_transform;
            _mpb_L.transform.parent = Breast_L.transform.parent;
            _mpb_R.transform.parent = Breast_R.transform.parent;

            _mpb_L.transform.localScale = Vector3.one;
            _mpb_R.transform.localScale = Vector3.one;

            if (!(_Chest_transform == Breast_L.transform.parent && _Chest_transform == Breast_R.transform.parent))
            {
                _mpb_D_L.transform.parent = Breast_L.transform.parent;
                _mpb_D_R.transform.parent = Breast_R.transform.parent;
            }

            //胸ボーン整理
            Breast_L.transform.parent = _mpb_L.transform;
            Breast_R.transform.parent = _mpb_R.transform;

            if (!m._onlysquish)
            {
                //アバター胸ボーン以下のオブジェクトを並列に整理
                int _childcount_L = Breast_L.transform.childCount;
                int _childcount_R = Breast_R.transform.childCount;
                List<Transform> _Breast_L_children = new List<Transform>();
                List<Transform> _Breast_R_children = new List<Transform>();

                for (int i = 0; i < _childcount_L; i++) { _Breast_L_children.Add(Breast_L.transform.GetChild(i).transform); }
                for (int i = 0; i < _childcount_L; i++) { _Breast_L_children[i].parent = _mpb_L.transform; }

                for (int i = 0; i < _childcount_R; i++) { _Breast_R_children.Add(Breast_R.transform.GetChild(i).transform); }
                for (int i = 0; i < _childcount_R; i++) { _Breast_R_children[i].parent = _mpb_R.transform; }
            }

            //ボーン名変更
            _mpb_L.name = Breast_L.name;
            _mpb_R.name = Breast_R.name;

            // MARK:PhysBone設定
            foreach (VRCPhysBoneBase pb in new[] { PhysBone_L, PhysBone_R })
            {
                pb.pull = m._PhysBone_Pull;
                pb.spring = m._PhysBone_Momentum;
                pb.stiffness = m._PhysBone_Stiffness;
                pb.gravity = m._PhysBone_Gravity;
                pb.gravityFalloff = m._PhysBone_GravityFalloff;
                pb.immobile = m._PhysBone_Immobile;
                pb.immobileType = (VRCPhysBoneBase.ImmobileType)m._PhysBone_Immobile_type;

                pb.maxAngleX = m._PhysBone_Limit_Angle;
                // pb.limitRotation = m._PhysBone_Limit_Rotation;
                pb.radius = m._PhysBone_Collision_Radius;
                (pb.allowCollision, pb.collisionFilter) = AllowFilter(m._PhysBone_AllowCollision);
                pb.stretchMotion = m._PhysBone_Stretch_Motion;
                pb.maxStretch = m._PhysBone_Max_Stretch;
                pb.maxSquish = m._PhysBone_Max_Squish;
                (pb.allowGrabbing, pb.grabFilter) = AllowFilter(m._PhysBone_AllowGrabbing);
                (pb.allowPosing, pb.poseFilter) = AllowFilter(m._PhysBone_AllowPosing);
                pb.grabMovement = m._PhysBone_Grab_Movement;
                pb.snapToHand = m._PhysBone_SnapToHand;

                for (int i = 0; i < 10; i++)
                {
                    if (m._PhysBone_collider[i] && pb.colliders.Count > i + 6)
                    {
                        pb.colliders[i + 6] = CheckPhysBoneCollider(m._avatar.transform, GetFullPath(m._PhysBone_collider[i]?.gameObject).Replace(GetFullPath(m._avatar) + "/", ""));
                    }
                }

                if (m._nosquish)
                {
                    pb.stretchMotion = 0f;
                    pb.maxStretch = 0f;
                    pb.maxSquish = 0f;
                }

                if (m._onlysquish)
                {
                    pb.immobile = 1.0f;
                    pb.immobileType = VRCPhysBoneBase.ImmobileType.AllMotion;
                }
            }

            PhysBone_L.limitRotation = m._PhysBone_Limit_Rotation;
            PhysBone_R.limitRotation = MirrorRotationX(m._PhysBone_Limit_Rotation);

            Buffer_L.radius = m._PhysBone_Collision_Radius;
            Buffer_R.radius = m._PhysBone_Collision_Radius;


            // MARK:Inertia設定
            foreach (VRCPhysBoneBase pb in new[] { Inertia_L, Inertia_R })
            {
                pb.enabled = m._inertia_enabled;

                pb.pull = m._inertia_Pull;
                pb.spring = m._inertia_Momentum;
                pb.stiffness = m._inertia_Stiffness;
                pb.gravity = m._inertia_Gravity;
                pb.gravityFalloff = m._inertia_GravityFalloff;
                pb.immobile = m._inertia_Immobile;
                pb.immobileType = (VRCPhysBoneBase.ImmobileType)m._inertia_Immobile_type;

                pb.maxAngleX = m._inertia_LimitAngle;
                // pb.limitRotation = m._inertia_LimitRotation;
                pb.stretchMotion = m._inertia_StretchMotion;
                pb.maxStretch = m._inertia_MaxStretch;
                pb.maxSquish = m._inertia_MaxSquish;

                if (m._onlysquish)
                {
                    pb.immobile = 1.0f;
                    pb.immobileType = VRCPhysBoneBase.ImmobileType.AllMotion;
                }
            }

            Inertia_L.limitRotation = m._inertia_LimitRotation;
            Inertia_R.limitRotation = MirrorRotationX(m._inertia_LimitRotation);

            // MARK:Parallel Bone設定
            foreach (VRCConstraintBase con in new[] { Rotation_X_L, Rotation_X_R })
            {
                VRCConstraintSource source0 = con.Sources[0];
                VRCConstraintSource source1 = con.Sources[1];
                VRCConstraintSource source2 = con.Sources[2];
                if (m._parallelBone_enabled)
                {
                    var squishStrengthX = m._parallelBone_squishStrengthX;
                    if (m._parallelBone_squishStrengthX == 0)
                    {
                        squishStrengthX = 1f;
                        source2.SourceTransform = null;
                    }
                    source0.Weight = (1f - m._parallelBone_strengthX) / squishStrengthX;
                    source1.Weight = m._parallelBone_strengthX / squishStrengthX;
                }
                else
                {
                    source0.Weight = 1f;
                    source1.Weight = 0f;
                    source1.SourceTransform = null;
                    source2.Weight = 0f;
                    source2.SourceTransform = null;
                }
                con.Sources[0] = source0;
                con.Sources[1] = source1;
                con.Sources[2] = source2;
            }

            foreach (VRCConstraintBase con in new[] { Rotation_Y_L, Rotation_Y_R })
            {
                VRCConstraintSource source0 = con.Sources[0];
                VRCConstraintSource source1 = con.Sources[1];
                VRCConstraintSource source2 = con.Sources[2];
                if (m._parallelBone_enabled)
                {
                    var squishStrengthY = m._parallelBone_squishStrengthY;
                    if (m._parallelBone_squishStrengthY == 0)
                    {
                        squishStrengthY = 1f;
                        source2.SourceTransform = null;
                    }
                    source0.Weight = (1f - m._parallelBone_strengthY) / squishStrengthY;
                    source1.Weight = m._parallelBone_strengthY / squishStrengthY;
                }
                else
                {
                    source0.Weight = 1f;
                    source1.Weight = 0f;
                    source1.SourceTransform = null;
                    source2.Weight = 0f;
                    source2.SourceTransform = null;
                }
                con.Sources[0] = source0;
                con.Sources[1] = source1;
                con.Sources[2] = source2;
            }


            // MARK:Gravity
            float gravity_sag = m._gravity_sag;
            float gravity_squish = m._gravity_squish;
            float gravity_squishAngle = m._gravity_squishAngle;
            float gravity_sagAngle = m._gravity_sagAngle;

            if (!m._gravity_enabled)
            {
                gravity_sag = 0f;
                gravity_squish = 0f;
                gravity_squishAngle = 0f;
                gravity_sagAngle = 0f;
            }

            Gravity_Limit_L.position = new Vector3(0f, 0f, Mathf.Lerp(0.12f, 0.1421f, gravity_sag));
            Gravity_Limit_R.position = new Vector3(0f, 0f, Mathf.Lerp(0.12f, 0.1421f, gravity_sag));
            foreach (VRCPhysBoneBase pb in new[] { Gravity_L, Gravity_R })
            {
                pb.maxSquish = Mathf.Lerp(0.1555f, 0.6f, gravity_squish);
                //0.1555fはGravityの必要最小つぶれ
            }

            Target_L_Squish.rotation = Quaternion.Euler(new Vector3(0f, -gravity_squishAngle, 0f));
            Target_R_Squish.rotation = Quaternion.Euler(new Vector3(0f, gravity_squishAngle, 0f));
            Target_L_Sag.rotation = Quaternion.Euler(new Vector3(0f, gravity_sagAngle, 0f));
            Target_R_Sag.rotation = Quaternion.Euler(new Vector3(0f, -gravity_sagAngle, 0f));


            // MARK:Interference / Grab / Squish
            Scale_L.GlobalWeight = m._squishAnimationStrength;
            Scale_R.GlobalWeight = m._squishAnimationStrength;

            Floor_Collider.enabled = m._floor;
            Inter_Breast_Collider_L.enabled = m._interference;
            Inter_Breast_Collider_R.enabled = m._interference;
            Inter_Breast_Collider_L.radius = m._breast_collider_radius;
            Inter_Breast_Collider_R.radius = m._breast_collider_radius;
            Inter_Breast_Collider_L.height = m._breast_collider_radius * 3f;
            Inter_Breast_Collider_R.height = m._breast_collider_radius * 3f;
            Inter_Breast_Collider_L.position = new Vector3(0f, 0f, 0.03f);
            Inter_Breast_Collider_R.position = new Vector3(0f, 0f, 0.03f);

            foreach (VRCPhysBoneBase pb in new[] { Collision_L, Collision_R })
            {
                pb.radius = m._PhysBone_Collision_Radius * 1.05f;
                for (int i = 0; i < 10; i++)
                {
                    if (m._PhysBone_collider[i] && pb.colliders.Count > i)
                    {
                        pb.colliders[i] = CheckPhysBoneCollider(m._avatar.transform, GetFullPath(m._PhysBone_collider[i]?.gameObject).Replace(GetFullPath(m._avatar) + "/", ""));
                    }
                }

            }


            foreach (VRCConstraintBase con in new[] { Scale_L, Scale_R })
            {
                VRCConstraintSource source0 = con.Sources[0];
                var animationStrength = m._breastInterference_AnimationStrength * 2f;
                if (!m._breastInterference_BreakPreventionAnimation)
                {
                    source0.Weight = (animationStrength == 0) ? 100000f : 1 / animationStrength;
                }
                else
                {
                    source0.Weight = 100000f;
                }
                con.Sources[0] = source0;
            }


            Interaction_Squish_L_Collider.enabled = m._playerInteractions;
            Interaction_Squish_R_Collider.enabled = m._playerInteractions;

            Interaction_Sender_L.radius = m._PhysBone_Collision_Radius;
            Interaction_Sender_R.radius = m._PhysBone_Collision_Radius;

            Interaction_Receiver_1_L.radius = m._PhysBone_Collision_Radius;
            Interaction_Receiver_1_R.radius = m._PhysBone_Collision_Radius;
            Interaction_Receiver_1_L.position = new Vector3(0f, 0f, 0.12f);
            Interaction_Receiver_1_R.position = new Vector3(0f, 0f, 0.12f);

            Interaction_Receiver_2_L.radius = m._PhysBone_Collision_Radius * 2f;
            Interaction_Receiver_2_R.radius = m._PhysBone_Collision_Radius * 2f;
            Interaction_Receiver_2_L.position = new Vector3(0f, 0f, 0.12f - m._PhysBone_Collision_Radius);
            Interaction_Receiver_2_R.position = new Vector3(0f, 0f, 0.12f - m._PhysBone_Collision_Radius);

            Interaction_Squish_L_Collider.position = new Vector3(0f, 0f, m._PhysBone_Collision_Radius);
            Interaction_Squish_R_Collider.position = new Vector3(0f, 0f, m._PhysBone_Collision_Radius);
            Interaction_Target.transform.position = new Vector3(0f, 0f, 0.12f * (1f - m._PhysBone_Max_Squish));

            float limit_colider_radius = 0.1f;
            float buffer_limit_colider_position_z = m._buffer_limit_colider_position * 0.12f - m._PhysBone_Collision_Radius - limit_colider_radius;
            Buffer_Limit_L.radius = limit_colider_radius;
            Buffer_Limit_R.radius = limit_colider_radius;
            Buffer_Limit_L.position = new Vector3(0f, 0f, buffer_limit_colider_position_z);
            Buffer_Limit_R.position = new Vector3(0f, 0f, buffer_limit_colider_position_z);

            // MARK:Anti-Penetration
            foreach (VRCConstraintBase con in new[] { Rotation_Y_L, Rotation_Y_R })
            {
                VRCConstraintSource source3 = con.Sources[3];
                if (!m._breastInterference_BreakPreventionRotation)
                {
                    source3.SourceTransform = null;
                    con.Sources[3] = source3;
                }
            }

            PhysBone_Limit_for_Clipping_L.enabled = m._breastInterference_BreakPreventionCollider;
            PhysBone_Limit_for_Clipping_R.enabled = m._breastInterference_BreakPreventionCollider;

            PhysBone_Limit_for_Clipping_L.radius = Mathf.Clamp01(0.08f - m._PhysBone_Collision_Radius);
            PhysBone_Limit_for_Clipping_R.radius = Mathf.Clamp01(0.08f - m._PhysBone_Collision_Radius);

            PhysBone_Limit_for_Clipping_L.position = new Vector3(0.06f, 0f, 0.06f);
            PhysBone_Limit_for_Clipping_R.position = new Vector3(-0.06f, 0f, 0.06f);


            Clipping_Limit_Collider_L.enabled = m._breastInterference_BreakPreventionCollider;
            Clipping_Limit_Collider_R.enabled = m._breastInterference_BreakPreventionCollider;

            Clipping_Limit_Collider_L.radius = m._PhysBone_Collision_Radius * 1.75f;
            Clipping_Limit_Collider_R.radius = m._PhysBone_Collision_Radius * 1.75f;

            Clipping_Limit_Collider_L.position = new Vector3(0f, 0f, -m._PhysBone_Collision_Radius * 0.5f);
            Clipping_Limit_Collider_R.position = new Vector3(0f, 0f, -m._PhysBone_Collision_Radius * 0.5f);

            foreach (VRCPositionConstraintBase pc in new[] { Clipping_Limit_L, Clipping_Limit_R })
            {
                var src = pc.Sources[0].SourceTransform;
                var tr = pc.TargetTransform;

                pc.PositionOffset = -tr.InverseTransformPoint(src.position);

                pc.Locked = true;
                pc.IsActive = true;
            }

            // Default Physbone設定
            foreach (VRCPhysBoneBase pb in new[] { Default_PhysBone_L, Default_PhysBone_R })
            {
                pb.pull = m._PhysBone_Pull;
                pb.spring = m._PhysBone_Momentum;
                pb.stiffness = m._PhysBone_Stiffness;
                pb.gravity = m._PhysBone_Gravity;
                pb.gravityFalloff = m._PhysBone_GravityFalloff;
                pb.immobile = m._PhysBone_Immobile;
                pb.immobileType = (VRCPhysBoneBase.ImmobileType)m._PhysBone_Immobile_type;

                pb.maxAngleX = m._PhysBone_Limit_Angle;
                pb.limitRotation = m._PhysBone_Limit_Rotation;
            }

            UnityEngine.Object.DestroyImmediate(m.gameObject);
        }

        private void SetMarshmallowPBMenu(GameObject _mpb_menu_instance, marshmallow_PB_MA m)
        {
            // Menu
            ModularAvatarMenuInstaller MPB_Installer = _mpb_menu_instance.transform.Find("MPB").gameObject.GetComponent<ModularAvatarMenuInstaller>();
            ModularAvatarMenuItem MPB_Menu = _mpb_menu_instance.transform.Find("MPB").gameObject.GetComponent<ModularAvatarMenuItem>();
            ModularAvatarMenuItem PB_OFF_Menu = _mpb_menu_instance.transform.Find("MPB/PB_OFF").gameObject.GetComponent<ModularAvatarMenuItem>();
            ModularAvatarMenuItem PB_Normal_ON_Menu = _mpb_menu_instance.transform.Find("MPB/PB_Normal_ON").gameObject.GetComponent<ModularAvatarMenuItem>();
            ModularAvatarMenuItem Squish_OFF_Menu = _mpb_menu_instance.transform.Find("MPB/Squish_OFF").gameObject.GetComponent<ModularAvatarMenuItem>();
            ModularAvatarMenuItem Player_Interaction_ON_Menu = _mpb_menu_instance.transform.Find("MPB/Player_Interaction_ON").gameObject.GetComponent<ModularAvatarMenuItem>();
            ModularAvatarMenuItem Limit_for_Clipping_ON_Menu = _mpb_menu_instance.transform.Find("MPB/Limit_for_Clipping_ON").gameObject.GetComponent<ModularAvatarMenuItem>();

            // MARK:Toggle
            if (m._marshmallowPBEnabled)
            {
                MPB_Menu.name = _texts[m._lang]["_Menu_Title"];
                PB_OFF_Menu.name = _texts[m._lang]["_Menu_PB_OFF"];
                PB_Normal_ON_Menu.name = _texts[m._lang]["_Menu_PB_Normal_ON"];
                Squish_OFF_Menu.name = _texts[m._lang]["_Menu_Squish_OFF"];
                Limit_for_Clipping_ON_Menu.name = _texts[m._lang]["_Menu_Limit_Clipping_ON"];
                Player_Interaction_ON_Menu.name = _texts[m._lang]["_Menu_Interaction_ON"];

                Player_Interaction_ON_Menu.isDefault = m._playerInteractions;
                Limit_for_Clipping_ON_Menu.isDefault = m._breastInterference_BreakPreventionCollider;

                if (m._installTargetMenu) MPB_Installer.installTargetMenu = m._installTargetMenu;
            }
            else UnityEngine.Object.DestroyImmediate(_mpb_menu_instance.gameObject);
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

        private (VRCPhysBoneBase.AdvancedBool advbool, VRCPhysBoneBase.PermissionFilter filter) AllowFilter(int allowint)
        {
            VRCPhysBoneBase.AdvancedBool advbool = VRCPhysBoneBase.AdvancedBool.True;
            VRCPhysBoneBase.PermissionFilter filter = new() { allowSelf = true, allowOthers = true };

            switch (allowint)
            {
                case 0:
                    advbool = VRCPhysBoneBase.AdvancedBool.False;
                    break;
                case 1:
                    advbool = VRCPhysBoneBase.AdvancedBool.True;
                    break;
                case 2:
                    advbool = VRCPhysBoneBase.AdvancedBool.Other;
                    filter = new() { allowSelf = true, allowOthers = false };
                    break;
                case 3:
                    advbool = VRCPhysBoneBase.AdvancedBool.Other;
                    filter = new() { allowSelf = false, allowOthers = true };
                    break;
            }

            return (advbool, filter);
        }

        Vector3 MirrorPositionX(Vector3 vec) => new Vector3(-vec.x, vec.y, vec.z);
        Vector3 MirrorRotationX(Vector3 vec) => new Vector3(vec.x, -vec.y, -vec.z);
        Quaternion MirrorQuaternionX(Quaternion qua) => new Quaternion(qua.x, -qua.y, qua.z, qua.w);

        void CopyGrobalTransform(Transform source, Transform target)
        {
            // 位置と回転はワールド値をそのままコピー
            target.SetPositionAndRotation(source.position, source.rotation);

            // スケールは parent の影響を打ち消してローカルにセット
            if (target.parent == null)
            {
                target.localScale = source.lossyScale;                 // 親がない場合はそのまま
            }
            else
            {
                Vector3 pScale = target.parent.lossyScale;             // 親のワールドスケール
                target.localScale = new Vector3(
                    source.lossyScale.x / pScale.x,
                    source.lossyScale.y / pScale.y,
                    source.lossyScale.z / pScale.z);
            }
        }

        private void ErrorMessage(marshmallow_PB_MA m, string str)
        {
            EditorUtility.DisplayDialog("Error", _texts[m._lang][str] + "\n(" + _texts[m._lang]["_Message_Exit_Playmode"] + ")", "OK");
            _is_error = true;
            throw new System.Exception(str);
        }
    }
}
#endif