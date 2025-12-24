using System;
using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2023 wataameya
namespace wataameya.marshmallow_PB
{
    [CreateAssetMenu(menuName = "wataameya/marshmallowPreset_PB")]
    public class marshmallowPreset_PB : ScriptableObject
    {
        public string pb_preset_name = "";
        public float _PhysBone_Pull = 0f;
        public float _PhysBone_Momentum = 0f;
        public float _PhysBone_Stiffness = 0f;
        public float _PhysBone_Gravity = 0f;
        public float _PhysBone_GravityFalloff = 0f;
        public float _PhysBone_Immobile = 0f;
        public int _PhysBone_Immobile_type = 0;
        public float _PhysBone_Stretch_Motion = 0f;
        public float _PhysBone_Max_Stretch = 0f;
        public float _PhysBone_Max_Squish = 0f;

        public bool _inertia_enabled = true;
        public float _inertia_Pull = 0f;
        public float _inertia_Momentum = 0f;
        public float _inertia_Stiffness = 0f;
        public float _inertia_Gravity = 0f;
        public float _inertia_GravityFalloff = 0f;
        public float _inertia_Immobile = 0f;
        public int _inertia_Immobile_type = 0;
        public float _inertia_StretchMotion = 0f;
        public float _inertia_MaxStretch = 0f;
        public float _inertia_MaxSquish = 0f;

        public bool _parallelBone_enabled = true;
        public float _parallelBone_strengthX = 0f;
        public float _parallelBone_strengthY = 0f;
        public float _parallelBone_squishStrengthX = 0f;
        public float _parallelBone_squishStrengthY = 0f;
        public string ConvertPeresettoString()
        {
            float[] pb_array =
            {
                0.20f, // Preset version 2.0
                this._PhysBone_Pull,
                this._PhysBone_Momentum,
                this._PhysBone_Stiffness,
                this._PhysBone_Gravity,
                this._PhysBone_GravityFalloff,
                this._PhysBone_Immobile,
                (float)this._PhysBone_Immobile_type, //int → float
                this._PhysBone_Stretch_Motion,
                this._PhysBone_Max_Stretch,
                this._PhysBone_Max_Squish,
                this._inertia_enabled ? 1.0f : 0.0f, // bool → float
                this._inertia_Pull,
                this._inertia_Momentum,
                this._inertia_Stiffness,
                this._inertia_Gravity,
                this._inertia_GravityFalloff,
                this._inertia_Immobile,
                (float)this._inertia_Immobile_type, //int → float
                this._inertia_StretchMotion,
                this._inertia_MaxStretch,
                this._inertia_MaxSquish,
                this._parallelBone_enabled ? 1.0f : 0.0f, // bool → float
                this._parallelBone_strengthX,
                this._parallelBone_strengthY,
                this._parallelBone_squishStrengthX,
                this._parallelBone_squishStrengthY
            };

            // // float → byte配列
            // byte[] bytes = new byte[pb_array.Length * sizeof(float)];
            // Buffer.BlockCopy(pb_array, 0, bytes, 0, bytes.Length);

            // // byte配列 → Base64文字列
            // string str = Convert.ToBase64String(bytes);

            float scale = 100f; // 小数第2位まで保持

            // 1. float → sbyte配列へスケーリング
            sbyte[] sbyteArray = new sbyte[pb_array.Length];
            for (int i = 0; i < pb_array.Length; i++)
            {
                sbyteArray[i] = (sbyte)Mathf.Round(pb_array[i] * scale);
            }

            // 2. sbyte配列 → byte配列
            byte[] bytes = new byte[sbyteArray.Length * sizeof(sbyte)];
            Buffer.BlockCopy(sbyteArray, 0, bytes, 0, bytes.Length);

            // 3. byte配列 → Base64文字列
            string str = Convert.ToBase64String(bytes);

            return str;
        }

        public string ConvertStringtoPreset(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);
            float scale = 100f; // 小数第2位まで保持
            sbyte[] sbyteArray = new sbyte[bytes.Length / sizeof(sbyte)];
            Buffer.BlockCopy(bytes, 0, sbyteArray, 0, bytes.Length);
            float[] pb_array = new float[sbyteArray.Length];
            for (int i = 0; i < sbyteArray.Length; i++)
                pb_array[i] = (float)sbyteArray[i] / scale;
            // Debug.Log(string.Join(", ", pb_array));

            if (pb_array.Length == 27)
            {
                if (pb_array[0] == 0.20f)
                {
                    this._PhysBone_Pull = pb_array[1];
                    this._PhysBone_Momentum = pb_array[2];
                    this._PhysBone_Stiffness = pb_array[3];
                    this._PhysBone_Gravity = pb_array[4];
                    this._PhysBone_GravityFalloff = pb_array[5];
                    this._PhysBone_Immobile = pb_array[6];
                    this._PhysBone_Immobile_type = (int)pb_array[7];
                    this._PhysBone_Stretch_Motion = pb_array[8];
                    this._PhysBone_Max_Stretch = pb_array[9];
                    this._PhysBone_Max_Squish = pb_array[10];
                    this._inertia_enabled = pb_array[11] == 1.0f;
                    this._inertia_Pull = pb_array[12];
                    this._inertia_Momentum = pb_array[13];
                    this._inertia_Stiffness = pb_array[14];
                    this._inertia_Gravity = pb_array[15];
                    this._inertia_GravityFalloff = pb_array[16];
                    this._inertia_Immobile = pb_array[17];
                    this._inertia_Immobile_type = (int)pb_array[18];
                    this._inertia_StretchMotion = pb_array[19];
                    this._inertia_MaxStretch = pb_array[20];
                    this._inertia_MaxSquish = pb_array[21];
                    this._parallelBone_enabled = pb_array[22] == 1.0f;
                    this._parallelBone_strengthX = pb_array[23];
                    this._parallelBone_strengthY = pb_array[24];
                    this._parallelBone_squishStrengthX = pb_array[25];
                    this._parallelBone_squishStrengthY = pb_array[26];

                    return "Import successful!";
                }
                else
                {
                    return "Error:Preset version mismatch.";
                }
            }
            else
            {
                return "Error:Invalid preset string.";
            }
        }
    }
}