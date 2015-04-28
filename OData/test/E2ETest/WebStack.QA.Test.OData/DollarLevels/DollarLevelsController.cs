using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace WebStack.QA.Test.OData.DollarLevels
{
    public class DLManagersController : ODataController
    {
         public DLManagersController()
        {
            if (null == _DLManagers)
            {
                InitDLManagers();
            }
        }

        /// <summary>
        /// static so that the data is shared among requests.
        /// </summary>
        private static List<DLManager> _DLManagers = null;

        private static void InitDLManagers()
        {
            _DLManagers = Enumerable.Range(1, 10).Select(i =>
                        new DLManager
                        {
                            ID = i,
                            Name = "Name" + i,
                        }).ToList();

            for (int i = 1; i < 9; i++)
            {
                _DLManagers[i].Manager = _DLManagers[i + 1];
                _DLManagers[i + 1].DirectReports = new List<DLManager> { _DLManagers[i] };
            }
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IHttpActionResult Get()
        {
            return Ok(_DLManagers.AsQueryable());
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public IHttpActionResult Get(int key)
        {
            return Ok(_DLManagers.Single(e => e.ID == key));
        }
    }
}
