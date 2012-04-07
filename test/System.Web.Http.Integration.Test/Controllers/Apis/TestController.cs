// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http
{
    public class TestController : ApiController
    {
        public User GetUser(int id) { return null; }
        public List<User> GetUsers() { return null; }

        public List<User> GetUsersByName(string name) { return null; }

        [AcceptVerbs("PATCH")]
        public void PutUser(User user) { }

        public User GetUserByNameAndId(string name, int id) { return null; }
        public User GetUserByNameAndAge(string name, int age) { return null; }
        public User GetUserByNameAgeAndSsn(string name, int age, int ssn) { return null; }
        public User GetUserByNameIdAndSsn(string name, int id, int ssn) { return null; }
        public User GetUserByNameAndSsn(string name, int ssn) { return null; }
        public User PostUser(User user) { return null; }
        public User PostUserByNameAndAge(string name, int age) { return null; }
        public User PostUserByName(string name) { return null; }
        public User PostUserByNameAndAddress(string name, UserAddress address) { return null; }
        public User DeleteUserByIdAndOptName(int id, string name = "DefaultName") { return null; }
        public User DeleteUserByIdNameAndAge(int id, string name, int age) { return null; }
    }
}
