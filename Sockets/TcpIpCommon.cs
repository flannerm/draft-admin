using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using DraftAdmin.PlayoutCommands;

namespace DraftAdmin.Sockets
{
    public static class TcpIpCommon
    {

        public static string GetMyID()
        {
            string myIDBuilder = "";
            myIDBuilder += System.Environment.MachineName;
            myIDBuilder += ".";
            myIDBuilder += System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

            return myIDBuilder;
        }

        #region Dealing With Command Objects

        public static string SerializeCommandObject(PlayerCommand CommandObject)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerCommand));
            StringWriter StringToSend = new StringWriter();

            try
            {
                serializer.Serialize(StringToSend, CommandObject);

                return StringToSend.ToString();
            }
            finally
            { StringToSend.Close(); }
        }

        public static PlayerCommand DeserializeCommandObject(string CommandObjectString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerCommand));
            PlayerCommand ObjectToReturn = new PlayerCommand();
            StringReader StringToDeserialize = new StringReader(CommandObjectString);

            try
            {
                ObjectToReturn = (PlayerCommand)serializer.Deserialize(StringToDeserialize);
                return ObjectToReturn;
            }
            finally
            { StringToDeserialize.Close(); }

        }

        #endregion

        public static bool WriteToTraceLogSimple(string item2Log)
        {
            // write incoming commands to log
            // can be used for real-time playback...

            string LocalLogPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) +
                              @"\Logs\" + DateTime.Now.ToString("MMddyyyy") + "tracelog.txt";

            //local variable indicating whether log was written sucessfully
            bool wroteLog = false;
            //create the local log directory if it doesn't exist
            if (!Directory.Exists(Path.GetDirectoryName(LocalLogPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LocalLogPath));
            }

            //a local variable used to loop to keep trying to log if we get an
            //file access error
            bool keeptrying = true;
            while (keeptrying)
            {
                try
                {
                    string stringToWrite = "";
                    stringToWrite = "\r\n" + DateTime.Now.ToString("hh:mm:ss.fff tt") + "|" + item2Log.Replace("\r", "").Replace("\n", "");
                    //stringToWrite = "\r\n" +  DateTime.Now.ToLongTimeString() + "|" + item2Log.Replace("\r","").Replace("\n","");
                    //try to append message to log file
                    File.AppendAllText(LocalLogPath, stringToWrite);
                    //if successful then we no longer have to try
                    keeptrying = false;
                    wroteLog = true;
                }
                catch (Exception logerror)
                {
                    if (!logerror.Message.Contains("The process cannot access the file"))
                    {
                        //not scrictly necessary but if we have an error other than file access
                        //stop trying to log send and email and throw the exception
                        // ReSharper disable RedundantAssignment
                        keeptrying = false;
                        // ReSharper restore RedundantAssignment
                        throw;
                    }
                }
            }
            return wroteLog;
        }
    }
}
