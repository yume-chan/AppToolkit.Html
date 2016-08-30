using System;

namespace AppToolkit.Html.Interfaces
{
    internal interface PropertyIdentity
    {
        string Name { get; }

        Type OwnerType { get; }
    }

    internal class PropertyIdentity<T> : PropertyIdentity, IEquatable<PropertyIdentity<T>>
    {
        public PropertyIdentity(string name, Type ownerType, T defaultValue)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));

            Name = name;
            OwnerType = ownerType;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public Type OwnerType { get; }

        public T DefaultValue { get; }

        public bool Equals(PropertyIdentity<T> other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (Name != other.Name)
                return false;
            if (OwnerType != other.OwnerType)
                return false;

            return true;
        }

        public override bool Equals(object obj) => Equals(obj as PropertyIdentity<T>);

        public override int GetHashCode() => Name.GetHashCode() ^ OwnerType.GetHashCode();
    }
}
