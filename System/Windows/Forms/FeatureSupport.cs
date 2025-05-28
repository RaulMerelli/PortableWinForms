namespace System.Windows.Forms
{
    using System.Configuration.Assemblies;
    using System.Diagnostics;
    using System;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    public abstract class FeatureSupport : IFeatureSupport
    {
        public static bool IsPresent(string featureClassName, string featureConstName)
        {
            return IsPresent(featureClassName, featureConstName, new Version(0, 0, 0, 0));
        }

        public static bool IsPresent(string featureClassName, string featureConstName, Version minimumVersion)
        {
            object featureId = null;
            IFeatureSupport featureSupport = null;

            //APPCOMPAT: If Type.GetType() throws, we want to return
            //null to preserve Everett behavior.
            Type c = null;
            try
            {
                c = Type.GetType(featureClassName);
            }
            catch (ArgumentException) { }

            if (c != null)
            {
                FieldInfo fi = c.GetField(featureConstName);

                if (fi != null)
                {
                    featureId = fi.GetValue(null);
                }
            }

            if (featureId != null && typeof(IFeatureSupport).IsAssignableFrom(c))
            {

                featureSupport = (IFeatureSupport)SecurityUtils.SecureCreateInstance(c);

                if (featureSupport != null)
                {
                    return featureSupport.IsPresent(featureId, minimumVersion);
                }
            }
            return false;
        }

        public static Version GetVersionPresent(string featureClassName, string featureConstName)
        {
            object featureId = null;
            IFeatureSupport featureSupport = null;

            //APPCOMPAT: If Type.GetType() throws, we want to return
            //null to preserve Everett behavior.
            Type c = null;
            try
            {
                c = Type.GetType(featureClassName);
            }
            catch (ArgumentException) { }

            if (c != null)
            {
                FieldInfo fi = c.GetField(featureConstName);

                if (fi != null)
                {
                    featureId = fi.GetValue(null);
                }
            }

            if (featureId != null)
            {
                featureSupport = (IFeatureSupport)SecurityUtils.SecureCreateInstance(c);

                if (featureSupport != null)
                {
                    return featureSupport.GetVersionPresent(featureId);
                }
            }
            return null;
        }

        public virtual bool IsPresent(object feature)
        {
            return IsPresent(feature, new Version(0, 0, 0, 0));
        }

        public virtual bool IsPresent(object feature, Version minimumVersion)
        {
            Version ver = GetVersionPresent(feature);

            if (ver != null)
            {
                return ver.CompareTo(minimumVersion) >= 0;
            }
            return false;
        }

        public abstract Version GetVersionPresent(object feature);
    }
}