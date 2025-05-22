#region assembly System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Drawing.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security.Permissions;

namespace System.Drawing.Design
{

    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public class UITypeEditor
    {
        public virtual bool IsDropDownResizable => false;

        static UITypeEditor()
        {
            Hashtable table = new Hashtable
            {
                [typeof(DateTime)] = "System.ComponentModel.Design.DateTimeEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                [typeof(Array)] = "System.ComponentModel.Design.ArrayEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                [typeof(IList)] = "System.ComponentModel.Design.CollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                [typeof(ICollection)] = "System.ComponentModel.Design.CollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                [typeof(byte[])] = "System.ComponentModel.Design.BinaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                [typeof(Stream)] = "System.ComponentModel.Design.BinaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                [typeof(string[])] = "System.Windows.Forms.Design.StringArrayEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                [typeof(Collection<string>)] = "System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            };
            TypeDescriptor.AddEditorTable(typeof(UITypeEditor), table);
        }

        public UITypeEditor()
        {
        }

        public object EditValue(IServiceProvider provider, object value)
        {
            return EditValue(null, provider, value);
        }

        public virtual object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            return value;
        }

        public UITypeEditorEditStyle GetEditStyle()
        {
            return GetEditStyle(null);
        }

        public bool GetPaintValueSupported()
        {
            return GetPaintValueSupported(null);
        }

        public virtual bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }

        public virtual UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.None;
        }

        //public void PaintValue(object value, Graphics canvas, Rectangle rectangle)
        //{
        //    PaintValue(new PaintValueEventArgs(null, value, canvas, rectangle));
        //}

        public virtual void PaintValue(PaintValueEventArgs e)
        {
        }
    }
}