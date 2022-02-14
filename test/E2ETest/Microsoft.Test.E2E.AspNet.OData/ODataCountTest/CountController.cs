//-----------------------------------------------------------------------------
// <copyright file="CountController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ODataCountTest
{
    public class HeroesController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private static CountEdmModel.CountContext _db = new CountEdmModel.CountContext();

        static HeroesController()
        {
            if (_db.Heroes.Any())
            {
                return;
            }

            var weapons = Enumerable.Range(1, 5).Select(e => new Weapon
            {
                Id = e,
                Name = "Weapon.No." + e
            }).ToList();

            foreach (var weapon in weapons)
            {
                _db.Weapons.Add(weapon);
            }

            var hero = new Hero
            {
                Id = 1,
                Name = "Hero.No.1",
                Weapons = weapons
            };
            _db.Heroes.Add(hero);

            _db.SaveChanges();
        }

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(_db.Heroes);
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult GetWeapons()
        {
            return Ok(_db.Weapons);
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult GetNames()
        {
            var names =  new List<string>
            {
                "Hero1",
                "Hero2"
            };
            return Ok(names);
        }

#if NETCORE
        public void Dispose()
        {
            // _db.Dispose();
        }
#endif
    }
}
