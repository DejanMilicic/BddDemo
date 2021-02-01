using System;
using System.Collections.Generic;
using System.Linq;
using Digitalis.Infrastructure;

namespace Digitalis.Models
{
    public class Entry : Entity
    {
        public List<string> Tags { get; set; }
    }
}
