using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace IPFIXCollector.Workers
{
    public class DataWorker
    {
        private CollectorSettings collectorSettings;

        public DataWorker(CollectorSettings s_collectorSettings)
        {
            collectorSettings = s_collectorSettings;
        }

        public void SaveData(string json, UInt16 templateid)
        {

            if (collectorSettings.SaveDataToFile)
            {
                string folder = collectorSettings.PathToSaveFolder;

                if (collectorSettings.SaveByTemplateID)
                {
                    folder += "/" + templateid;
                }

                if (collectorSettings.SaveByDate)
                {
                    folder += "/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
                }

                if (collectorSettings.SaveByTemplateID || collectorSettings.SaveByDate)
                {
                    folder += "/";
                }


                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                File.AppendAllText(folder + collectorSettings.SaveFileName, json + Environment.NewLine);
            }
        }

        public Formatting GetFormatting()
        {
            if (collectorSettings.IndentedOutput)
            {
                return Formatting.Indented;
            }
            else
            {
                return Formatting.None;
            }
        }

        public IPAddress UInt32ToIPAddress(UInt32 address)
        {
            return new IPAddress(new byte[] {
                (byte)((address>>24) & 0xFF) ,
                (byte)((address>>16) & 0xFF) ,
                (byte)((address>>8)  & 0xFF) ,
                (byte)( address & 0xFF)});
        }


        public object GetDataFromBytes(byte[] data, int type, MessageLogger messageLogger, bool swapStringBytes = false)
        {
            messageLogger.PrintDebug("GetDataFromBytes  len: " + data.Length + "   type: " + type);
            if (type == 0)
            {
                if (data.Length == 4)
                {
                    Array.Reverse(data);
                    Int32 value = BitConverter.ToInt32(data);
                    messageLogger.PrintDebug("INT32 : " + value);
                    return value;
                }
                else if (data.Length == 8)
                {
                    Array.Reverse(data);
                    Int64 value = BitConverter.ToInt64(data);
                    messageLogger.PrintDebug("INT64 : " + value);
                    return value;
                }
                else if (data.Length == 2)
                {
                    Array.Reverse(data);
                    Int16 value = BitConverter.ToInt16(data);
                    messageLogger.PrintDebug("INT16 : " + value);
                    return value;
                }
                else if (data.Length == 1)
                {
                    byte value = data[0];
                    messageLogger.PrintDebug("Byte : " + value);
                    return value;
                }
                else
                {
                    if (collectorSettings.SwapHexBytes)
                    {
                        Array.Reverse(data);
                    }
                    string value = BitConverter.ToString(data).Replace("-", ""); ;
                    messageLogger.PrintDebug("HEX : " + value);
                    return value;
                }
            }
            else if (type == 1)
            {

                if (data.Length == 1)
                {
                    byte value = data[0];
                    messageLogger.PrintDebug("Byte : " + value);
                    return value;
                }
                else if (data.Length < 3)
                {
                    Array.Reverse(data);
                    Int16 value = BitConverter.ToInt16(data);
                    messageLogger.PrintDebug("INT16 : " + value);
                    return value;
                }
                else if (data.Length < 5)
                {
                    Array.Reverse(data);
                    Int32 value = BitConverter.ToInt32(data);
                    messageLogger.PrintDebug("INT32 : " + value);
                    return value;
                }
                else
                {
                    Array.Reverse(data);
                    Int64 value = BitConverter.ToInt64(data);
                    messageLogger.PrintDebug("INT64 : " + value);
                    return value;
                }
            }
            else if (type == 2)
            {
                Array.Reverse(data);
                double value = BitConverter.ToDouble(data);
                messageLogger.PrintDebug("Double : " + value);
                return value;
            }
            else if (type == 3)
            {
                if (swapStringBytes)
                {
                    Array.Reverse(data);
                }
                string value = Encoding.UTF8.GetString(data);
                value = value.TrimEnd('\0');
                messageLogger.PrintDebug("String : " + value);
                return value;
            }
            else if (type == 4)
            {
                Array.Reverse(data);
                UInt32 number = BitConverter.ToUInt32(data);
                string value = UInt32ToIPAddress(number).ToString();
                messageLogger.PrintDebug("IPAddress : " + value);
                return value;
            }
            else if (type == 5)
            {
                if (data.Length < 3)
                {
                    Array.Reverse(data);
                    UInt16 value = BitConverter.ToUInt16(data);
                    messageLogger.PrintDebug("UINT16 : " + value);
                    return value;
                }
                else if (data.Length < 5)
                {
                    Array.Reverse(data);
                    UInt32 value = BitConverter.ToUInt32(data);
                    messageLogger.PrintDebug("UINT32 : " + value);
                    return value;
                }
                else
                {
                    Array.Reverse(data);
                    UInt64 value = BitConverter.ToUInt64(data);
                    messageLogger.PrintDebug("UINT64 : " + value);
                    return value;
                }
            }
            else
            {
                if (collectorSettings.SwapHexBytes)
                {
                    Array.Reverse(data);
                }
                string value = BitConverter.ToString(data).Replace("-", "");
                messageLogger.PrintDebug("HEX : " + value);
                return value;
            }
        }


    }
}
