using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace UniMob.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ViewBase<TState> : UIBehaviour, IView, IViewTreeElement
    {
        [NotNull] private readonly ViewRenderScope _renderScope = new ViewRenderScope();
        [NotNull] private readonly List<IViewTreeElement> _children = new List<IViewTreeElement>();

        private bool _mounted;

        private bool _hasState;
        private TState _state;

        private bool _hasSource;
        private readonly MutableAtom<TState> _source = Atom.Value(default(TState));

        private ReactionAtom _renderAtom;
        private PerfWatcher _renderPerf;

        [NotNull] protected TState State => _state;

        // ReSharper disable once InconsistentNaming
        public RectTransform rectTransform => (RectTransform) transform;

        void IView.SetSource(object newSource)
        {
            if (!(newSource is TState nextState))
            {
                var expected = typeof(TState).Name;
                var actual = newSource.GetType().Name;
                Debug.LogError($"Wrong model type at '{name}': expected={expected}, actual={actual}");
                return;
            }

            _renderScope.Link(this);

            if (_renderAtom == null)
                _renderAtom = new ReactionAtom(DoRender);

            _hasSource = true;
            _source.Value = nextState;

            _renderAtom.Update();
        }

        protected void Unmount()
        {
            if (!_hasSource)
            {
                Assert.IsFalse(_hasState, "hasModel");
                Assert.IsFalse(_mounted, "mounted");
                return;
            }

            Assert.IsNotNull(_renderAtom, "renderAtom == null");
            _renderAtom.Deactivate();

            _source.Value = default;
            _hasSource = false;

            if (!_hasState)
            {
                Assert.IsFalse(_mounted, "mounted");
                return;
            }

            try
            {
                using (Atom.NoWatch)
                {
                    Deactivate();
                }
            }
            catch (Exception ex)
            {
                Zone.Current.HandleUncaughtException(ex);
            }

            if (_mounted)
            {
                _mounted = false;
                DidStateDetached(_state);
            }

            foreach (var child in _children)
            {
                child.Unmount();
            }

            _state = default;
            _hasState = false;
        }

        void IView.ResetSource()
        {
            Unmount();
        }

        void IViewTreeElement.AddChild(IViewTreeElement view)
        {
            _children.Add(view);
        }

        void IViewTreeElement.Unmount()
        {
            Unmount();
        }

        private void DoRender()
        {
            Assert.IsTrue(_hasSource, "!hasSource");

            var nextState = _source.Value;
            if (nextState == null)
            {
                Debug.LogWarning("Model == null", this);
                return;
            }

            using (Atom.NoWatch)
            {
                if (!_hasState || !nextState.Equals(_state))
                {
                    if (_hasState)
                    {
                        try
                        {
                            Deactivate();
                        }
                        catch (Exception ex)
                        {
                            Zone.Current.HandleUncaughtException(ex);
                        }
                    }

                    _hasState = true;
                    _state = nextState;

                    try
                    {
                        Activate();
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }
                }
            }

            Assert.IsNotNull(_renderScope, "renderScope == null");
            using (_renderScope.Enter(this))
            {
                if (isActiveAndEnabled && gameObject.activeSelf)
                {
                    _children.Clear();

                    try
                    {
                        using (_renderPerf.Watch())
                        {
                            Render();
                        }
                    }
                    catch (Exception ex)
                    {
                        Zone.Current.HandleUncaughtException(ex);
                    }

                    if (!_mounted)
                    {
                        _mounted = true;
                        DidStateAttached(_state);
                    }
                }
            }
        }

        protected virtual void DidStateAttached(TState state)
        {
        }

        protected virtual void DidStateDetached(TState state)
        {
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_renderPerf == null || !_renderPerf.Name.EndsWith(name))
            {
                _renderPerf = new PerfWatcher("Render " + name);
            }

            if (_hasSource)
            {
                Assert.IsNotNull(_renderAtom, "renderAtom == null");
                _renderAtom.Update();
            }
        }

        protected virtual void Activate()
        {
        }

        protected virtual void Deactivate()
        {
        }

        protected abstract void Render();
    }
}