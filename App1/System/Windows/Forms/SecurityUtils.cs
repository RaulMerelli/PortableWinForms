namespace System.Windows.Forms
{
    using System;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;
    using System.Security;
    using System.Security.Permissions;

    internal static class SecurityUtils
    {

        private static volatile ReflectionPermission memberAccessPermission = null;
        private static volatile ReflectionPermission restrictedMemberAccessPermission = null;

        private static ReflectionPermission MemberAccessPermission
        {
            get
            {
                if (memberAccessPermission == null)
                {
                    memberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
                }
                return memberAccessPermission;
            }
        }

        private static ReflectionPermission RestrictedMemberAccessPermission
        {
            get
            {
                if (restrictedMemberAccessPermission == null)
                {
                    restrictedMemberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess);
                }
                return restrictedMemberAccessPermission;
            }
        }

        private static void DemandReflectionAccess(Type type)
        {
            try
            {
                MemberAccessPermission.Demand();
            }
            catch (SecurityException)
            {
                DemandGrantSet(type.Assembly);
            }
        }

        [SecuritySafeCritical]
        private static void DemandGrantSet(Assembly assembly)
        {
            //PermissionSet targetGrantSet = assembly.PermissionSet;
            PermissionSet targetGrantSet = new PermissionSet(PermissionState.None);
            targetGrantSet.AddPermission(RestrictedMemberAccessPermission);
            targetGrantSet.Demand();
        }

        private static bool HasReflectionPermission(Type type)
        {
            try
            {
                DemandReflectionAccess(type);
                return true;
            }
            catch (SecurityException)
            {
            }

            return false;
        }


        internal static object SecureCreateInstance(Type type)
        {
            return SecureCreateInstance(type, null, false);
        }


        internal static object SecureCreateInstance(Type type, object[] args, bool allowNonPublic)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

            // if it's an internal type, we demand reflection permission.
            if (!type.IsVisible)
            {
                DemandReflectionAccess(type);
            }
            else if (allowNonPublic && !HasReflectionPermission(type))
            {
                // Someone is trying to instantiate a public type in *our* assembly, but does not
                // have full reflection permission. We shouldn't pass BindingFlags.NonPublic in this case.
                // The reason we don't directly demand the permission here is because we don't know whether
                // a public or non-public .ctor will be invoked. We want to allow the public .ctor case to
                // succeed.
                allowNonPublic = false;
            }

            if (allowNonPublic)
            {
                flags |= BindingFlags.NonPublic;
            }

            return Activator.CreateInstance(type, flags, null, args, null);
        }

#if (!Microsoft_NAMESPACE)

        internal static object SecureCreateInstance(Type type, object[] args)
        {
            return SecureCreateInstance(type, args, false);
        }


        internal static object SecureConstructorInvoke(Type type, Type[] argTypes, object[] args, bool allowNonPublic)
        {
            return SecureConstructorInvoke(type, argTypes, args, allowNonPublic, BindingFlags.Default);
        }

        internal static object SecureConstructorInvoke(Type type, Type[] argTypes, object[] args,
                                                       bool allowNonPublic, BindingFlags extraFlags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // if it's an internal type, we demand reflection permission.
            if (!type.IsVisible)
            {
                DemandReflectionAccess(type);
            }
            else if (allowNonPublic && !HasReflectionPermission(type))
            {
                // Someone is trying to invoke a ctor on a public type, but does not
                // have full reflection permission. We shouldn't pass BindingFlags.NonPublic in this case.
                allowNonPublic = false;
            }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | extraFlags;
            if (!allowNonPublic)
            {
                flags &= ~BindingFlags.NonPublic;
            }

            ConstructorInfo ctor = type.GetConstructor(flags, null, argTypes, null);
            if (ctor != null)
            {
                return ctor.Invoke(args);
            }

            return null;
        }

        private static bool GenericArgumentsAreVisible(MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                Type[] parameterTypes = method.GetGenericArguments();
                foreach (Type type in parameterTypes)
                {
                    if (!type.IsVisible)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static object FieldInfoGetValue(FieldInfo field, object target)
        {
            Type type = field.DeclaringType;
            if (type == null)
            {
                // Type is null for Global fields.
                if (!field.IsPublic)
                {
                    DemandGrantSet(field.Module.Assembly);
                }
            }
            else if (!(type != null && type.IsVisible && field.IsPublic))
            {
                DemandReflectionAccess(type);
            }
            return field.GetValue(target);
        }

        internal static object MethodInfoInvoke(MethodInfo method, object target, object[] args)
        {
            Type type = method.DeclaringType;
            if (type == null)
            {
                // Type is null for Global methods. In this case we would need to demand grant set on 
                // the containing assembly for internal methods.
                if (!(method.IsPublic && GenericArgumentsAreVisible(method)))
                {
                    DemandGrantSet(method.Module.Assembly);
                }
            }
            else if (!(type.IsVisible && method.IsPublic && GenericArgumentsAreVisible(method)))
            {
                // this demand is required for internal types in system.dll and its friend assemblies. 
                DemandReflectionAccess(type);
            }
            return method.Invoke(target, args);
        }

        internal static object ConstructorInfoInvoke(ConstructorInfo ctor, object[] args)
        {
            Type type = ctor.DeclaringType;
            if ((type != null) && !(type.IsVisible && ctor.IsPublic))
            {
                DemandReflectionAccess(type);
            }
            return ctor.Invoke(args);
        }

        internal static object ArrayCreateInstance(Type type, int length)
        {
            if (!type.IsVisible)
            {
                DemandReflectionAccess(type);
            }
            return Array.CreateInstance(type, length);
        }
#endif
    }
}