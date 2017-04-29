using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace NasaBot.Models
{
    [Serializable]
    [XmlRoot]
    public class Process
    {
        [XmlIgnore]
        public string SenderId { get; set; }

        public List<Step> Steps { get; set; }

        public List<Question> Questions { get; set; }

        public List<ProcessEntity> Entities { get; set; }

        public List<Variable> GlobalVariables { get; set; }

        public bool ShowReceipt { get; set; }

        public bool ShowConfirmation { get; set; }

        public bool IsInProcess { get; set; }

        public bool IsFlow { get; set; }

        public List<Intent> ListedIntents { get; set; }
    }

    [Serializable]
    public class Question
    {
        public string Intent { get; set; }

        public string PaginationIntent { get; set; }

        public bool IsFlow { get; set; }

        public bool ClearValues { get; set; }

        public List<Step> Steps { get; set; }
    }

    [Serializable]
    public class Step
    {
        [XmlElement(ElementName = "Question")]
        public string Question { get; set; }

        [XmlElement(ElementName = "FieldType")]
        public string FieldType { get; set; }

        [XmlElement(ElementName = "FieldName")]
        public string FieldName { get; set; }

        [XmlElement(ElementName = "CustomBinding")]
        public string CustomBinding { get; set; }

        [XmlElement(ElementName = "CustomMapping")]
        public string CustomMapping { get; set; }

        public List<StepInput> StepInputs { get; set; }

        public List<StepButton> StepButtons { get; set; }

        [XmlElement(ElementName = "Value")]
        public string Value { get; set; }

        [XmlElement(ElementName = "ReturnAttachment")]
        public bool ReturnAttachment { get; set; }

        [XmlElement(ElementName = "RequireUserData")]
        public bool RequireUserData { get; set; }

        [XmlElement(ElementName = "ShowQuickReplies")]
        public bool ShowQuickReplies { get; set; }

        [XmlElement(ElementName = "OptionalStep")]
        public bool IsOptional { get; set; }

        [XmlElement(ElementName = "FillInternal")]
        public bool FillInternally { get; set; }
    }

    [Serializable]
    public class ProcessEntity
    {
        [XmlElement(ElementName = "Entity")]
        public string Entity { get; set; }

        [XmlElement(ElementName = "InitialEntity")]
        public bool InitialEntity { get; set; }

        [XmlElement(ElementName = "CancelEntity")]
        public bool CancelEntity { get; set; }

    }

    [Serializable]
    public class StepInput
    {
        [XmlElement(ElementName = "InputFieldIndex")]
        public int InputFieldIndex { get; set; }

        [XmlElement(ElementName = "IsGlobal")]
        public bool IsGlobal { get; set; }
    }

    [Serializable]
    public class StepButton
    {
        [XmlElement(ElementName = "PrefixTitle")]
        public string PrefixTitle { get; set; }

        public string Title { get; set; }

        [XmlElement(ElementName = "IsPostback")]
        public bool IsPostback { get; set; }
    }

    [Serializable]
    public class Variable
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Value")]
        public string Value { get; set; }
    }

    public class Intent
    {
        public string Name { get; set; }
    }
}