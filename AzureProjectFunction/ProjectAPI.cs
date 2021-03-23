using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureProjectFunction
{
    public static class ProjectAPI
    {
        [FunctionName("CreateProject")]
        public static async Task<IActionResult> CreateProject(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "project")]
           [Table("projects", Connection = "AzureWebJobsStorage")]
           IAsyncCollector<ProjectTableEntity> projectTable,
           HttpRequest req, ILogger log)
        {
            log.LogInformation("Creating a new project entity.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ProjectCreateModel>(requestBody);

            var project = new Project
            {
                ConsultantID = input.ConsultantID,
                ClientID = input.ClientID,
                Name = input.Name,
                Description = input.Description,
                StartDate = input.StartDate,
                EndDate = input.EndDate
            };

            await projectTable.AddAsync(project.ToTableEntity());

            return new OkObjectResult(project);
        }

        [FunctionName("GetAllProjects")]
        public static async Task<IActionResult> GetProjects(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "project")]
           [Table("projects", Connection = "AzureWebJobsStorage")]
           CloudTable projectTable,
           HttpRequest req, ILogger log)
        {
            log.LogInformation("Getting all projects.");

            var query = new TableQuery<ProjectTableEntity>();
            var segment = await projectTable.ExecuteQuerySegmentedAsync(query, null);

            return new OkObjectResult(segment.Select(Mappings.ToProject));
        }

        [FunctionName("GetProjectByID")]
        public static async Task<IActionResult> GetProjectByID(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "project/{id}")]
            [Table("project", "PROJECT", "{id}", Connection = "AzureWebJobsStorage")]
            ProjectTableEntity project,
            HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Getting project by id.");

            if (project == null)
            {
                log.LogInformation($"Project {id} not found.");
                return new NotFoundResult();
            }

            return new OkObjectResult(project.ToProject());
        }

        [FunctionName("UpdateProject")]
        public static async Task<IActionResult> UpdateProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "project/{id}")]
            [Table("projects", Connection = "AzureWebJobsStorage")]
            CloudTable projectTable,
            HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Updating project.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<ProjectUpdateModel>(requestBody);
            var findOperation = TableOperation.Retrieve<ProjectTableEntity>("PROJECT", id);
            var findResult = await projectTable.ExecuteAsync(findOperation);

            if (findResult == null)
                return new NotFoundResult();

            var existingRow = (ProjectTableEntity)findResult.Result;

            if (!string.IsNullOrEmpty(updated.Name))
                existingRow.Name = updated.Name;
            if (!string.IsNullOrEmpty(updated.Description))
                existingRow.Description = updated.Description;
            if (updated.StartDate != null)
                existingRow.StartDate = updated.StartDate;
            if (updated.EndDate != null)
                existingRow.EndDate = updated.EndDate;

            var replaceOperation = TableOperation.Replace(existingRow);
            await projectTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ToProject());
        }

        [FunctionName("DeleteProject")]
        public static async Task<IActionResult> DeleteProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "project/{id}")]
            [Table("projects", Connection = "AzureWebJobsStorage")]
            CloudTable projectTable,
            HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Deleting project.");

            var deleteOperation = TableOperation.Delete(new TableEntity()
            {
                PartitionKey = "PROJECT",
                RowKey = id,
                ETag = "*"
            });

            try
            {
                var deleteResult = await projectTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {

                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}
