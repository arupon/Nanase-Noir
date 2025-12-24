using UnityEngine;
using VRC.SDKBase;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using VRC.Dynamics;
using VRC.SDK3.Avatars.ScriptableObjects;
using wataameya.marshmallow_PB;

namespace wataameya.marshmallow_PB.ndmf
{
    public class marshmallow_PB_MA : MonoBehaviour, IEditorOnly
    {
        // Basic Settings
        public GameObject _avatar;
        public int _index = 0;
        public int _previndex = -1;
        public GameObject _Breast_L;
        public GameObject _Breast_R;
        public GameObject _mpb;
        public float _breast_blendshape = 0f;
        public float _prevbreast_blendshape = -1f;
        public float _breast_scale = 1.0f;

        // Presets
        public MarshmallowPreset_ver2[] _presets = new MarshmallowPreset_ver2[0];
        public marshmallowPreset_PB[] _pbpresets = new marshmallowPreset_PB[0];

        // PhysBone Settings
        public int _PhysBone_index = 7;
        public int _prevPhysBone_index = -1;
        public int _PhysBone_Preset = 0;                               // PhysBone プリセット
        public float _PhysBone_Pull = 0.1f;
        public float _PhysBone_Momentum = 0.5f;
        public float _PhysBone_Stiffness = 0.25f;
        public float _PhysBone_Gravity = 0.02f;
        public float _PhysBone_GravityFalloff = 1f;
        public float _PhysBone_Immobile = 0.5f;
        public int _PhysBone_Immobile_type = 0;
        public float _PhysBone_Limit_Angle = 35f;
        public Vector3 _PhysBone_Limit_Rotation = new Vector3(-10f, 0f, 0f);                   // 新規追加: LimitRotation
        public float _PhysBone_Collision_Radius = 0.06f;
        public int _PhysBone_AllowCollision = 1;                  // 新規追加: AllowCollision
        public float _PhysBone_Stretch_Motion = 0.5f;
        public float _PhysBone_Max_Stretch = 0.3f;
        public float _PhysBone_Max_Squish = 0.6f;
        public int _PhysBone_AllowGrabbing = 1;                   // 新規追加: AllowGrabbing
        public int _PhysBone_AllowPosing = 0;                    // 新規追加: AllowPosing
        public float _PhysBone_Grab_Movement = 0.1f;                 // 新規追加: GrabMovement
        public bool _PhysBone_SnapToHand = false;                    // 新規追加: SnapToHand
        public VRCPhysBoneColliderBase[] _PhysBone_collider = new VRCPhysBoneColliderBase[10];

        // Inertia Settings
        public bool _inertia_enabled = true;                        // Inertia有効化
        public float _inertia_Pull = 0.1f;
        public float _inertia_Momentum = 0.5f;
        public float _inertia_Stiffness = 0.25f;
        public float _inertia_Gravity = 0.02f;
        public float _inertia_GravityFalloff = 1f;
        public float _inertia_Immobile = 0.5f;
        public int _inertia_Immobile_type = 0;
        public float _inertia_LimitAngle = 20f;
        public Vector3 _inertia_LimitRotation = Vector3.zero;
        public float _inertia_StretchMotion = 0.5f;
        public float _inertia_MaxStretch = 0.5f;
        public float _inertia_MaxSquish = 0.5f;

        // Parallel Bone Function
        public bool _parallelBone_enabled = true;                   // 平行ボーン機能有効化
        public float _parallelBone_strengthX = 0.5f;                // 平行ボーンの強さ（X）
        public float _parallelBone_strengthY = 0.3f;                 // 平行ボーンの強さ（Y）
        public float _parallelBone_squishStrengthX = 0.5f;          // つぶれ状態での平行ボーンの強さ（X）
        public float _parallelBone_squishStrengthY = 0.5f;           // つぶれ状態での平行ボーンの強さ（Y）

        // Gravity Function
        public bool _gravity_enabled = true;                         // 重力機能有効化
        public float _gravity_squish = 0.8f;                   // 胸のつぶれやすさ(仰向け)
        public float _gravity_sag = 0.8f;                            // 胸の垂れ下がりやすさ
        public float _gravity_squishAngle = 10f;                // 胸のつぶれ角度
        public float _gravity_sagAngle = 10f;                // 胸の垂れ下がり角度

        // Interference / Grab / Squish
        public float _squishAnimationStrength = 0.6f;               // つぶれアニメーションの強さ
        public bool _floor = true;                                  // 床コライダー
        public bool _interference = true;                          // 両胸干渉機能
        public float _breast_collider_radius = 0.05f;    // 両胸干渉コライダーの半径
        public float _breastInterference_AnimationStrength = 1f;   // 両胸干渉アニメーション強さ
        public bool _playerInteractions = false;  // 他プレイヤー胸干渉対象

        // Anti-Penetration Function
        public bool _breastInterference_BreakPreventionRotation = false;   // 両胸干渉防止回転
        public bool _breastInterference_BreakPreventionAnimation = false;   // 両胸干渉防止アニメーション
        public bool _breastInterference_BreakPreventionCollider = false;                  // 貫通防止用コライダー
        // public bool _runBreakPreventionEnabled = false;              // 走り破綻防止機能
        public float _buffer_limit_colider_position = 0.5f;  // 
        public bool _nosquish = false;                              // つぶれ機能オフ

        // Menu Setting
        public bool _marshmallowPBEnabled = true;                   // ましゅまろPBオンオフ機能：PB、つぶれ、胸干渉
        public VRCExpressionsMenu _installTargetMenu = null;                  // インストール先メニュー

        // Advanced Settings
        public bool _delete_all_PB = false;                         // PB全削除
        public bool _use_transfrom_offset = false;                  // オフセット機能
        public bool _onlysquish = false;                            // つぶれ機能のみ適用
        public bool _is_chestbase = true;                            // Chestベース調整
        public int _lang = 0;
        public bool _isOpen_PhysBone = false;
        public bool _isOpen_Collider = false;
        public bool _isOpen_Inertia = false;
        public string _version = "0.0"; // バージョン管理
        public int _lang_number = 100; // ver2.0以前のバージョン検知用、 ver2.0以降では100固定
    }
}
