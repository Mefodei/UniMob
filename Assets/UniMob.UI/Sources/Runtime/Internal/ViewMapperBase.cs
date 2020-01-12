using System;
using System.Collections.Generic;

namespace UniMob.UI.Internal
{
    public abstract class ViewMapperBase : IViewTreeElement
    {
        private List<Item> _items = new List<Item>();
        private List<Item> _next = new List<Item>();

        private readonly ViewRenderScope _renderScope = new ViewRenderScope();
        private IDisposable _activeRender;

        class Item
        {
            public IView View;
            public IState State;
        }

        protected abstract IView ResolveView(IViewState state);
        protected abstract void RecycleView(IView view);

        void IViewTreeElement.AddChild(IViewTreeElement view)
        {
        }

        void IViewTreeElement.Unmount()
        {
            RemoveAll();
        }

        private void RenderItems(IState[] states, int startIndex, int count, Action<IView, IState> postRender = null)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            if (_activeRender == null)
                throw new InvalidOperationException("Must call BeginRender() before RenderArray()");

            for (int i = 0; i < count; i++)
            {
                var model = states[startIndex + i];

                var oldChildIndex = ViewContext.ChildIndex;
                ViewContext.ChildIndex = startIndex + i;

                var item = RenderItemInternal(model);
                postRender?.Invoke(item.View, item.State);

                ViewContext.ChildIndex = oldChildIndex;
            }
        }

        private IView RenderItem(IState states)
        {
            if (_activeRender == null)
                throw new InvalidOperationException("Must call BeginRender() before RenderItem()");

            return RenderItemInternal(states).View;
        }

        private Item RenderItemInternal(IState state)
        {
            var viewState = state.InnerViewState;

            var nextViewReference = viewState.View;

            var item = _items.Find(o => ReferenceEquals(o.State, viewState));
            if (item == null)
            {
                var view = ResolveView(viewState);

                view.SetSource(viewState);
                item = new Item {State = viewState, View = view};
            }
            else
            {
                _items.Remove(item);

                if (!item.View.ViewReference.Equals(nextViewReference))
                {
                    item.View.ResetSource();
                    RecycleView(item.View);
                    item.View = ResolveView(viewState);
                }

                item.View.SetSource(viewState);
                item.State = viewState;
            }

            item.View.ViewReference.LinkAtomToScope();

            _next.Add(item);

            return item;
        }

        public class ViewMapperRenderScope : IDisposable
        {
            internal ViewMapperBase Mapper { get; set; }

            public bool AutoRecycle { get; set; } = true;

            public void Initialize()
            {
                Mapper.BeginRender();
            }

            void IDisposable.Dispose()
            {
                Mapper.EndRender();

                if (AutoRecycle)
                {
                    Pools.ViewMapperRenderScope.Recycle(this);
                }
            }

            public void RenderItems(IState[] states, Action<IView, IState> postRender = null)
                => Mapper.RenderItems(states, 0, states.Length, postRender);

            public void RenderItems(IState[] states, int startIndex, int count, Action<IView, IState> postRender = null)
                => Mapper.RenderItems(states, startIndex, count, postRender);

            public IView RenderItem(IState state)
                => Mapper.RenderItem(state);
        }

        public ViewMapperRenderScope CreateRender()
        {
            var scope = Pools.ViewMapperRenderScope.Get();
            scope.Mapper = this;
            scope.Initialize();
            return scope;
        }

        private void BeginRender()
        {
            if (_activeRender != null)
                throw new InvalidOperationException("Must not call Render() inside other Render()");

            _renderScope.Link(this);
            _activeRender = _renderScope.Enter(this);

            PrepareRender();
        }

        private void EndRender()
        {
            if (_activeRender == null)
                throw new InvalidOperationException("Must not call EndRender() without BeginRender()");

            RemoveAll();

            var old = _items;
            _items = _next;
            _next = old;

            _activeRender?.Dispose();
            _activeRender = null;
        }

        private void RemoveAll()
        {
            RemoveAllAndClear(_items);
        }

        private void PrepareRender()
        {
            RemoveAllAndClear(_next);
        }

        private void RemoveAllAndClear(List<Item> list)
        {
            if (list.Count == 0)
                return;

            foreach (var removed in list)
            {
                removed.View.ResetSource();
                RecycleView(removed.View);
            }

            list.Clear();
        }
    }
}