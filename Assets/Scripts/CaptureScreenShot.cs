using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.IO;
using System;

namespace UnityEngine.Recorder.Examples
{
    [Serializable]
    public class ScreenShotData
    {
        public string name;
        public int width;
        public int height;
    }

    public class CaptureScreenShot : MonoBehaviour
    {
        RecorderController m_RecorderController;

        [SerializeField]
        private ScreenShotData[] screenShotDatas;

        private void Setting(string name, int width, int height)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            m_RecorderController = new RecorderController(controllerSettings);

            var mediaOutputFolder = Path.Combine(Application.dataPath, "../../../", "screenshot");

            // Image
            var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageRecorder.name = name;
            imageRecorder.Enabled = true;
            imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageRecorder.CaptureAlpha = false;

            imageRecorder.OutputFile = Path.Combine(mediaOutputFolder, name + "_" + width + "_" + height + "_") + currentTime;

            imageRecorder.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = width,
                OutputHeight = height,
            };

            // Setup Recording
            controllerSettings.AddRecorderSettings(imageRecorder);
            controllerSettings.SetRecordModeToSingleFrame(0);

        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            { 
                StartCoroutine(Capture());   
            }
        }

        IEnumerator Capture()
        {
            foreach(ScreenShotData data in screenShotDatas)
            {
                Setting(data.name, data.width, data.height);
                m_RecorderController.PrepareRecording();
                m_RecorderController.StartRecording();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}