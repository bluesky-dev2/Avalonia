namespace Avalonia.Styling.Activators
{
    /// <summary>
    /// Base class implementation of <see cref="IStyleActivator"/>.
    /// </summary>
    internal abstract class StyleActivatorBase : IStyleActivator
    {
        private IStyleActivatorSink? _sink;
        private int _tag;
        private bool _value;

        public bool IsActive => _value = EvaluateIsActive();

        public bool IsSubscribed => _sink is not null;

        public void Subscribe(IStyleActivatorSink sink, int tag = 0)
        {
            if (_sink is null)
            {
                Initialize();
                _sink = sink;
                _tag = tag;
            }
            else
            {
                throw new AvaloniaInternalException("StyleActivator is already subscribed.");
            }
        }

        public void Unsubscribe(IStyleActivatorSink sink)
        {
            if (_sink != sink)
            {
                throw new AvaloniaInternalException("StyleActivatorSink is not subscribed.");
            }

            _sink = null;
            Deinitialize();
        }

        public void Dispose()
        {
            _sink = null;
            Deinitialize();
        }

        /// <summary>
        /// Evaluates the <see cref="IsActive"/> value.
        /// </summary>
        /// <remarks>
        /// This method should read directly from its inputs and not rely on any subscriptions to
        /// fire in order to be up-to-date.
        /// </remarks>
        protected abstract bool EvaluateIsActive();

        /// <summary>
        /// Called from a derived class when the <see cref="IsActive"/> state should be re-evaluated
        /// and the subscriber notified of any change.
        /// </summary>
        /// <returns>
        /// The evaluated active state;
        /// </returns>
        protected bool ReevaluateIsActive()
        {
            var value = EvaluateIsActive();

            if (value != _value)
            {
                _value = value;
                _sink?.OnNext(value, _tag);
            }

            return value;
        }

        /// <summary>
        /// Called in response to a <see cref="Subscribe(IStyleActivatorSink, int)"/> to allow the
        /// derived class to set up any necessary subscriptions.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Called in response to an <see cref="Unsubscribe(IStyleActivatorSink)"/> or
        /// <see cref="Dispose"/> to allow the derived class to dispose any active subscriptions.
        /// </summary>
        protected abstract void Deinitialize();
    }
}
