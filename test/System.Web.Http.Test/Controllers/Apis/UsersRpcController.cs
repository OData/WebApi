
namespace System.Web.Http
{
    public class UsersRpcController : ApiController
    {
        public User EchoUser(string firstName, string lastName)
        {
            return new User()
            {
                FirstName = firstName,
                LastName = lastName,
            };
        }

        [Authorize]
        [HttpGet]
        public User AddAdmin(string firstName, string lastName)
        {
            return new User()
            {
                FirstName = firstName,
                LastName = lastName,
            };
        }

        public User RetriveUser(int id)
        {
            return new User()
            {
                LastName = "UserLN" + id,
                FirstName = "UserFN" + id
            };
        }

        public User EchoUserObject(User user)
        {
            return user;
        }

        public User Admin()
        {
            return new User
            {
                FirstName = "Yao",
                LastName = "Huang"
            };
        }

        public void DeleteAllUsers()
        {
        }

        public void AddUser([FromBody] User user)
        {
        }
    }
}
