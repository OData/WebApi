namespace ReproNavError.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;

    public class AppUsageController : ODataController
    {
        private List<ApplicationUsage> _appUsages = new List<ApplicationUsage>
        {
            new ApplicationUsage
            {
                Id = "App1",
                KeyCredentials = new List<KeyCredentialUsage>
                {
                    new KeyCredentialUsage
                    {
                        Id = "Key1",
                        Buckets = new List<StatBucket>
                        {
                            new StatBucket { Id = "Bucket1", Count = 10 },
                            new StatBucket { Id = "Bucket2", Count = 20 },
                        }
                    },
                    new KeyCredentialUsage
                    {
                        Id = "Key2",
                        Buckets = new List<StatBucket>
                        {
                            new StatBucket { Id = "Bucket3", Count = 30 },
                            new StatBucket { Id = "Bucket4", Count = 40 },
                        }
                    }
                }
            },
            new ApplicationUsage
            {
                Id = "App2",
                KeyCredentials = new List<KeyCredentialUsage>
                {
                    new KeyCredentialUsage
                    {
                        Id = "Key3",
                        Buckets = new List<StatBucket>
                        {
                            new StatBucket { Id = "Bucket5", Count = 50 },
                            new StatBucket { Id = "Bucket6", Count = 60 },
                        }
                    }
                }
            }
        };

        [HttpGet]
        [ODataRoute("GetAppUsage()")]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_appUsages);
        }

        [HttpGet]
        [ODataRoute("GetAppUsage()/{appId}")]
        [EnableQuery]
        public IActionResult Get(string appId)
        {
            return Ok(SingleResult.Create(_appUsages.Where(a => a.Id == appId).AsQueryable()));
        }

        [HttpGet]
        [ODataRoute("GetAppUsage()/{appId}/KeyCredentials")]
        public IActionResult GetKeyCredentials(string appId)
        {
            return Ok(_appUsages.FirstOrDefault(a => a.Id == appId)!.KeyCredentials);
        }

        [HttpGet]
        [ODataRoute("GetAppUsage()/{appId}/KeyCredentials/{id}")]
        public IActionResult GetKeyCredentials(string appId, string id)
        {
            return Ok(_appUsages.FirstOrDefault(a => a.Id == appId).KeyCredentials.FirstOrDefault(k => k.Id == id));
        }
    }

}
