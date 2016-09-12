using System;
using System.Linq;
using System.Reflection;

namespace Hach.Fusion.FFCO.Business.Extensions
{
    /// <summary>
    /// Allows you to specify the DateTimeKind to a DateTime property so that entity framework can set the right DateTimeKind automatically.
    /// </summary>
    /// <remarks>
    /// This code was found at: http://www.gitshah.com/2015/02/how-to-automatically-set-datetimekind.html and http://stackoverflow.com/questions/4648540/entity-framework-datetime-and-utc
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeKindAttribute : Attribute
    {
        public DateTimeKindAttribute(DateTimeKind kind)
        {
            Kind = kind;
        }

        public DateTimeKind Kind { get; }

        public static void Apply(object entity)
        {
            if (entity == null)
                return;

            //Find all properties that are of type DateTime or DateTime?;
            var properties = entity.GetType().GetProperties()
                .Where(x => x.PropertyType == typeof(DateTime)
                         || x.PropertyType == typeof(DateTime?));

            foreach (var property in properties)
            {
                //Check whether these properties have the DateTimeKindAttribute;
                var attr = property.GetCustomAttribute<DateTimeKindAttribute>();
                if (attr == null)
                    continue;

                var dt = property.PropertyType == typeof(DateTime?)
                    ? (DateTime?)property.GetValue(entity)
                    : (DateTime)property.GetValue(entity);

                if (dt == null)
                    continue;

                //If the value is not null set the appropriate DateTimeKind;
                property.SetValue(entity, DateTime.SpecifyKind(dt.Value, attr.Kind));
            }
        }
    }
}
