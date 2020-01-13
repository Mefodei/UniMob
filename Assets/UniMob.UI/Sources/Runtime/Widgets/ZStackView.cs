using UniMob.UI.Internal;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public class ZStackView : View<IZStackState>
    {
        private ViewMapperBase _mapper;

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);
        }

        protected override void Render()
        {
            var alignment = State.Alignment;
            var children = State.Children;

            using (var render = _mapper.CreateRender())
            {
                foreach (var child in children)
                {
                    var childSize = child.Size;
                    var childView = render.RenderItem(child);

                    LayoutData layout;
                    layout.Size = childSize;
                    layout.Alignment = alignment;
                    layout.Corner = alignment;
                    layout.CornerPosition = Vector2.zero;
                    ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
                }
            }
        }
    }

    public interface IZStackState : IViewState
    {
        Alignment Alignment { get; }
        IState[] Children { get; }
    }
}