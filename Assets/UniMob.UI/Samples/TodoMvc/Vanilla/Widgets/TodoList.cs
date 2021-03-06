using UniMob.UI.Samples.TodoMvc.Views;

namespace UniMob.UI.Samples.TodoMvc.Vanilla.Widgets
{
    public class TodoList : StatefulWidget
    {
        public override State CreateState() => new TodoListState();
    }

    public class TodoListState : ViewState<TodoList>, ITodoListState
    {
        private readonly MutableAtom<bool> _full = Atom.Value(false);

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("TodoList");

        public override WidgetSize CalculateSize()
        {
            var size = base.CalculateSize();
            if (_full.Value) size = new WidgetSize(250, 250);
            return size;
        }

        public void OnTap()
        {
            _full.Value = !_full.Value;
        }
    }
}