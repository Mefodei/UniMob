using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UniMob
{
    public class ComputedAtom<T> : AtomBase, MutableAtom<T>
    {
        private readonly IEqualityComparer<T> _comparer;
        private readonly AtomPull<T> _pull;
        private readonly AtomPush<T> _push;

        private bool _hasCache;
        private T _cache;
        private Exception _exception;

        private bool _isRunningSetter;

        internal ComputedAtom(
            [NotNull] AtomPull<T> pull,
            AtomPush<T> push = null,
            bool keepAlive = false,
            Action onActive = null,
            Action onInactive = null,
            IEqualityComparer<T> comparer = null)
            : base(keepAlive, onActive, onInactive)
        {
            _pull = pull ?? throw new ArgumentNullException(nameof(pull));
            _push = push;
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public T Value
        {
            get
            {
                if (!IsActive && Stack == null && !KeepAlive)
                {
                    return _pull();
                }

                Update();
                SubscribeToParent();

                if (_exception != null)
                {
                    throw _exception;
                }

                return _cache;
            }
            set
            {
                if (_push == null)
                    throw new InvalidOperationException("It is not possible to assign a new value to a readonly Atom");

                if (_isRunningSetter)
                {
                    var message = "The setter of MutableAtom is trying to update itself. " +
                                  "Did you intend to invoke Atom.Push(..), instead of the setter?";
                    throw new InvalidOperationException(message);
                }

                try
                {
                    using (Atom.NoWatch)
                    {
                        if (_hasCache && _comparer.Equals(value, _cache))
                            return;

                        State = AtomState.Obsolete;
                        _cache = default;
                        _exception = null;
                        _hasCache = false;

                        _isRunningSetter = true;
                        _push(value);
                    }
                }
                finally
                {
                    _isRunningSetter = false;
                    ObsoleteSubscribers();
                }
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            _hasCache = false;
            _cache = default;
            _exception = null;
        }

        protected override void Evaluate()
        {
            try
            {
                var value = _pull();

                using (Atom.NoWatch)
                {
                    if (_hasCache && _comparer.Equals(value, _cache))
                        return;
                }

                _hasCache = true;
                _cache = value;
                _exception = null;
            }
            catch (Exception exception)
            {
                _hasCache = false;
                _cache = default;
                _exception = exception;
            }
            finally
            {
                State = AtomState.Actual;
            }

            ObsoleteSubscribers();
        }

        public T Get() => Value;

        public void Set(T value) => Value = value;

        public void Invalidate()
        {
            State = AtomState.Obsolete;

            _hasCache = false;
            _cache = default;
            _exception = null;

            ObsoleteSubscribers();
        }

        public override string ToString()
        {
            if (_exception != null)
            {
                return _exception.ToString();
            }

            return _hasCache ? Convert.ToString(_cache) : "[undefined]";
        }
    }
}