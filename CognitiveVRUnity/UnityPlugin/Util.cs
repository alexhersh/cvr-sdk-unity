using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using CognitiveVR.External;
#if !NETFX_CORE
using System.Globalization;
#endif
using System.Text.RegularExpressions;

namespace CognitiveVR
{
	public class Util
	{
        public static Vector3 vector_zero = new Vector3(0, 0, 0);
        public static Vector3 vector_forward = new Vector3(0, 0, 1);


        private const string LOG_TAG = "com.cvr.cognitivevr: ";
		private static bool sLogEnabled = false;
		private static IDictionary<string, object> sDeviceAndAppInfo = new Dictionary<string, object>();
        internal static IDictionary<string, object> getDeviceAndAppInfo() { return sDeviceAndAppInfo; }
        internal static string getSDKName(string namePrefix) { return namePrefix; }

        private static HashSet<string> sValidCurrencyCodes = new HashSet<string>();
        private static IDictionary<string, HashSet<string>> sCurrencyCodesBySymbol = new Dictionary<string, HashSet<string>>();

		public static void setLogEnabled(bool value)
		{
			sLogEnabled = value;
		}

        public static bool IsLoggingEnabled
        {
            get
            {
                return sLogEnabled;
            }
        }

        public static void logWarning(string msg, UnityEngine.Object context = null)
        {
            if (sLogEnabled)
            {
                if (context != null)
                    Debug.LogWarning(LOG_TAG + msg, context);
                else
                    Debug.LogWarning(LOG_TAG + msg);
            }
        }

        // Internal logging.  These can be enabled by calling Util.setLogEnabled(true)
        public static void logDebug(string msg)
		{
			if (sLogEnabled)
			{
				Debug.Log(LOG_TAG + msg);
			}
		}
		
		public static void logError(Exception e)
		{
			if (sLogEnabled)
			{
				Debug.LogException(e);
			}
		}
		
		public static void logError(string msg)
		{
			if (sLogEnabled)
			{
				Debug.LogError(LOG_TAG + msg);
			}
		}

        static int uniqueId;
        /// <summary>
        /// this should be used instead of System.Guid.NewGuid(). these only need to be unique, not complicated
        /// </summary>
        /// <returns></returns>
        public static string GetUniqueId()
        {
            return uniqueId++.ToString();
        }

        internal static void cacheDeviceAndAppInfo()
        {
            // Clear out any previously set data
            sDeviceAndAppInfo.Clear();

            // Get the rest of the information about the device
            sDeviceAndAppInfo.Add("cvr.app.name", Application.productName);
            sDeviceAndAppInfo.Add("cvr.app.version", Application.version);
            sDeviceAndAppInfo.Add("cvr.unity.version", Application.unityVersion);
            sDeviceAndAppInfo.Add("cvr.device.model", SystemInfo.deviceModel);
            sDeviceAndAppInfo.Add("cvr.device.type", SystemInfo.deviceType.ToString());
            sDeviceAndAppInfo.Add("cvr.device.platform", Application.platform);
            sDeviceAndAppInfo.Add("cvr.device.os", SystemInfo.operatingSystem);
            sDeviceAndAppInfo.Add("cvr.device.graphics.name", SystemInfo.graphicsDeviceName);
            sDeviceAndAppInfo.Add("cvr.device.graphics.type", SystemInfo.graphicsDeviceType.ToString());
            sDeviceAndAppInfo.Add("cvr.device.graphics.vendor", SystemInfo.graphicsDeviceVendor);
            sDeviceAndAppInfo.Add("cvr.device.graphics.version", SystemInfo.graphicsDeviceVersion);
            sDeviceAndAppInfo.Add("cvr.device.graphics.memory", SystemInfo.graphicsMemorySize);
            sDeviceAndAppInfo.Add("cvr.device.processor", SystemInfo.processorType);
            sDeviceAndAppInfo.Add("cvr.device.memory", SystemInfo.systemMemorySize);
#if UNITY_2017_2_OR_NEWER
            sDeviceAndAppInfo.Add("cvr.vr.enabled", UnityEngine.XR.XRSettings.enabled);
            sDeviceAndAppInfo.Add("cvr.vr.display.model", UnityEngine.XR.XRSettings.enabled && UnityEngine.XR.XRDevice.isPresent ? UnityEngine.XR.XRDevice.model : "Not Found"); //vive mvt, vive. mv, oculus rift cv1, acer ah100
            sDeviceAndAppInfo.Add("cvr.vr.display.family", UnityEngine.XR.XRSettings.enabled && UnityEngine.XR.XRDevice.isPresent ? UnityEngine.XR.XRSettings.loadedDeviceName : "Not Found"); //openvr, oculus, windowsmr
#else
            sDeviceAndAppInfo.Add("cvr.vr.enabled", UnityEngine.VR.VRSettings.enabled);
            sDeviceAndAppInfo.Add("cvr.vr.display.model", UnityEngine.VR.VRSettings.enabled && UnityEngine.VR.VRDevice.isPresent ? UnityEngine.VR.VRDevice.model : "Not Found");
            sDeviceAndAppInfo.Add("cvr.vr.display.family", UnityEngine.VR.VRSettings.enabled && UnityEngine.VR.VRDevice.isPresent ? UnityEngine.VR.VRDevice.family : "Not Found");
#endif

        }

