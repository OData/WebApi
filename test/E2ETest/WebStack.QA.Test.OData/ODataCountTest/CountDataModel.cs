using System.Collections.Generic;

namespace WebStack.QA.Test.OData.ODataCountTest
{
    public class Hero
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Weapon> Weapons { get; set; }
    }

    public class Weapon
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
