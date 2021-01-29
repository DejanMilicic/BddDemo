using System;
using NJsonSchema.Generation;

namespace Digitalis.Infrastructure
{
    internal class CustomSchemaNameGenerator : ISchemaNameGenerator
    {
        public string Generate(Type type)
        {
            return type.FullName.Replace(".", "_");
        }
    }
}