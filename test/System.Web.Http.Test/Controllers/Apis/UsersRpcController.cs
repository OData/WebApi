// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

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

        public Task<User> EchoUserAsync(string firstName, string lastName)
        {
            return TaskHelpers.FromResult(new User()
            {
                FirstName = firstName,
                LastName = lastName,
            });
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

        public Task DeleteAllUsersAsync()
        {
            return TaskHelpers.Completed();
        }

        public void AddUser([FromBody] User user)
        {
        }

        public Task WrappedTaskReturningMethod()
        {
            return TaskHelpers.FromResult(TaskHelpers.Completed());
        }

        public object TaskAsObjectReturningMethod()
        {
            return TaskHelpers.Completed();
        }
    }
}
