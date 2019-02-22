using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Linq;

namespace MonteCristo.MongoDB.Framework.Models
{
    public class BaseEntity
    {
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }
        public DateTime Created { get; protected set; } = DateTime.UtcNow.AddHours(7);
        public void CopyPropertiesTo(BaseEntity entity)
        {
            if (entity == null) throw new ArgumentNullException($"Target {nameof(entity)} should be not null when copying properties");

            if (GetType().FullName != entity.GetType().FullName) throw new ArgumentException($"{GetType().FullName} has to the same with {entity.GetType().FullName}");

            var props = GetType().GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(SetCopyAttribute)));

            foreach (var prop in props)
            {
                var value = prop.GetValue(this, null);
                var targetProp = entity.GetType().GetProperty(prop.Name);
                targetProp.SetValue(entity, value);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SetCopyAttribute : Attribute { }
}
