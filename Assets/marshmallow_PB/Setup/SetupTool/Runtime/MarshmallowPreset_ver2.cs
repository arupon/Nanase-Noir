using System;
using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2023 wataameya
namespace wataameya.marshmallow_PB
{
    [CreateAssetMenu(menuName = "wataameya/MarshmallowPreset_ver2")]
    public class MarshmallowPreset_ver2 : ScriptableObject
    {
        public string avatar_name = "";
        //[HideInInspector]
        public string path_breast_L = "";
        //[HideInInspector]
        public string path_breast_R = "";
        //[HideInInspector]
        public Vector3 avatar_chest_position = new Vector3(0f, 0f, 0f);
        //[HideInInspector]
        public Vector3 avatar_chest_rotation = new Vector3(0f, 0f, 0f);
        //[HideInInspector]
        public Vector3 avatar_chest_scale = new Vector3(1f, 1f, 1f);
        //[HideInInspector]
        public Vector3 mpb_breast_L_position_0 = new Vector3(0f, 0f, 0f);
        //[HideInInspector]
        public Vector3 mpb_breast_L_rotation_0 = new Vector3(3f, -5f, 0f);
        //[HideInInspector]
        public Vector3 mpb_breast_scale_0 = new Vector3(1f, 1f, 1f);
        //[HideInInspector]
        public Vector3 mpb_breast_L_position_100 = new Vector3(0f, 0f, 0f);
        //[HideInInspector]
        public Vector3 mpb_breast_L_rotation_100 = new Vector3(3f, -5f, 0f);
        //[HideInInspector]
        public Vector3 mpb_breast_scale_100 = new Vector3(1f, 1f, 1f);
        //[HideInInspector]
        public float limit_angle_0 = 30f;
        //[HideInInspector]
        public float limit_angle_100 = 30f;
        //[HideInInspector]
        public float physbone_collision_radius_0 = 0.04f;
        //[HideInInspector]
        public float physbone_collision_radius_100 = 0.06f;
        //[HideInInspector]
        public float breast_collider_radius_0 = 0.05f;
        //[HideInInspector]
        public float breast_collider_radius_100 = 0.05f;
        //[HideInInspector]
        public float buffer_collider_position_0 = 0.75f;
        //[HideInInspector]
        public float buffer_collider_position_100 = 0.5f;
        //[HideInInspector]
        // public float parallelbone_strengthX_0 = 0.5f;
        // //[HideInInspector]
        // public float parallelbone_strengthX_100 = 0.5f;
        // //[HideInInspector]
        // public float parallelbone_strengthY_0 = 0.3f;
        // //[HideInInspector]
        // public float parallelbone_strengthY_100 = 0.3f;
        //[HideInInspector]
        public string[] path_collider = new string[10] { "", "", "", "", "", "", "", "", "", "" };
        //[HideInInspector]
        public string[] path_PB = new string[10] { "", "", "", "", "", "", "", "", "", "" };

        public void CaptureChestFrom(Transform t)
        {
            avatar_chest_position = GetDigitVector3(t.position);
            avatar_chest_rotation = GetDigitVector3(t.eulerAngles);
            avatar_chest_scale = GetDigitVector3(t.lossyScale);
        }
        public void Capture0From(Transform t)
        {
            mpb_breast_L_position_0 = GetDigitVector3(t.localPosition);
            mpb_breast_L_rotation_0 = GetDigitVector3(t.localEulerAngles);
            mpb_breast_scale_0 = GetDigitVector3(t.localScale);
        }
        public void Capture100From(Transform t)
        {
            mpb_breast_L_position_100 = GetDigitVector3(t.localPosition);
            mpb_breast_L_rotation_100 = GetDigitVector3(t.localEulerAngles);
            mpb_breast_scale_100 = GetDigitVector3(t.localScale);
        }
        private static Vector3 GetDigitVector3(Vector3 vec)
        {
            float digit = 1000f;
            vec.x = Mathf.Round(Normalize(vec.x) * digit) / digit;
            vec.y = Mathf.Round(Normalize(vec.y) * digit) / digit;
            vec.z = Mathf.Round(Normalize(vec.z) * digit) / digit;
            return vec;
        }
        private static float Normalize(float angle)
        {
            return (angle > 180f) ? angle - 360f : angle;
        }
    }
}