        //returns vive/rift/gear/unknown based on hmd model name
        public static string GetSimpleHMDName()
        {
#if UNITY_2017_2_OR_NEWER
            string rawHMDName = UnityEngine.XR.XRDevice.model.ToLower();
#else
            string rawHMDName = UnityEngine.VR.VRDevice.model.ToLower();
#endif
            if (rawHMDName.Contains("vive mv") || rawHMDName.Contains("vive. mv") || rawHMDName.Contains("vive dvt")){ return "vive"; }
            if (rawHMDName.Contains("rift cv1")) { return "rift"; }
            if (rawHMDName.Contains("galaxy note 4") || rawHMDName.Contains("galaxy note 5") || rawHMDName.Contains("galaxy s6")) { return "gear"; }

            return "unknown";
        }

        internal static void cacheCurrencyInfo()
        {
            // Clear out any previously set data
            sValidCurrencyCodes.Clear();
            sCurrencyCodesBySymbol.Clear();
#if !NETFX_CORE
            // Cache a set of valid ISO 4217 currency codes and a map of valid currency symbols to ISO 4217 currency codes
            foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    RegionInfo ri = new RegionInfo(info.Name);
                    sValidCurrencyCodes.Add(ri.ISOCurrencySymbol);

                    if (sCurrencyCodesBySymbol.ContainsKey(ri.CurrencySymbol))
                    {
                        sCurrencyCodesBySymbol[ri.CurrencySymbol].Add(ri.ISOCurrencySymbol);
                    }
                    else
                    {
                        HashSet<string> codes = new HashSet<string>();
                        codes.Add(ri.ISOCurrencySymbol);
                        sCurrencyCodesBySymbol.Add(ri.CurrencySymbol, codes);
                    }
                }
                catch (ArgumentException)
                {
                    // Not a valid culture name.  Ok, move along....
                }
            }
#endif
        }

        // Given an input currency string, return a string that is valid currency string.
        // This can be either a valid ISO 4217 currency code or a currency symbol (e.g., for real currencies),  or simply any other ASCII string (e.g., for virtual currencies)
        // If one cannot be determined, this method returns "unknown"
        public static string getValidCurrencyString(string currency)
        {
#if NETFX_CORE
            return "NETFX_CORE";
#else
            string validCurrencyStr;

            // First check if the string is already a valid ISO 4217 currency code (i.e., it's in the list of known codes)
            if (sValidCurrencyCodes.Contains(currency.ToUpper()))
            {
                // It is, just return it
                validCurrencyStr = currency.ToUpper();
            }
            else
            {
                // Not a valid currency code, is it a currency symbol?
                HashSet<string> possibleCodes;
                if (sCurrencyCodesBySymbol.TryGetValue(currency.ToUpper(), out possibleCodes))
                {
                    // It's a valid symbol

                    // If there is only one associated currency code, use it
                    if (1 == possibleCodes.Count)
                    {
                        using (IEnumerator<string> iter = possibleCodes.GetEnumerator())
                        {
                            iter.MoveNext();
                            validCurrencyStr = iter.Current;
                        }
                    }
                    else
                    {
                        // Ok, more than one code associated with this symbol
                        // We make a best guess as to the actual currency code based on the user's locale.
                        RegionInfo ri = RegionInfo.CurrentRegion;
                        if (possibleCodes.Contains(ri.ISOCurrencySymbol))
                        {
                            // The locale currency is in the list of possible codes
                            // It's pretty likely that this currency symbol refers to the locale currency, so let's assume that
                            // This is not a perfect solution, but it's the best we can do until Google and Amazon start giving us more than just currency symbols
                            validCurrencyStr = ri.ISOCurrencySymbol;
                        }
                        else
                        {
                            // We have no idea which currency this symbol refers to, so just set it to "unknown"
                            validCurrencyStr = "unknown";
                        }
                    }
                }
                else
                {
                    // This is not a known currencyc sy so mbol,it must be a virtual currency
                    // Strip out any non-ASCII haracters
                    validCurrencyStr = Regex.Replace(currency, @"[^\u0000-\u007F]", string.Empty);
                }
            }

            return validCurrencyStr;
#endif
        }

        static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static double lastTime;
        static int lastFrame = -1;

        public static double Timestamp(int frame)
        {
            if (frame == lastFrame)
                return lastTime;
            TimeSpan span = DateTime.UtcNow - epoch;
            lastFrame = frame;
            lastTime = span.TotalSeconds;
            return span.TotalSeconds;
        }

        /// <summary>
		/// Get the Unix timestamp
		/// </summary>
		public static double Timestamp()
		{
			TimeSpan span = DateTime.UtcNow - epoch;
			return span.TotalSeconds;
		}

        internal static void AddPref(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        internal static bool TryGetPrefValue(string key, out string value)
        {
            bool keyFound = false;
            value = default(string);
            if (PlayerPrefs.HasKey(key))
            {
                keyFound = true;
                value = PlayerPrefs.GetString(key);
            }

            return keyFound;
        }

        public static int GetResponseCode(Dictionary<string, string> headers)
        {
            int returnCode = -1;
            if (headers.ContainsKey("STATUS"))
            {
                //Debug.Log(headers["STATUS"]);
                //HTTP/1.1 200 OK
                if (int.TryParse(headers["STATUS"].Substring(9).Remove(3),out returnCode))
                {
                    return returnCode;
                }
            }
            else
            {
                Debug.Log("GetResponseCode could not find a status. Likely url is incorrect");
            }
            return returnCode;
        }
    }

    public static class JsonUtil
    {
        //https://forum.unity3d.com/threads/how-to-load-an-array-with-jsonutility.375735/
        public static T[] GetJsonArray<T>(string json)
        {
            string newJson = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        //used for serializing object manifest data
        [Serializable]
        private class Wrapper<T>
        {
#pragma warning disable 0649
            public T[] array;
#pragma warning restore 0649
        }

        //only used for non-nested builds
        

        /// <returns>"name":["obj","obj","obj"]</returns>
        public static string SetListString(string name, List<string> list)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append("\"");
                builder.Append(list[i]);
                builder.Append("\"");
                builder.Append(",");
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("]");
            return builder.ToString();
        }

        /// <returns>"name":["obj","obj","obj"]</returns>
        public static StringBuilder SetListString(string name, List<string> list, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append("\"");
                builder.Append(list[i]);
                builder.Append("\"");
                builder.Append(",");
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("]");
            return builder;
        }

        /// <returns>"name":[obj,obj,obj]</returns>
        public static string SetListObject<T>(string name, List<T> list)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":{");
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append(list[i].ToString());
                builder.Append(",");
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("}");
            return builder.ToString();
        }

        /// <returns>"name":[obj,obj,obj]</returns>
        public static StringBuilder SetListObject<T>(string name, List<T> list, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":{");
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append(list[i].ToString());
                builder.Append(",");
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("}");
            return builder;
        }

        /// <returns>"name":"stringval"</returns>
        public static string SetString(string name, string stringValue)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            builder.Append("\"");
            builder.Append(stringValue);
            builder.Append("\"");

            return builder.ToString();
        }

        /// <returns>"name":"stringval"</returns>
        public static StringBuilder SetString(string name, string stringValue, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            builder.Append("\"");
            builder.Append(stringValue);
            builder.Append("\"");

            return builder;
        }

        /// <returns>"name":"intValue"</returns>
        public static string SetInt(string name, int intValue)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            builder.Append("\"");
            //builder.Concat(intValue);
            builder.Append(intValue);
            builder.Append("\"");

            return builder.ToString();
        }

        /// <returns>"name":"intValue"</returns>
        public static StringBuilder SetInt(string name, int intValue, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            builder.Append("\"");
            //builder.Concat(intValue);
            builder.Append(intValue);
            builder.Append("\"");

            return builder;
        }

        /// <returns>"name":"floatValue"</returns>
        public static string SetFloat(string name, float floatValue)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            builder.Append("\"");
            //builder.Concat(floatValue);
            builder.Append(floatValue);
            builder.Append("\"");

            return builder.ToString();
        }

        /// <returns>"name":"floatValue"</returns>
        public static StringBuilder SetFloat(string name, float floatValue, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            builder.Append("\"");
            //builder.Concat(floatValue);
            builder.Append(floatValue);
            builder.Append("\"");

            return builder;
        }

        /// <returns>"name":"doubleValue"</returns>
        public static string SetDouble(string name, double doubleValue)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            builder.Append("\"");
            //builder.ConcatDouble(doubleValue);
            builder.Append(doubleValue);
            builder.Append("\"");

            return builder.ToString();
        }

        /// <returns>"name":"doubleValue"</returns>
        public static StringBuilder SetDouble(string name, double doubleValue, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            builder.Append("\"");
            //builder.ConcatDouble(doubleValue);
            builder.Append(doubleValue);
            builder.Append("\"");

            return builder;
        }

        /// <returns>"name":objectValue.ToString()</returns>
        public static string SetObject(string name, object objectValue)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            if (objectValue.GetType() == typeof(bool))
                builder.Append(objectValue.ToString().ToLower());
            else
                builder.Append(objectValue.ToString());

            return builder.ToString();
        }

        /// <returns>"name":objectValue.ToString()</returns>
        public static StringBuilder SetObject(string name, object objectValue, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":");

            if (objectValue.GetType() == typeof(bool))
                builder.Append(objectValue.ToString().ToLower());
            else
                builder.Append(objectValue.ToString());

            return builder;
        }

        /// <returns>"name":[0.1,0.2,0.3]</returns>
        public static string SetVector(string name, float[] pos, bool centimeterLimit = false)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append( name );
            builder.Append("\":[");

            if (centimeterLimit)
            {
                builder.Append(string.Format("{0:0.00}", pos[0]));

                builder.Append(",");
                builder.Append(string.Format("{0:0.00}", pos[1]));

                builder.Append(",");
                builder.Append(string.Format("{0:0.00}", pos[2]));

            }
            else
            {
                //builder.Concat(pos[0]);
                builder.Append(pos[0]);
                builder.Append(",");
                //builder.Concat(pos[1]);
                builder.Append(pos[1]);
                builder.Append(",");
                //builder.Concat(pos[2]);
                builder.Append(pos[2]);
            }

            builder.Append("]");
            return builder.ToString();
        }

        /// <returns>"name":[0.1,0.2,0.3]</returns>
        public static StringBuilder SetVector(string name, float[] pos, StringBuilder builder, bool centimeterLimit = false)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");

            if (centimeterLimit)
            {
                builder.Append(string.Format("{0:0.00}", pos[0]));

                builder.Append(",");
                builder.Append(string.Format("{0:0.00}", pos[1]));

                builder.Append(",");
                builder.Append(string.Format("{0:0.00}", pos[2]));

            }
            else
            {
                //builder.Concat(pos[0]);
                builder.Append(pos[0]);
                builder.Append(",");
                //builder.Concat(pos[1]);
                builder.Append(pos[1]);
                builder.Append(",");
                //builder.Concat(pos[2]);
                builder.Append(pos[2]);
            }

            builder.Append("]");
            return builder;
        }

        /// <returns>"name":[0.1,0.2,0.3]</returns>
        public static string SetVector(string name, Vector3 pos, bool centimeterLimit = false)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");

            if (centimeterLimit)
            {
                builder.Append(string.Format("{0:0.00}", pos.x));

                builder.Append(",");
                builder.Append(string.Format("{0:0.00}", pos.y));

                builder.Append(",");
                builder.Append(string.Format("{0:0.00}", pos.z));

            }
            else
            {
                //builder.Concat(pos.x);
                builder.Append(pos.x);
                builder.Append(",");
                //builder.Concat(pos.y);
                builder.Append(pos.y);
                builder.Append(",");
                //builder.Concat(pos.z);
                builder.Append(pos.z);
            }

            builder.Append("]");
            return builder.ToString();
        }

        /// <returns>"name":[0.1,0.2,0.3]</returns>
        public static StringBuilder SetVector(string name, Vector3 pos, StringBuilder builder, bool centimeterLimit = false)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");

            if (centimeterLimit)
            {
                builder.Append(string.Format("{0:0.00}", pos.x));

                builder.Append(",");
                builder.Append(string.Format("{0:0.00}", pos.y));

                builder.Append(",");
                builder.Append(string.Format("{0:0.00}", pos.z));

            }
            else
            {
                //builder.Concat(pos.x);
                builder.Append(pos.x);
                builder.Append(",");
                //builder.Concat(pos.y);
                builder.Append(pos.y);
                builder.Append(",");
                //builder.Concat(pos.z);
                builder.Append(pos.z);
            }

            builder.Append("]");
            return builder;
        }

        /// <returns>"name":[0.1,0.2,0.3,0.4]</returns>
        public static string SetQuat(string name, Quaternion quat)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");

            //builder.Concat(quat.x);
            builder.Append(quat.x);
            builder.Append(",");
            //builder.Concat(quat.y);
            builder.Append(quat.y);
            builder.Append(",");
            //builder.Concat(quat.z);
            builder.Append(quat.z);
            builder.Append(",");
            //builder.Concat(quat.w);
            builder.Append(quat.w);

            builder.Append("]");
            return builder.ToString();
        }

        /// <returns>"name":[0.1,0.2,0.3,0.4]</returns>
        public static StringBuilder SetQuat(string name, Quaternion quat, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");

            //builder.Concat(quat.x);
            builder.Append(quat.x);
            builder.Append(",");
            //builder.Concat(quat.y);
            builder.Append(quat.y);
            builder.Append(",");
            //builder.Concat(quat.z);
            builder.Append(quat.z);
            builder.Append(",");
            //builder.Concat(quat.w);
            builder.Append(quat.w);

            builder.Append("]");
            return builder;
        }

        /// <returns>"name":[0.1,0.2,0.3,0.4]</returns>
        public static string SetQuat(string name, float[] quat)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");

            //builder.Concat(quat[0]);
            builder.Append(quat[0]);
            builder.Append(",");
            //builder.Concat(quat[1]);
            builder.Append(quat[1]);
            builder.Append(",");
            //builder.Concat(quat[2]);
            builder.Append(quat[2]);
            builder.Append(",");
            //builder.Concat(quat[3]);
            builder.Append(quat[3]);

            builder.Append("]");
            return builder.ToString();
        }

        /// <returns>"name":[0.1,0.2,0.3,0.4]</returns>
        public static StringBuilder SetQuat(string name, float[] quat, StringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\":[");

            //builder.Concat(quat[0]);
            builder.Append(quat[0]);
            builder.Append(",");
            //builder.Concat(quat[1]);
            builder.Append(quat[1]);
            builder.Append(",");
            //builder.Concat(quat[2]);
            builder.Append(quat[2]);
            builder.Append(",");
            //builder.Concat(quat[3]);
            builder.Append(quat[3]);

            builder.Append("]");
            return builder;
        }
    }
}