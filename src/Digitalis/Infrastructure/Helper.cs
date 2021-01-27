using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalis.Infrastructure
{
    public static class Helper
    {
        public static List<string> GetRandomTags()
        {
            List<string> tags = new List<string> { "music", "business", "politics", "domestic", "international", "health" };

            List<string> randomTags = new List<string>();

            for (int i = 0; i < new Random().Next(1, tags.Count + 1); i++)
            {
                string tag = tags.OrderBy(x => Guid.NewGuid()).First();
                randomTags.Add(tag);
                tags.Remove(tag);
            }

            return randomTags;
        }
    }
}
