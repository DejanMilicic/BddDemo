using System.Collections.Generic;
using Digitalis.Infrastructure;

namespace Digitalis.Models
{
    public class User : Entity
    {
        public string Email { get; set; }
        public List<(string, string)> Claims { get; set; } = new List<(string, string)>();
    }
}
