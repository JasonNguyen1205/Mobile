using System;

namespace Mobile.Droid.Fragments.Base
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PermissionsDefinition : Attribute
    {
        public string[] Permissions { get; }

        public PermissionsDefinition(params string[] permissions)
        {
            Permissions = permissions;
        }
    }
}