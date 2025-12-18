using System;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace server.Common.CoreConstans;

public static class CoreConstants
{
    public const string Prefix = "tms"; // URL prefix
    public const string UploadFolder = "Upload"; // Upload subfolder
    public const string ROOTFOLDER = "wwwroot";
    public const string FOLDERCLOUDINARY = "public";
    public class EntityTypeUpload
    {
        public const string User = "user";
        public const string Project = "project";
        public const string Task = "task";
        public const string Comment = "comment";
        public const string Attachment = "attachment";
    }


    /// <summary>
    /// Project status
    /// </summary>
    public enum ProjectStatus
    {
        Pending,
        InProgress,
        Resolved,
        Rejected
    }
}
