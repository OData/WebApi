namespace System.Web.Http
{
    public class ActionAttributeTestController : ApiController
    {
        [HttpGet]
        public void RetriveUsers() { }

        [HttpPost]
        public void AddUsers(int id) { }

        [HttpPut]
        public void UpdateUsers(User user) { }

        [HttpDelete]
        public void RemoveUsers(string name) { }

        [HttpOptions]
        public void Help(int id) { }

        [HttpHead]
        public void Ping(int id) { }

        [HttpPatch]
        public void Update(int id) { }

        [AcceptVerbs("PATCH", "HEAD")]
        [CLSCompliant(false)]
        public void Users(double key) { }

        [ActionName("Deny")]
        public void Reject(int id) { }

        public void Approve(int id) { }

        [NonAction]
        public void NonAction() { }

        public void Options() { }

        public void Head() { }

        public void PatchUsers() { }
    }
}
