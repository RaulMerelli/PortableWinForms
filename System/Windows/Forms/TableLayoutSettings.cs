﻿namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Windows.Forms.Layout;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [
     TypeConverter(typeof(TableLayoutSettingsTypeConverter)),
     Serializable
    ]
    public sealed class TableLayoutSettings : LayoutSettings, ISerializable
    {


        static private int[] borderStyleToOffset = {
            /*None = */ 0,
            /*Single = */ 1,
            /*Inset = */ 2,
            /*InsetDouble = */ 3,
            /*Outset = */ 2,
            /*OutsetDouble = */ 3,
            /*OutsetPartial = */ 3
        };
        private TableLayoutPanelCellBorderStyle _borderStyle;
        private TableLayoutSettingsStub _stub;

        // used by TableLayoutSettingsTypeConverter
        internal TableLayoutSettings() : base(null)
        {
            _stub = new TableLayoutSettingsStub();
        }

        internal TableLayoutSettings(IArrangedElement owner) : base(owner) { }

        [SuppressMessage("Microsoft.Performance", "CA1801:AvoidUnusedParameters")]
        internal TableLayoutSettings(SerializationInfo serializationInfo, StreamingContext context) : this()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(this);
            string stringVal = serializationInfo.GetString("SerializedString");

            if (!String.IsNullOrEmpty(stringVal) && converter != null)
            {
                TableLayoutSettings tls = converter.ConvertFromInvariantString(stringVal) as TableLayoutSettings;
                if (tls != null)
                {
                    this.ApplySettings(tls);
                }
            }
        }

        public override LayoutEngine LayoutEngine
        {
            get { return TableLayout.Instance; }
        }

        private TableLayout TableLayout
        {
            get { return (TableLayout)this.LayoutEngine; }
        }


        [DefaultValue(TableLayoutPanelCellBorderStyle.None)]
        internal TableLayoutPanelCellBorderStyle CellBorderStyle
        {
            get { return _borderStyle; }
            set
            {
                //valid values are 0x0 to 0x6
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)TableLayoutPanelCellBorderStyle.None, (int)TableLayoutPanelCellBorderStyle.OutsetPartial))
                {
                    throw new ArgumentException("InvalidArgument");
                }
                _borderStyle = value;
                //set the CellBorderWidth according to the current CellBorderStyle.
                TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(Owner);
                containerInfo.CellBorderWidth = borderStyleToOffset[(int)value];
                LayoutTransaction.DoLayout(Owner, Owner, PropertyNames.CellBorderStyle);
                Debug.Assert(CellBorderStyle == value, "CellBorderStyle should be the same as we set");
            }
        }

        [DefaultValue(0)]
        internal int CellBorderWidth
        {
            get { return TableLayout.GetContainerInfo(Owner).CellBorderWidth; }
        }

        [DefaultValue(0)]
        public int ColumnCount
        {
            get
            {
                TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(Owner);
                return containerInfo.MaxColumns;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ColumnCount", value, "InvalidLowBoundArgumentEx");
                }
                TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(Owner);
                containerInfo.MaxColumns = value;
                LayoutTransaction.DoLayout(Owner, Owner, PropertyNames.Columns);
                Debug.Assert(ColumnCount == value, "the max columns should equal to the value we set it to");

            }
        }

        [DefaultValue(0)]
        public int RowCount
        {
            get
            {
                TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(Owner);
                return containerInfo.MaxRows;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("RowCount", value, "InvalidLowBoundArgumentEx");
                }
                TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(Owner);
                containerInfo.MaxRows = value;
                LayoutTransaction.DoLayout(Owner, Owner, PropertyNames.Rows);
                Debug.Assert(RowCount == value, "the max rows should equal to the value we set it to");

            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableLayoutRowStyleCollection RowStyles
        {
            get
            {
                if (IsStub)
                {
                    return _stub.RowStyles;
                }
                else
                {
                    TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(Owner);
                    return containerInfo.RowStyles;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableLayoutColumnStyleCollection ColumnStyles
        {
            get
            {
                if (IsStub)
                {
                    return _stub.ColumnStyles;
                }
                else
                {
                    TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(Owner);
                    return containerInfo.ColumnStyles;
                }
            }
        }

        [DefaultValue(TableLayoutPanelGrowStyle.AddRows)]
        public TableLayoutPanelGrowStyle GrowStyle
        {
            get
            {
                return TableLayout.GetContainerInfo(Owner).GrowStyle;
            }

            set
            {
                //valid values are 0x0 to 0x2
                if (!ClientUtils.IsEnumValid(value, (int)value, (int)TableLayoutPanelGrowStyle.FixedSize, (int)TableLayoutPanelGrowStyle.AddColumns))
                {
                    throw new ArgumentException("InvalidArgument");
                }

                TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(Owner);
                if (containerInfo.GrowStyle != value)
                {
                    containerInfo.GrowStyle = value;
                    LayoutTransaction.DoLayout(Owner, Owner, PropertyNames.GrowStyle);
                }
            }
        }

        internal bool IsStub
        {
            get
            {
                if (_stub != null)
                {
                    Debug.Assert(_stub.IsValid, "seems like we're still partying on an object that's given over its rows and columns, that's a nono.");
                    return true;
                }
                return false;
            }
        }

        internal void ApplySettings(TableLayoutSettings settings)
        {
            if (settings.IsStub)
            {
                if (!IsStub)
                {
                    // we're the real-live thing here, gotta walk through and touch controls
                    settings._stub.ApplySettings(this);
                }
                else
                {
                    // we're just copying another stub into us, just replace the member
                    _stub = settings._stub;
                }
            }

        }
        #region Extended Properties   
        public int GetColumnSpan(object control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (IsStub)
            {
                return _stub.GetColumnSpan(control);
            }
            else
            {
                IArrangedElement element = LayoutEngine.CastToArrangedElement(control);
                return TableLayout.GetLayoutInfo(element).ColumnSpan;
            }
        }

        public void SetColumnSpan(object control, int value)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException("ColumnSpan", "InvalidArgument");
            }
            if (IsStub)
            {
                _stub.SetColumnSpan(control, value);
            }
            else
            {
                IArrangedElement element = LayoutEngine.CastToArrangedElement(control);
                // LayoutInfo.SetColumnSpan() throws ArgumentException if out of range.
                if (element.Container != null)
                {
                    TableLayout.ClearCachedAssignments(TableLayout.GetContainerInfo(element.Container));
                }
                TableLayout.GetLayoutInfo(element).ColumnSpan = value;
                LayoutTransaction.DoLayout(element.Container, element, PropertyNames.ColumnSpan);
                Debug.Assert(GetColumnSpan(element) == value, "column span should equal to the value we set");
            }

        }

        public int GetRowSpan(object control)
        {
            if (IsStub)
            {
                return _stub.GetRowSpan(control);
            }
            else
            {
                IArrangedElement element = LayoutEngine.CastToArrangedElement(control);
                return TableLayout.GetLayoutInfo(element).RowSpan;
            }
        }

        public void SetRowSpan(object control, int value)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException("RowSpan", "InvalidArgument");
            }
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            if (IsStub)
            {
                _stub.SetRowSpan(control, value);
            }
            else
            {

                IArrangedElement element = LayoutEngine.CastToArrangedElement(control);
                // LayoutInfo.SetColumnSpan() throws ArgumentException if out of range.
                if (element.Container != null)
                {
                    TableLayout.ClearCachedAssignments(TableLayout.GetContainerInfo(element.Container));
                }
                TableLayout.GetLayoutInfo(element).RowSpan = value;
                LayoutTransaction.DoLayout(element.Container, element, PropertyNames.RowSpan);
                Debug.Assert(GetRowSpan(element) == value, "row span should equal to the value we set");
            }

        }

        //get the row position of the element
        [DefaultValue(-1)]
        public int GetRow(object control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (IsStub)
            {
                return _stub.GetRow(control);
            }
            else
            {
                IArrangedElement element = LayoutEngine.CastToArrangedElement(control);
                TableLayout.LayoutInfo layoutInfo = TableLayout.GetLayoutInfo(element);
                return layoutInfo.RowPosition;
            }
        }

        //set the row position of the element
        //if we set the row position to -1, it will automatically switch the control from 
        //absolutely positioned to non-absolutely positioned
        public void SetRow(object control, int row)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (row < -1)
            {
                throw new ArgumentOutOfRangeException("Row", "InvalidArgument");
            }
            SetCellPosition(control, row, -1,  /*rowSpecified=*/true, /*colSpecified=*/false);

        }

        //get the column position of the element
        [DefaultValue(-1)]
        public TableLayoutPanelCellPosition GetCellPosition(object control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            return new TableLayoutPanelCellPosition(GetColumn(control), GetRow(control));
        }

        //get the column position of the element
        [DefaultValue(-1)]
        public void SetCellPosition(object control, TableLayoutPanelCellPosition cellPosition)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            SetCellPosition(control, cellPosition.Row, cellPosition.Column,  /*rowSpecified=*/true, /*colSpecified=*/true);

        }

        //get the column position of the element
        [DefaultValue(-1)]
        public int GetColumn(object control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (IsStub)
            {
                return _stub.GetColumn(control);
            }
            else
            {
                IArrangedElement element = LayoutEngine.CastToArrangedElement(control);
                TableLayout.LayoutInfo layoutInfo = TableLayout.GetLayoutInfo(element);
                return layoutInfo.ColumnPosition;
            }
        }

        //set the column position of the element
        //if we set the column position to -1, it will automatically switch the control from 
        //absolutely positioned to non-absolutely positioned
        public void SetColumn(object control, int column)
        {
            if (column < -1)
            {
                throw new ArgumentException("InvalidArgument");
            }
            if (IsStub)
            {
                _stub.SetColumn(control, column);
            }
            else
            {
                SetCellPosition(control, -1, column,  /*rowSpecified=*/false, /*colSpecified=*/true);
            }
        }

        private void SetCellPosition(object control, int row, int column, bool rowSpecified, bool colSpecified)
        {

            if (IsStub)
            {
                if (colSpecified)
                {
                    _stub.SetColumn(control, column);
                }
                if (rowSpecified)
                {
                    _stub.SetRow(control, row);
                }
            }
            else
            {

                IArrangedElement element = LayoutEngine.CastToArrangedElement(control);
                if (element.Container != null)
                {
                    TableLayout.ClearCachedAssignments(TableLayout.GetContainerInfo(element.Container));
                }
                TableLayout.LayoutInfo layoutInfo = TableLayout.GetLayoutInfo(element);
                if (colSpecified)
                {
                    layoutInfo.ColumnPosition = column;
                }
                if (rowSpecified)
                {
                    layoutInfo.RowPosition = row;
                }
                LayoutTransaction.DoLayout(element.Container, element, PropertyNames.TableIndex);
                Debug.Assert(!colSpecified || GetColumn(element) == column, "column position shoule equal to what we set");
                Debug.Assert(!rowSpecified || GetRow(element) == row, "row position shoule equal to what we set");
            }
        }

        internal IArrangedElement GetControlFromPosition(int column, int row)
        {
            return TableLayout.GetControlFromPosition(Owner, column, row);
        }

        internal TableLayoutPanelCellPosition GetPositionFromControl(IArrangedElement element)
        {
            return TableLayout.GetPositionFromControl(Owner, element);
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(this);
            string stringVal = (converter != null) ? converter.ConvertToInvariantString(this) : null;

            if (!String.IsNullOrEmpty(stringVal))
            {
                si.AddValue("SerializedString", stringVal);
            }
        }

        internal List<ControlInformation> GetControlsInformation()
        {
            if (IsStub)
            {
                return _stub.GetControlsInformation();
            }
            else
            {
                List<ControlInformation> controlsInfo = new List<ControlInformation>(Owner.Children.Count);

                foreach (IArrangedElement element in Owner.Children)
                {
                    Control c = element as Control;
                    if (c != null)
                    {
                        ControlInformation controlInfo = new ControlInformation();

                        // We need to go through the PropertyDescriptor for the Name property
                        // since it is shadowed.
                        PropertyDescriptor prop = TypeDescriptor.GetProperties(c)["Name"];
                        if (prop != null && prop.PropertyType == typeof(string))
                        {
                            controlInfo.Name = prop.GetValue(c);
                        }
                        else
                        {
                            Debug.Fail("Name property missing on control");
                        }

                        controlInfo.Row = GetRow(c);
                        controlInfo.RowSpan = GetRowSpan(c);
                        controlInfo.Column = GetColumn(c);
                        controlInfo.ColumnSpan = GetColumnSpan(c);
                        controlsInfo.Add(controlInfo);
                    }

                }
                return controlsInfo;
            }

        }

        internal struct ControlInformation
        {
            internal object Name;
            internal int Row;
            internal int Column;
            internal int RowSpan;
            internal int ColumnSpan;

            internal ControlInformation(object name, int row, int column, int rowSpan, int columnSpan)
            {
                Name = name;
                Row = row;
                Column = column;
                RowSpan = rowSpan;
                ColumnSpan = columnSpan;
            }
        }

        private class TableLayoutSettingsStub
        {

            private static ControlInformation DefaultControlInfo = new ControlInformation(null, -1, -1, 1, 1);
            private TableLayoutColumnStyleCollection columnStyles;
            private TableLayoutRowStyleCollection rowStyles;
            private Dictionary<object, ControlInformation> controlsInfo;
            private bool isValid = true;

            public TableLayoutSettingsStub()
            {
            }

            ///
            internal void ApplySettings(TableLayoutSettings settings)
            {
                //
                // apply row,column,rowspan,colspan
                //
                TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(settings.Owner);
                Control appliedControl = containerInfo.Container as Control;
                if (appliedControl != null && controlsInfo != null)
                {

                    // we store the control names, look up the controls 
                    // in the appliedControl's control collection and apply the row,column settings.
                    foreach (object controlName in controlsInfo.Keys)
                    {
                        ControlInformation controlInfo = controlsInfo[controlName];

                        // Look for the control in our table, we have to go through
                        // PropertyDescriptor rather than just going using appliedControl.Controls[controlName]
                        // because the Name property is shadowed at design time
                        foreach (Control tableControl in appliedControl.Controls)
                        {
                            if (tableControl != null)
                            {
                                string name = null;
                                PropertyDescriptor prop = TypeDescriptor.GetProperties(tableControl)["Name"];
                                if (prop != null && prop.PropertyType == typeof(string))
                                {
                                    name = prop.GetValue(tableControl) as string;
                                }
                                else
                                {
                                    Debug.Fail("Name property missing on control");
                                }
                                if (WindowsFormsUtils.SafeCompareStrings(name, controlName as string, /* ignoreCase = */ false))
                                {
                                    settings.SetRow(tableControl, controlInfo.Row);
                                    settings.SetColumn(tableControl, controlInfo.Column);
                                    settings.SetRowSpan(tableControl, controlInfo.RowSpan);
                                    settings.SetColumnSpan(tableControl, controlInfo.ColumnSpan);
                                    break;
                                }
                            }
                        }
                    }
                }

                //
                // assign over the row and column styles
                // 
                containerInfo.RowStyles = rowStyles;
                containerInfo.ColumnStyles = columnStyles;

                // since we've given over the styles to the other guy, null out.
                columnStyles = null;
                rowStyles = null;

                // set a flag for assertion detection.
                isValid = false;

            }



            public TableLayoutColumnStyleCollection ColumnStyles
            {
                get
                {
                    if (columnStyles == null)
                    {
                        columnStyles = new TableLayoutColumnStyleCollection();
                    }
                    return columnStyles;
                }
            }

            public bool IsValid
            {
                get { return isValid; }
            }

            public TableLayoutRowStyleCollection RowStyles
            {
                get
                {
                    if (rowStyles == null)
                    {
                        rowStyles = new TableLayoutRowStyleCollection();
                    }
                    return rowStyles;
                }
            }

            internal List<ControlInformation> GetControlsInformation()
            {

                if (controlsInfo == null)
                {
                    return new List<ControlInformation>();
                }

                List<ControlInformation> listOfControlInfo = new List<ControlInformation>(controlsInfo.Count);
                foreach (object name in controlsInfo.Keys)
                {
                    ControlInformation ci = controlsInfo[name];
                    ci.Name = name;
                    listOfControlInfo.Add(ci);
                }
                return listOfControlInfo;
            }

            private ControlInformation GetControlInformation(object controlName)
            {

                if (controlsInfo == null)
                {
                    return DefaultControlInfo;
                }
                if (!controlsInfo.ContainsKey(controlName))
                {
                    return DefaultControlInfo;
                }
                return controlsInfo[controlName];

            }

            public int GetColumn(object controlName)
            {
                return GetControlInformation(controlName).Column;
            }
            public int GetColumnSpan(object controlName)
            {
                return GetControlInformation(controlName).ColumnSpan;
            }
            public int GetRow(object controlName)
            {
                return GetControlInformation(controlName).Row;
            }
            public int GetRowSpan(object controlName)
            {
                return GetControlInformation(controlName).RowSpan;
            }

            private void SetControlInformation(object controlName, ControlInformation info)
            {
                if (controlsInfo == null)
                {
                    controlsInfo = new Dictionary<object, ControlInformation>();
                }
                controlsInfo[controlName] = info;
            }

            public void SetColumn(object controlName, int column)
            {
                if (GetColumn(controlName) != column)
                {
                    ControlInformation info = GetControlInformation(controlName);
                    info.Column = column;
                    SetControlInformation(controlName, info);
                }

            }
            public void SetColumnSpan(object controlName, int value)
            {
                if (GetColumnSpan(controlName) != value)
                {
                    ControlInformation info = GetControlInformation(controlName);
                    info.ColumnSpan = value;
                    SetControlInformation(controlName, info);
                }
            }
            public void SetRow(object controlName, int row)
            {
                if (GetRow(controlName) != row)
                {
                    ControlInformation info = GetControlInformation(controlName);
                    info.Row = row;
                    SetControlInformation(controlName, info);
                }
            }
            public void SetRowSpan(object controlName, int value)
            {
                if (GetRowSpan(controlName) != value)
                {
                    ControlInformation info = GetControlInformation(controlName);
                    info.RowSpan = value;
                    SetControlInformation(controlName, info);
                }
            }


        } // end of System.Windows.Forms.TableLayoutSettings

        internal class StyleConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(InstanceDescriptor))
                {
                    return true;
                }
                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                {
                    throw new ArgumentNullException("destinationType");
                }

                if (destinationType == typeof(InstanceDescriptor) && value is TableLayoutStyle)
                {
                    TableLayoutStyle style = (TableLayoutStyle)value;

                    switch (style.SizeType)
                    {
                        case SizeType.AutoSize:
                            return new InstanceDescriptor(
                                style.GetType().GetConstructor(new Type[] { }),
                                new object[] { });
                        case SizeType.Absolute:
                        case SizeType.Percent:
                            return new InstanceDescriptor(
                                style.GetType().GetConstructor(new Type[] { typeof(SizeType), typeof(int) }),
                                new object[] { style.SizeType, style.Size });
                        default:
                            Debug.Fail("Unsupported SizeType.");
                            break;
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }


    public enum SizeType
    {

        AutoSize,

        Absolute,

        Percent
    }


    public class ColumnStyle : TableLayoutStyle
    {


        public ColumnStyle() { }


        public ColumnStyle(SizeType sizeType)
        {
            this.SizeType = sizeType;
        }


        public ColumnStyle(SizeType sizeType, float width)
        {
            this.SizeType = sizeType;
            this.Width = width;
        }


        public float Width
        {
            get { return base.Size; }
            set { base.Size = value; }
        }
    }


    public class RowStyle : TableLayoutStyle
    {


        public RowStyle() { }

        public RowStyle(SizeType sizeType)
        {
            this.SizeType = sizeType;
        }


        public RowStyle(SizeType sizeType, float height)
        {
            this.SizeType = sizeType;
            this.Height = height;
        }


        public float Height
        {
            get { return base.Size; }
            set { base.Size = value; }
        }
    }
}