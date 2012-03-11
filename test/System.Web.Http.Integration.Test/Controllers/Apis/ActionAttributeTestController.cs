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

        [AcceptVerbs("PATCH", "HEAD")]
        [CLSCompliant(false)]
        public void Users(double key) { }

        [ActionName("Deny")]
        public void Reject(int id) { }

        public void Approve(int id) { }

        [NonAction]
        public void NonAction() { }
    }
}
