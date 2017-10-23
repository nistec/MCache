using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Caching.Demo.Entities
{
    [Serializable]
    public class EntitySample
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Creation { get; set; }
        public object Value { get; set; }

        public EntitySample()
        {
            Id = 123456;
            Name = "EntitySample";
            Creation = DateTime.Now;
            Value = "object value";
        }

    }
}
