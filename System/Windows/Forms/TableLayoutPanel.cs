﻿namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms.Layout;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ProvideProperty("ColumnSpan", typeof(Control))]
    [ProvideProperty("RowSpan", typeof(Control))]
    [ProvideProperty("Row", typeof(Control))]
    [ProvideProperty("Column", typeof(Control))]
    [ProvideProperty("CellPosition", typeof(Control))]
    [DefaultProperty("ColumnCount")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class TableLayoutPanel : Panel, IExtenderProvider
    {
        private TableLayoutSettings _tableLayoutSettings;
        private static readonly object EventCellPaint = new object();

        public TableLayoutPanel()
        {
            _tableLayoutSettings = TableLayout.CreateSettings(this);
        }

        public override LayoutEngine LayoutEngine
        {
            get { return TableLayout.Instance; }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public TableLayoutSettings LayoutSettings
        {
            get
            {
                return _tableLayoutSettings;
            }
            set
            {
                if (value != null && value.IsStub)
                {
                    // WINRES only scenario.
                    // we only support table layout settings that have been created from a type converter.  
                    // this is here for localization (WinRes) support.
                    using (new LayoutTransaction(this, this, PropertyNames.LayoutSettings))
                    {
                        // apply RowStyles, ColumnStyles, Row & Column assignments.
                        _tableLayoutSettings.ApplySettings(value);
                    }
                }
                else
                {
                    throw new NotSupportedException("TableLayoutSettingSettingsIsNotSupported");
                }

            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Localizable(true)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set
            {
                base.BorderStyle = value;
                Debug.Assert(BorderStyle == value, "BorderStyle should be the same as we set it");
            }
        }


        [DefaultValue(TableLayoutPanelCellBorderStyle.None), Localizable(true)]
        public TableLayoutPanelCellBorderStyle CellBorderStyle
        {
            get { return _tableLayoutSettings.CellBorderStyle; }
            set
            {
                _tableLayoutSettings.CellBorderStyle = value;

                // PERF: dont turn on ResizeRedraw unless we know we need it.
                if (value != TableLayoutPanelCellBorderStyle.None)
                {
                    SetStyle(ControlStyles.ResizeRedraw, true);
                }
                this.Invalidate();
                Debug.Assert(CellBorderStyle == value, "CellBorderStyle should be the same as we set it");
            }
        }

        private int CellBorderWidth
        {
            get { return _tableLayoutSettings.CellBorderWidth; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new TableLayoutControlCollection Controls
        {
            get { return (TableLayoutControlCollection)base.Controls; }
        }

        [DefaultValue(0)]
        [Localizable(true)]
        public int ColumnCount
        {
            get { return _tableLayoutSettings.ColumnCount; }
            set
            {
                _tableLayoutSettings.ColumnCount = value;
                Debug.Assert(ColumnCount == value, "ColumnCount should be the same as we set it");
            }
        }

        [DefaultValue(TableLayoutPanelGrowStyle.AddRows)]
        public TableLayoutPanelGrowStyle GrowStyle
        {
            get
            {
                return _tableLayoutSettings.GrowStyle;
            }
            set
            {
                _tableLayoutSettings.GrowStyle = value;
            }
        }

        [DefaultValue(0)]
        [Localizable(true)]
        public int RowCount
        {
            get { return _tableLayoutSettings.RowCount; }
            set { _tableLayoutSettings.RowCount = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [DisplayName("Rows")]
        [MergableProperty(false)]
        [Browsable(false)]
        public TableLayoutRowStyleCollection RowStyles
        {
            get { return _tableLayoutSettings.RowStyles; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [DisplayName("Columns")]
        [Browsable(false)]
        [MergableProperty(false)]
        public TableLayoutColumnStyleCollection ColumnStyles
        {
            get { return _tableLayoutSettings.ColumnStyles; }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new TableLayoutControlCollection(this);
        }

        private bool ShouldSerializeControls()
        {
            TableLayoutControlCollection collection = this.Controls;
            return collection != null && collection.Count > 0;
        }

        #region Extended Properties
        bool IExtenderProvider.CanExtend(object obj)
        {
            Control control = obj as Control;
            return control != null && control.Parent == this;
        }

        [DefaultValue(1)]
        [DisplayName("ColumnSpan")]
        public int GetColumnSpan(Control control)
        {
            return _tableLayoutSettings.GetColumnSpan(control);
        }

        public void SetColumnSpan(Control control, int value)
        {
            // layout.SetColumnSpan() throws ArgumentException if out of range.
            _tableLayoutSettings.SetColumnSpan(control, value);
            Debug.Assert(GetColumnSpan(control) == value, "GetColumnSpan should be the same as we set it");
        }

        [DefaultValue(1)]
        [DisplayName("RowSpan")]
        public int GetRowSpan(Control control)
        {
            return _tableLayoutSettings.GetRowSpan(control);
        }

        public void SetRowSpan(Control control, int value)
        {
            // layout.SetRowSpan() throws ArgumentException if out of range.
            _tableLayoutSettings.SetRowSpan(control, value);
            Debug.Assert(GetRowSpan(control) == value, "GetRowSpan should be the same as we set it");
        }

        //get the row position of the control
        [DefaultValue(-1)]  //if change this value, also change the SerializeViaAdd in TableLayoutControlCollectionCodeDomSerializer
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DisplayName("Row")]
        public int GetRow(Control control)
        {
            return _tableLayoutSettings.GetRow(control);
        }

        //set the row position of the control
        public void SetRow(Control control, int row)
        {
            _tableLayoutSettings.SetRow(control, row);
            Debug.Assert(GetRow(control) == row, "GetRow should be the same as we set it");
        }

        //get the row and column position of the control
        [DefaultValue(typeof(TableLayoutPanelCellPosition), "-1,-1")]  //if change this value, also change the SerializeViaAdd in TableLayoutControlCollectionCodeDomSerializer
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DisplayName("Cell")]
        public TableLayoutPanelCellPosition GetCellPosition(Control control)
        {
            return _tableLayoutSettings.GetCellPosition(control);
        }

        //set the row and column of the control
        public void SetCellPosition(Control control, TableLayoutPanelCellPosition position)
        {
            _tableLayoutSettings.SetCellPosition(control, position);
        }



        //get the column position of the control
        [DefaultValue(-1)]  //if change this value, also change the SerializeViaAdd in TableLayoutControlCollectionCodeDomSerializer
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DisplayName("Column")]
        public int GetColumn(Control control)
        {
            return _tableLayoutSettings.GetColumn(control);
        }

        //set the column position of the control
        public void SetColumn(Control control, int column)
        {
            _tableLayoutSettings.SetColumn(control, column);
            Debug.Assert(GetColumn(control) == column, "GetColumn should be the same as we set it");
        }

        public Control GetControlFromPosition(int column, int row)
        {
            return (Control)_tableLayoutSettings.GetControlFromPosition(column, row);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // Using Control instead of IArrangedElement intentionally
        public TableLayoutPanelCellPosition GetPositionFromControl(Control control)
        {
            return _tableLayoutSettings.GetPositionFromControl(control);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int[] GetColumnWidths()
        {
            TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(this);
            if (containerInfo.Columns == null)
            {
                return new int[0];
            }

            int[] cw = new int[containerInfo.Columns.Length];
            for (int i = 0; i < containerInfo.Columns.Length; i++)
            {
                cw[i] = containerInfo.Columns[i].MinSize;
            }
            return cw;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int[] GetRowHeights()
        {
            TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(this);
            if (containerInfo.Rows == null)
            {
                return new int[0];
            }

            int[] rh = new int[containerInfo.Rows.Length];
            for (int i = 0; i < containerInfo.Rows.Length; i++)
            {
                rh[i] = containerInfo.Rows[i].MinSize;
            }
            return rh;
        }


        #endregion

        #region PaintCode

        public event TableLayoutCellPaintEventHandler CellPaint
        {
            add
            {
                Events.AddHandler(EventCellPaint, value);
            }
            remove
            {
                Events.RemoveHandler(EventCellPaint, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            this.Invalidate();
        }

        protected virtual void OnCellPaint(TableLayoutCellPaintEventArgs e)
        {
            TableLayoutCellPaintEventHandler handler = (TableLayoutCellPaintEventHandler)Events[EventCellPaint];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    base.OnPaintBackground(e);

        //    // paint borderstyles on top of the background image in WM_ERASEBKGND

        //    int cellBorderWidth = this.CellBorderWidth;
        //    TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(this);
        //    TableLayout.Strip[] colStrips = containerInfo.Columns;
        //    TableLayout.Strip[] rowStrips = containerInfo.Rows;
        //    TableLayoutPanelCellBorderStyle cellBorderStyle = this.CellBorderStyle;

        //    if (colStrips == null || rowStrips == null)
        //    {
        //        return;
        //    }
        //    int cols = colStrips.Length;
        //    int rows = rowStrips.Length;

        //    int totalColumnWidths = 0, totalColumnHeights = 0;

        //    Graphics g = e.Graphics;
        //    Rectangle displayRect = DisplayRectangle;
        //    Rectangle clipRect = e.ClipRectangle;

        //    //leave the space for the border
        //    int startx;
        //    bool isRTL = (RightToLeft == RightToLeft.Yes);
        //    if (isRTL)
        //    {
        //        startx = displayRect.Right - (cellBorderWidth / 2);
        //    }
        //    else
        //    {
        //        startx = displayRect.X + (cellBorderWidth / 2);
        //    }

        //    for (int i = 0; i < cols; i++)
        //    {
        //        int starty = displayRect.Y + (cellBorderWidth / 2);

        //        if (isRTL)
        //        {
        //            startx -= colStrips[i].MinSize;
        //        }

        //        for (int j = 0; j < rows; j++)
        //        {
        //            Rectangle outsideCellBounds = new Rectangle(startx, starty, ((TableLayout.Strip)colStrips[i]).MinSize, ((TableLayout.Strip)rowStrips[j]).MinSize);

        //            Rectangle insideCellBounds = new Rectangle(outsideCellBounds.X + (cellBorderWidth + 1) / 2, outsideCellBounds.Y + (cellBorderWidth + 1) / 2, outsideCellBounds.Width - (cellBorderWidth + 1) / 2, outsideCellBounds.Height - (cellBorderWidth + 1) / 2);

        //            if (clipRect.IntersectsWith(insideCellBounds))
        //            {
        //                //first, call user's painting code
        //                using (TableLayoutCellPaintEventArgs pcea = new TableLayoutCellPaintEventArgs(g, clipRect, insideCellBounds, i, j))
        //                {
        //                    OnCellPaint(pcea);
        //                }
        //                // paint the table border on top.
        //                ControlPaint.PaintTableCellBorder(cellBorderStyle, g, outsideCellBounds);
        //            }
        //            starty += rowStrips[j].MinSize;
        //            // Only sum this up once...
        //            if (i == 0)
        //            {
        //                totalColumnHeights += rowStrips[j].MinSize;
        //            }
        //        }

        //        if (!isRTL)
        //        {
        //            startx += colStrips[i].MinSize;
        //        }
        //        totalColumnWidths += colStrips[i].MinSize;
        //    }

        //    if (!HScroll && !VScroll && cellBorderStyle != TableLayoutPanelCellBorderStyle.None)
        //    {
        //        Rectangle tableBounds = new Rectangle(cellBorderWidth / 2 + displayRect.X, cellBorderWidth / 2 + displayRect.Y, displayRect.Width - cellBorderWidth, displayRect.Height - cellBorderWidth);
        //        // paint the border of the table if we are not auto scrolling.
        //        // if the borderStyle is Inset or Outset, we can only paint the lower bottom half since otherwise we will have 1 pixel loss at the border.
        //        if (cellBorderStyle == TableLayoutPanelCellBorderStyle.Inset)
        //        {
        //            g.DrawLine(SystemPens.ControlDark, tableBounds.Right, tableBounds.Y, tableBounds.Right, tableBounds.Bottom);
        //            g.DrawLine(SystemPens.ControlDark, tableBounds.X, tableBounds.Y + tableBounds.Height - 1, tableBounds.X + tableBounds.Width - 1, tableBounds.Y + tableBounds.Height - 1);
        //        }
        //        else if (cellBorderStyle == TableLayoutPanelCellBorderStyle.Outset)
        //        {
        //            using (Pen pen = new Pen(SystemColors.Window))
        //            {
        //                g.DrawLine(pen, tableBounds.X + tableBounds.Width - 1, tableBounds.Y, tableBounds.X + tableBounds.Width - 1, tableBounds.Y + tableBounds.Height - 1);
        //                g.DrawLine(pen, tableBounds.X, tableBounds.Y + tableBounds.Height - 1, tableBounds.X + tableBounds.Width - 1, tableBounds.Y + tableBounds.Height - 1);
        //            }
        //        }
        //        else
        //        {
        //            ControlPaint.PaintTableCellBorder(cellBorderStyle, g, tableBounds);
        //        }
        //        ControlPaint.PaintTableControlBorder(cellBorderStyle, g, displayRect);
        //    }
        //    else
        //    {
        //        ControlPaint.PaintTableControlBorder(cellBorderStyle, g, displayRect);
        //    }

        //}


        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void ScaleCore(float dx, float dy)
        //{

        //    base.ScaleCore(dx, dy);
        //    ScaleAbsoluteStyles(new SizeF(dx, dy));
        //}

        //protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        //{
        //    base.ScaleControl(factor, specified);
        //    ScaleAbsoluteStyles(factor);
        //}

        private void ScaleAbsoluteStyles(SizeF factor)
        {
            TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(this);
            int i = 0;

            // VSWhidbey 432427: the last row/column can be larger than the 
            // absolutely styled column width.
            int lastRowHeight = -1;
            int lastRow = containerInfo.Rows.Length - 1;
            if (containerInfo.Rows.Length > 0)
            {
                lastRowHeight = containerInfo.Rows[lastRow].MinSize;
            }

            int lastColumnHeight = -1;
            int lastColumn = containerInfo.Columns.Length - 1;
            if (containerInfo.Columns.Length > 0)
            {
                lastColumnHeight = containerInfo.Columns[containerInfo.Columns.Length - 1].MinSize;
            }

            foreach (ColumnStyle cs in ColumnStyles)
            {
                if (cs.SizeType == SizeType.Absolute)
                {
                    if (i == lastColumn && lastColumnHeight > 0)
                    {
                        // the last column is typically expanded to fill the table. use the actual
                        // width in this case.
                        cs.Width = (float)Math.Round(lastColumnHeight * factor.Width);
                    }
                    else
                    {
                        cs.Width = (float)Math.Round(cs.Width * factor.Width);
                    }
                }
                i++;
            }

            i = 0;

            foreach (RowStyle rs in RowStyles)
            {
                if (rs.SizeType == SizeType.Absolute)
                {
                    if (i == lastRow && lastRowHeight > 0)
                    {
                        // the last row is typically expanded to fill the table. use the actual
                        // width in this case.
                        rs.Height = (float)Math.Round(lastRowHeight * factor.Height);
                    }
                    else
                    {
                        rs.Height = (float)Math.Round(rs.Height * factor.Height);
                    }
                }
            }

        }

        #endregion
    }

    #region ControlCollection
    [ListBindable(false)]
    public class TableLayoutControlCollection : Control.ControlCollection
    {
        private TableLayoutPanel _container;

        public TableLayoutControlCollection(TableLayoutPanel container) : base(container)
        {
            _container = (TableLayoutPanel)container;
        }

        //the container of this TableLayoutControlCollection
        public TableLayoutPanel Container
        {
            get { return _container; }
        }

        //Add control to cell (x, y) on the table. The control becomes absolutely positioned if neither x nor y is equal to -1
        public virtual void Add(Control control, int column, int row)
        {
            base.Add(control);
            _container.SetColumn(control, column);
            _container.SetRow(control, row);
        }
    }
    #endregion
}