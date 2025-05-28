namespace System.Windows.Forms
{
    using System;

    public interface IFeatureSupport
    {
        bool IsPresent(object feature);
        bool IsPresent(object feature, Version minimumVersion);
        Version GetVersionPresent(object feature);
    }
}