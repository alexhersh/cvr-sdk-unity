﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CognitiveVR;
using System.Text;
using CognitiveVR.External;

namespace CognitiveVR
{
    public static class SensorRecorder
    {
        private static int jsonPart = 1;
        private static Dictionary<string, List<string>> CachedSnapshots = new Dictionary<string, List<string>>();
        private static int currentSensorSnapshots = 0;

        static SensorRecorder()
        {
            Core.OnSendData += Core_OnSendData;
            Core.CheckSessionId();
        }

        public static void RecordDataPoint(string category, float value)
        {
            Core.CheckSessionId();

            if (CachedSnapshots.ContainsKey(category))
            {
                CachedSnapshots[category].Add(GetSensorDataToString(Util.Timestamp(), value));
            }
            else
            {
                CachedSnapshots.Add(category, new List<string>());
                CachedSnapshots[category].Add(GetSensorDataToString(Util.Timestamp(), value));
            }
            currentSensorSnapshots++;
            if (currentSensorSnapshots >= CognitiveVR_Preferences.Instance.SensorSnapshotCount)
            {
                Core_OnSendData();
            }
        }

        private static void Core_OnSendData()
        {
            if (CachedSnapshots.Keys.Count <= 0) { CognitiveVR.Util.logDebug("Sensor.SendData found no data"); return; }

            var sceneSettings = Core.TrackingScene;
            if (sceneSettings == null) { CognitiveVR.Util.logDebug("Sensor.SendData found no SceneKeySettings"); return; }

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("{");
            JsonUtil.SetString("name", Core.UniqueID, sb);
            sb.Append(",");

            if (!string.IsNullOrEmpty(CognitiveVR_Preferences.LobbyId))
            {
                JsonUtil.SetString("lobbyId", CognitiveVR_Preferences.LobbyId, sb);
                sb.Append(",");
            }

            JsonUtil.SetString("sessionid", Core.SessionID, sb);
            sb.Append(",");
            JsonUtil.SetDouble("timestamp", (int)Core.SessionTimeStamp, sb);
            sb.Append(",");
            JsonUtil.SetInt("part", jsonPart, sb);
            sb.Append(",");
            jsonPart++;
            JsonUtil.SetString("formatversion", "1.0", sb);
            sb.Append(",");


            sb.Append("\"data\":[");
            foreach (var k in CachedSnapshots.Keys)
            {
                sb.Append("{");
                JsonUtil.SetString("name", k, sb);
                sb.Append(",");
                sb.Append("\"data\":[");
                foreach (var v in CachedSnapshots[k])
                {
                    sb.Append(v);
                    sb.Append(",");
                }
                if (CachedSnapshots.Values.Count > 0)
                    sb.Remove(sb.Length - 1, 1); //remove last comma from data array
                sb.Append("]");
                sb.Append("}");
                sb.Append(",");
            }
            if (CachedSnapshots.Keys.Count > 0)
            {
                sb.Remove(sb.Length - 1, 1); //remove last comma from sensor object
            }
            sb.Append("]}");

            CachedSnapshots.Clear();
            currentSensorSnapshots = 0;

            string url = Constants.POSTSENSORDATA(sceneSettings.SceneId, sceneSettings.VersionNumber);
            //byte[] outBytes = System.Text.UTF8Encoding.UTF8.GetBytes();
            //CognitiveVR_Manager.Instance.StartCoroutine(CognitiveVR_Manager.Instance.PostJsonRequest(outBytes, url));
            NetworkManager.Post(url, sb.ToString());
        }

        #region json

        static StringBuilder sbdatapoint = new StringBuilder(256);
        //put this into the list of saved sensor data based on the name of the sensor
        private static string GetSensorDataToString(double timestamp, double sensorvalue)
        {
            //TODO test if string concatenation is just faster/less garbage

            sbdatapoint.Length = 0;

            sbdatapoint.Append("[");
            //sb.ConcatDouble(timestamp);
            sbdatapoint.Append(timestamp);
            sbdatapoint.Append(",");
            //sb.ConcatDouble(sensorvalue);
            sbdatapoint.Append(sensorvalue);
            sbdatapoint.Append("]");

            return sbdatapoint.ToString();
        }

        #endregion
    }
}