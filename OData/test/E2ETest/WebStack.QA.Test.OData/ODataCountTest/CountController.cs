using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace WebStack.QA.Test.OData.ODataCountTest
{
    public class HeroesController : ODataController
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
        public IHttpActionResult Get()
        {
            return Ok(_db.Heroes);
        }

        [HttpGet]
        [EnableQuery]
        public IHttpActionResult GetWeapons()
        {
            return Ok(_db.Weapons);
        }

        [HttpGet]
        [EnableQuery]
        public IHttpActionResult GetNames()
        {
            var names =  new List<string>
            {
                "Hero1",
                "Hero2"
            };
            return Ok(names);
        }
    }
}
