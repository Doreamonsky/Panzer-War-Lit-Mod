using System.IO;
using UnityEngine;
using UnityMod;

namespace DebugLogger
{
    public class DebugLogger : IGeneralAddOn
    {
        private string logToDisplay;

        private string logPath = "";

        public void OnFixedUpdate()
        {
        }

        public void OnInitialized()
        {
            logPath = Application.persistentDataPath + "/" + "log.txt";

            Debug.Log(logPath);

            using (var writer = new StreamWriter(Application.persistentDataPath + "/" + "log.txt", true, System.Text.Encoding.UTF8))
            {
                var systemInfo = "\tTitle:当前系统基础信息：\n设备模型：" + SystemInfo.deviceModel + "\n设备名称：" + SystemInfo.deviceName + "\n设备类型：" + SystemInfo.deviceType + "\n设备唯一标识符：" + SystemInfo.deviceUniqueIdentifier + "\n显卡标识符：" + SystemInfo.graphicsDeviceID + "\n显卡设备名称：" + SystemInfo.graphicsDeviceName + "\n显卡厂商：" + SystemInfo.graphicsDeviceVendor + "\n显卡厂商ID:" + SystemInfo.graphicsDeviceVendorID + "\n显卡支持版本:" + SystemInfo.graphicsDeviceVersion + "\n显存（M）：" + SystemInfo.graphicsMemorySize + "\n显卡像素填充率(百万像素/秒)，-1未知填充率：" + SystemInfo.graphicsPixelFillrate + "\n显卡支持Shader层级：" + SystemInfo.graphicsShaderLevel + "\n支持最大图片尺寸：" + SystemInfo.maxTextureSize + "\nnpotSupport：" + SystemInfo.npotSupport + "\n操作系统：" + SystemInfo.operatingSystem + "\nCPU处理核数：" + SystemInfo.processorCount + "\nCPU类型：" + SystemInfo.processorType + "\nsupportedRenderTargetCount：" + SystemInfo.supportedRenderTargetCount + "\nsupports3DTextures：" + SystemInfo.supports3DTextures + "\nsupportsAccelerometer：" + SystemInfo.supportsAccelerometer + "\nsupportsComputeShaders：" + SystemInfo.supportsComputeShaders + "\nsupportsGyroscope：" + SystemInfo.supportsGyroscope + "\nsupportsImageEffects：" + SystemInfo.supportsImageEffects + "\nsupportsInstancing：" + SystemInfo.supportsInstancing + "\nsupportsLocationService：" + SystemInfo.supportsLocationService + "\nsupportsRenderTextures：" + SystemInfo.supportsRenderTextures + "\nsupportsRenderToCubemap：" + SystemInfo.supportsRenderToCubemap + "\nsupportsShadows：" + SystemInfo.supportsShadows + "\nsupportsSparseTextures：" + SystemInfo.supportsSparseTextures + "\nsupportsStencil：" + SystemInfo.supportsStencil + "\nsupportsVertexPrograms：" + SystemInfo.supportsVertexPrograms + "\nsupportsVibration：" + SystemInfo.supportsVibration + "\n内存大小：" + SystemInfo.systemMemorySize;


                writer.WriteLine("----" + systemInfo + "----");

                writer.Close();
            }


            Application.logMessageReceived += (condition, traceBack, logType) =>
            {
                logToDisplay = string.Format("Time:{0}Type:{1} Msg:{2} TraceBack:{3}", System.DateTime.Now, logType, condition, traceBack);

                var writer = new StreamWriter(logPath, true, System.Text.Encoding.UTF8);
                writer.WriteLine(logToDisplay);

                writer.Close();
                writer.Dispose();
            };
        }

        public void OnNewSceneLoaded(string name)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnUpdateGUI()
        {
            GUILayout.Label(logToDisplay);
        }
    }
}
