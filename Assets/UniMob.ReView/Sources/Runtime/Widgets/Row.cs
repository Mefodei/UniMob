using JetBrains.Annotations;
using UniMob.Async;
using UnityEngine;

namespace UniMob.ReView.Widgets
{
    public sealed class Row : MultiChildLayoutWidget
    {
        public Row(
            [NotNull] WidgetList children,
            [CanBeNull] Key key = null,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Start,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
            AxisSize crossAxisSize = AxisSize.Min,
            AxisSize mainAxisSize = AxisSize.Min
        ) : base(
            children,
            key
        )
        {
            CrossAxisAlignment = crossAxisAlignment;
            MainAxisAlignment = mainAxisAlignment;
            CrossAxisSize = crossAxisSize;
            MainAxisSize = mainAxisSize;
        }

        public CrossAxisAlignment CrossAxisAlignment { get; }
        public MainAxisAlignment MainAxisAlignment { get; }

        public AxisSize CrossAxisSize { get; }
        public AxisSize MainAxisSize { get; }

        public override State CreateState() => new RowState();
    }

    internal sealed class RowState : MultiChildLayoutState<Row>, IRowState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public RowState() : base("UniMob.Row")
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public WidgetSize InnerSize => _innerSize.Value;

        public override WidgetSize CalculateSize()
        {
            var wStretch = Widget.MainAxisSize == AxisSize.Max;
            var hStretch = Widget.CrossAxisSize == AxisSize.Max;

            if (wStretch && hStretch)
            {
                return WidgetSize.Stretched;
            }

            var size = CalculateInnerSize();

            float? width = null;
            float? height = null;

            if (size.IsWidthFixed && !wStretch) width = size.Width;
            if (size.IsHeightFixed && !hStretch) height = size.Height;

            return new WidgetSize(width, height);
        }

        private WidgetSize CalculateInnerSize()
        {
            float width = 0;
            float? height = 0;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                if (childSize.IsWidthFixed)
                {
                    width += childSize.Width;
                }

                if (height.HasValue && childSize.IsHeightFixed)
                {
                    height = Mathf.Max(height.Value, childSize.Height);
                }
                else
                {
                    height = null;
                }
            }

            return new WidgetSize(width, height);
        }
    }
}