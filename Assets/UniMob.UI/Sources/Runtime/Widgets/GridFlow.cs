using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class GridFlow : MultiChildLayoutWidget
    {
        public GridFlow(
            [NotNull] List<Widget> children,
            [CanBeNull] Key key = null,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Start,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
            AxisSize crossAxisSize = AxisSize.Min,
            AxisSize mainAxisSize = AxisSize.Min,
            [CanBeNull] int? maxCrossAxisCount = null,
            [CanBeNull] float? maxCrossAxisExtent = null)
            : base(children, key)
        {
            CrossAxisAlignment = crossAxisAlignment;
            MainAxisAlignment = mainAxisAlignment;
            CrossAxisSize = crossAxisSize;
            MainAxisSize = mainAxisSize;
            MaxCrossAxisCount = Math.Max(maxCrossAxisCount ?? int.MaxValue, 1);
            MaxCrossAxisExtent = Math.Max(maxCrossAxisExtent ?? float.MaxValue, 1);
        }

        public CrossAxisAlignment CrossAxisAlignment { get; }
        public MainAxisAlignment MainAxisAlignment { get; }
        public AxisSize CrossAxisSize { get; }
        public AxisSize MainAxisSize { get; }
        public int MaxCrossAxisCount { get; }
        public float MaxCrossAxisExtent { get; }

        public override State CreateState() => new GridFlowState();
    }

    internal class GridFlowState : MultiChildLayoutState<GridFlow>, IGridFlowState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Grid");

        public GridFlowState()
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public WidgetSize InnerSize => _innerSize.Value;
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public int MaxCrossAxisCount => Widget.MaxCrossAxisCount;
        public float MaxCrossAxisExtent => Widget.MaxCrossAxisExtent;

        public override WidgetSize CalculateSize()
        {
            var wStretch = Widget.CrossAxisSize == AxisSize.Max;
            var hStretch = Widget.MainAxisSize == AxisSize.Max;

            if (wStretch && hStretch)
            {
                return WidgetSize.Stretched;
            }

            var size = _innerSize.Value;

            float? width = null;
            float? height = null;

            if (size.IsWidthFixed && !wStretch) width = size.Width;
            if (size.IsHeightFixed && !hStretch) height = size.Height;

            return new WidgetSize(width, height);
        }

        private WidgetSize CalculateInnerSize()
        {
            var width = 0.0f;
            var height = 0.0f;

            var lineWidth = 0.0f;
            var lineHeight = 0.0f;
            var lineChildNum = 0;

            var maxLineWidth = Widget.MaxCrossAxisExtent;
            var maxLineChildNum = Widget.MaxCrossAxisCount;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                if (childSize.IsWidthStretched || childSize.IsHeightStretched)
                {
                    continue;
                }

                if (lineChildNum + 1 <= maxLineChildNum &&
                    lineWidth + childSize.Width <= maxLineWidth)
                {
                    lineChildNum++;
                    lineWidth += childSize.Width;
                    lineHeight = Math.Max(lineHeight, childSize.Height);
                }
                else
                {
                    width = Math.Max(width, lineWidth);
                    height += lineHeight;

                    lineChildNum = 1;
                    lineWidth = childSize.Width;
                    lineHeight = childSize.Height;
                }
            }

            width = Math.Max(width, lineWidth);
            height += lineHeight;

            width = Math.Min(width, MaxCrossAxisExtent);

            return new WidgetSize(width, height);
        }
    }
}