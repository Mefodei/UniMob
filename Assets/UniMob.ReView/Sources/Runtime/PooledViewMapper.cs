using System;
using UniMob.Pools;
using UnityEngine;

namespace UniMob.ReView
{
    public class PooledViewMapper : ViewMapperBase
    {
        private readonly Func<Transform> _parentSelector;
        private readonly bool _worldPositionStays;

        public PooledViewMapper(Transform parent, bool worldPositionStays = false)
            : this(() => parent, worldPositionStays)
        {
        }

        public PooledViewMapper(Func<Transform> parentSelector, bool worldPositionStays)
        {
            _parentSelector = parentSelector;
            _worldPositionStays = worldPositionStays;
        }

        protected override IView ResolveView(IState state)
        {
            var prefab = ViewContext.Loader.LoadViewPrefab(state);
            var view = GameObjectPool
                .Instantiate(prefab.gameObject, _parentSelector.Invoke(), _worldPositionStays)
                .GetComponent<IView>();
            view.rectTransform.anchoredPosition = Vector2.zero;
            return view;
        }

        protected override void RecycleView(IView view)
        {
            GameObjectPool.Recycle(view.gameObject, false);
        }
    }
}