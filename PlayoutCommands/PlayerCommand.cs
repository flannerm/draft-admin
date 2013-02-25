using System;
using System.Collections.Generic;


namespace DraftAdmin.PlayoutCommands
{

    // the player command object is used to pass
    // commands between the PlayoutManager/PreviewManager
    // and the gfxPlayer.

    [Serializable()]

    public class CommandParameter // : ISerializable
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public CommandParameter()
        {
            Name = null;
            Value = null;
        }
        public CommandParameter(string parameterName, string parameterValue)
        {
            Name = parameterName;
            Value = parameterValue;
        }
    }


    public enum CommandType
    {
        Unknown = 0,

        Initialize = 1,

        LoadTemplate = 2,
        ShowPage = 3,
        UpdatePage = 4,
        HidePage = 5,
        GeneratePreview = 6,

        CommandSuccessful = 7,
        CommandFailed = 8,

        Heartbeat = 9,
        ReceiptAcknowledgement = 10,

        GetStatus = 11,
        SetTimeout = 12,

        RequestData = 13,
    }

    public class PlayerCommand
    {

        public string ID { get; set; }
        public string CommandID { get; set; }
        public DateTime Timestamp { get; set; }
        public CommandType Command { get; set; }
        public string TemplateData { get; set; }
        public List<CommandParameter> Parameters { get; set; }

    }
}

