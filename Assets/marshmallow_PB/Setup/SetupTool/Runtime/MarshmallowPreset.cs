using System;
using System.Linq;
using UnityEngine;

// Copyright (c) 2023 wataameya

namespace wataameya.marshmallow_PB
{
    [Serializable]
    public class MarshmallowPreset : ScriptableObject
    {
        public string avatar_name = "";
        public string path_breast_L = "";
        public string path_breast_R = "";
        public Vector3 mpb_breast_L_position_0 = new Vector3(0f, 0f, 0f);
        public Vector3 mpb_breast_L_position_100 = new Vector3(0f, 0f, 0f);
        public Vector3 mpb_breast_L_rotation_0 = new Vector3(3f, -5f, 0f);
        public Vector3 mpb_breast_L_rotation_100 = new Vector3(3f, -5f, 0f);
        public Vector3 mpb_breast_scale_0 = new Vector3(1f, 1f, 1f);
        public Vector3 mpb_breast_scale_100 = new Vector3(1f, 1f, 1f);
        public float limit_angle_0 = 40f;
        public float limit_angle_100 = 40f;
        public float collision_radius_0 = 4f;
        public float collision_radius_100 = 6f;
        public float limit_collider_radius_0 = 0.135f;
        public float limit_collider_radius_100 = 0.135f;
        public float limit_collider_position_z_0 = 0.135f;
        public float limit_collider_position_z_100 = 0.135f;
        public float breast_collider_radius_0 = 0.1f;
        public float breast_collider_radius_100 = 0.1f;
        public string[] path_collider = new string[10] { "", "", "", "", "", "", "", "", "", "" };
        public string[] path_PB = new string[10] { "", "", "", "", "", "", "", "", "", "" };
    }
}