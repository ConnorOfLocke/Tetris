using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class GradientMaker : EditorWindow
{
    static Gradient gradient = new Gradient();
    static int textureWidth = 64;
    static int textureHeight = 64;
    static TextureFormat textureFormat = TextureFormat.RGB24;

    private void OnGUI()
    {
        gradient = EditorGUILayout.GradientField("Gradient", gradient);

        textureWidth = Mathf.Clamp(EditorGUILayout.IntField("Texture Width", textureWidth), 0, 4196);
        textureHeight = Mathf.Clamp(EditorGUILayout.IntField("Texture Height", textureHeight), 0, 4196);
        textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", textureFormat);
        
        if (GUILayout.Button("Bake Texture"))
        {
            Texture2D outputTexture = new Texture2D(textureWidth, textureHeight, textureFormat, true);
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    outputTexture.SetPixel(x, y, gradient.Evaluate((float)y / (float)textureHeight));
                }
            }

            outputTexture.Apply();
            byte[] textureData = outputTexture.EncodeToPNG();
            
            string dirPath = Application.dataPath;
            string curDate = System.DateTime.Now.TimeOfDay.Ticks.ToString();

            string fileName = $"{dirPath}\\gradient_{curDate}.png";
            
            System.IO.File.WriteAllBytes(fileName, textureData);

            AssetDatabase.Refresh();
            Debug.Log("Baked Gradient Texture to " + $"{dirPath}\\gradient_{curDate}.png");

        }
    }

    [MenuItem("Window/GradientMaker")]
    public static void OpenGradientMaker()
    {
        EditorWindow.GetWindow<GradientMaker>();
    }
}
#endif