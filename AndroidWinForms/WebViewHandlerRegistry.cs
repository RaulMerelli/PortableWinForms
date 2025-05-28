using System.Collections.Generic;

namespace AndroidWinForms
{
    public static class WebViewHandlerRegistry
    {
        private static readonly List<object> _handlers = new();

        public static void Register(object instance)
        {
            if (!_handlers.Contains(instance))
                _handlers.Add(instance);
        }

        public static void Unregister(object instance)
        {
            _handlers.Remove(instance);
        }

        public static IReadOnlyList<object> Instances => _handlers;
    }
}
