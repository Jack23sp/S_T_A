using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpriteShadersURP
{
    /// <summary>
    /// This is used to draw the Material GUI for Uber Shaders.
    /// </summary>
    public class SpriteShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            //Display some information:
            DisplayInformation();

            //Generate a MaterialProperty Dictionary:
            Dictionary<string, MaterialProperty> mpd = new Dictionary<string, MaterialProperty>();
            Dictionary<string, int> mpi = new Dictionary<string, int>();
            for (int i = 0; i < properties.Length; i++)
            {
                MaterialProperty mp = properties[i];
                mpd.Add(mp.name, mp);   //Property
                mpi.Add(mp.name, i);    //Index
            }

            //Display Special Settings for Lit Sprites:
            if (mpd.ContainsKey("_NormalMap"))
            {
                GUILayout.BeginVertical("Helpbox");
                EditorStyles.label.fontStyle = FontStyle.Bold;
                EditorGUILayout.LabelField("Lit Settings");
                EditorStyles.label.fontStyle = FontStyle.Normal;
                GUILayout.Label("");
                materialEditor.ShaderProperty(mpd["_MaskTex"], new GUIContent(mpd["_MaskTex"].displayName));
                materialEditor.ShaderProperty(mpd["_NormalMap"], new GUIContent(mpd["_NormalMap"].displayName));
                materialEditor.ShaderProperty(mpd["_Normal_Strength"], new GUIContent(mpd["_Normal_Strength"].displayName));
                GUILayout.EndVertical();
                GUILayout.Label("");
            }

            //Strong Tint Shaders:
            DisplayShaderFeature(mpd["BOOLEAN_STRONGTINTADDITIVE"], mpi["_StrongTintAdditive_Intensity"], mpi["_StrongTintAdditive_Tint_Color"], ref materialEditor, ref properties, true);
            DisplayShaderFeature(mpd["BOOLEAN_STRONGTINTOVERRIDE"], mpi["_StrongTintOverride_Intensity"], mpi["_StrongTintOverride_Tint_Color"], ref materialEditor, ref properties, true);
            DisplayShaderFeature(mpd["BOOLEAN_INNERGLOW"], mpi["_InnerGlow_Intensity"], mpi["_InnerGlow_Alpha_Threshold"], ref materialEditor, ref properties, true);

            //Effect Shaders:
            int effectFeature = DisplayShaderEnum(mpd["ENUM_EFFECTS"], ref materialEditor, ref properties);
            switch (effectFeature)
            {
                case (1):
                    DisplayShaderFeature(mpd["ENUM_EFFECTS"], mpi["_Frozen_Intensity"], mpi["_Frozen_Shine_Noise_Limit"], ref materialEditor, ref properties, false);
                    break;
                case (2):
                    DisplayShaderFeature(mpd["ENUM_EFFECTS"], mpi["_Hologram_Intensity"], mpi["_Hologram_Screen_Space"], ref materialEditor, ref properties, false);
                    break;
                case (3):
                    DisplayShaderFeature(mpd["ENUM_EFFECTS"], mpi["_HueOverlay_Intensity"], mpi["_HueOverlay_Hue_Shift_Speed"], ref materialEditor, ref properties, false);
                    break;
                case (4):
                    DisplayShaderFeature(mpd["ENUM_EFFECTS"], mpi["_ZoomingRainbow_Intensity"], mpi["_ZoomingRainbow_Animation_Speed"], ref materialEditor, ref properties, false);
                    break;
            }
            GUILayout.EndVertical();

            //Fading Shaders:
            int fadingFeature = DisplayShaderEnum(mpd["ENUM_FADING"], ref materialEditor, ref properties);
            switch (fadingFeature) { 
                case(1):
                    DisplayShaderFeature(mpd["ENUM_FADING"], mpi["_DissolveAlpha_Intensity"], mpi["_DissolveAlpha_Noise_Scale"], ref materialEditor, ref properties, false);
                    break;
                case (2):
                    DisplayShaderFeature(mpd["ENUM_FADING"], mpi["_DissolveAlphaSource_Intensity"], mpi["_DissolveAlphaSource_Noise_Factor"], ref materialEditor, ref properties, false);
                    break;
                case (3):
                    DisplayShaderFeature(mpd["ENUM_FADING"], mpi["_DissolveGlow_Intensity"], mpi["_DissolveGlow_Glow_Width"], ref materialEditor, ref properties, false);
                    break;
                case (4):
                    DisplayShaderFeature(mpd["ENUM_FADING"], mpi["_DissolveGlowSource_Intensity"], mpi["_DissolveGlowSource_Noise_Scale"], ref materialEditor, ref properties, false);
                    break;
            }
            GUILayout.EndVertical();
            GUILayout.Label("");
            GUILayout.Label("");
        }

        public int DisplayShaderEnum(MaterialProperty condition, ref MaterialEditor materialEditor, ref MaterialProperty[] properties)
        {
            if (condition.floatValue > 0.2f)
            {
                GUI.backgroundColor = new Color(1f, 1f, 1f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.45f, 0.45f, 0.45f);
            }
            EditorGUILayout.BeginVertical("Helpbox");
            EditorStyles.label.fontStyle = FontStyle.Bold;
            materialEditor.ShaderProperty(condition, new GUIContent(condition.displayName, "Picks the " + condition.displayName + " feature and recompiles the shader."));
            EditorStyles.label.fontStyle = FontStyle.Normal;

            if (condition.floatValue > 0.2f)
            {
                EditorGUILayout.LabelField("");
            }

            return Mathf.RoundToInt(condition.floatValue);
        }

        public void DisplayShaderFeature(MaterialProperty condition,int firstProperty, int lastProperty, ref MaterialEditor materialEditor, ref MaterialProperty[] properties, bool box)
        {
            if (condition.floatValue > 0.2f)
            {
                GUI.backgroundColor = new Color(1f, 1f ,1f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.45f, 0.45f, 0.45f);
            }
            if (box)
            {
                EditorGUILayout.BeginVertical("Helpbox");
                EditorStyles.label.fontStyle = FontStyle.Bold;
                materialEditor.ShaderProperty(condition, new GUIContent(condition.displayName, "Toggles the " + condition.displayName + " feature and recompiles the shader."));
                EditorStyles.label.fontStyle = FontStyle.Normal;
            }

            if (condition.floatValue > 0.2f)
            {
                if (box)
                {
                    EditorGUILayout.LabelField("");
                }

                for (int i = firstProperty; i <= lastProperty; i++)
                {
                    materialEditor.ShaderProperty(properties[i], new GUIContent(properties[i].displayName.Split(':')[1].Substring(1)));
                }
            }

            if (box)
            {
                EditorGUILayout.EndVertical();
            }
        }

        public void Category(string label)
        {
            EditorStyles.label.fontStyle = FontStyle.Bold;
            TextAnchor originalAnchor = EditorStyles.label.alignment;
            EditorStyles.label.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - " + label + " - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
            EditorStyles.label.fontStyle = FontStyle.Normal;
            EditorStyles.label.alignment = originalAnchor;
        }
        public void CategoryEnd()
        {
            EditorStyles.label.fontStyle = FontStyle.Bold;
            TextAnchor originalAnchor = EditorStyles.label.alignment;
            EditorStyles.label.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
            EditorStyles.label.fontStyle = FontStyle.Normal;
            EditorStyles.label.alignment = originalAnchor;
            EditorGUILayout.LabelField("");
        }

        public void DisplayInformation()
        {
            EditorGUILayout.BeginVertical();
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Uber Shaders:");
            EditorStyles.label.fontStyle = FontStyle.Normal;
            EditorGUILayout.LabelField("They allow you to combine multiple shaders.");
            EditorGUILayout.LabelField("But due to limitations we can't combine every single shader in one.");
            EditorGUILayout.LabelField("I may add additional uber shaders in upcoming updates though.");
            EditorGUILayout.LabelField("Send Requests: ekincantascontact@gmail.com");
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndVertical();
        }
    }
}