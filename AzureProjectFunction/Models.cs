using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureProjectFunction
{
    public class Project
    {
        public string ID { get; set; } = Guid.NewGuid().ToString("n");
        public string ConsultantID { get; set; }
        public string ClientID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ProjectCreateModel
    {
        public string ConsultantID { get; set; }
        public string ClientID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ProjectUpdateModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ProjectTableEntity: TableEntity
    {
        public string ConsultantID { get; set; }
        public string ClientID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public static class Mappings
    {
        public static ProjectTableEntity ToTableEntity(this Project project)
        {
            return new ProjectTableEntity
            {
                PartitionKey = "PROJECT",
                RowKey = project.ID,
                ConsultantID = project.ConsultantID,
                ClientID = project.ClientID,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            };
        }

        public static Project ToProject(this ProjectTableEntity project)
        {
            return new Project
            {
                ID = project.RowKey,
                ConsultantID = project.ConsultantID,
                ClientID = project.ClientID,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            };
        }
    }
}
