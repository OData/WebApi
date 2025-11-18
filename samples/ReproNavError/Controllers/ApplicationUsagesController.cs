using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;

namespace ReproNavError.Controllers
{
    public class ApplicationUsagesController : ODataController
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
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_appUsages);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(string key)
        {
            var appUsage = _appUsages.FirstOrDefault(a => a.Id == key);
            if (appUsage == null)
            {
                return NotFound();
            }
            return Ok(appUsage);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult GetKeyCredentials([FromODataUri] string key)
        {
            var appUsage = _appUsages.FirstOrDefault(a => a.Id == key);
            if (appUsage == null)
            {
                return NotFound();
            }
            return Ok(appUsage.KeyCredentials);
        }


        [HttpGet]
        [ODataRoute("ApplicationUsages/{appId}/KeyCredentials/{keyId}")]
        [EnableQuery]
        public IActionResult GetkeyCredentialById([FromODataUri] string appId, [FromODataUri] string keyId)
        {
            var appUsage = _appUsages.FirstOrDefault(a => a.Id == appId);
            if (appUsage == null)
            {
                return NotFound();
            }
            var keyCredential = appUsage.KeyCredentials.FirstOrDefault(k => k.Id == keyId);
            if (keyCredential == null)
            {
                return NotFound();
            }
            return Ok(keyCredential);
        }

    }
}
