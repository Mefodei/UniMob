namespace UniMob.UI.Widgets
{
    public class Empty : StatefulWidget
    {
        public override State CreateState() => new EmptyState();
    }

    public class EmptyState : ViewState<Empty>, IEmptyState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Empty");

        public override WidgetSize CalculateSize() => WidgetSize.Fixed(0, 0);
    }
}