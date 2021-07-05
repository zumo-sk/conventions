namespace Conventions.Domain.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    public static class DataHelpers
    {
        public const string Id = "id";

        public static IDictionary<string, object> ToDictionary(this object model, bool includeId = false)
        {
            var result = new Dictionary<string, object>();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(model))
            {
                var name = descriptor.Name;
                if (includeId == false && string.Equals(name, Id, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var value = descriptor.GetValue(model);
                result.Add(name, value);
            }

            return result;
        }

        public static void CopyTo<TT, T>(this TT source, T dest)
        {
            var sourceProps = typeof(TT).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(T).GetProperties().Where(x => x.CanWrite).ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.FirstOrDefault(x => x.Name == sourceProp.Name);
                    if (p != null)
                    {
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }
            }
        }
    }
}
