    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                    _instance.OnInstance();
                }

                return _instance;
            }
        }

        public static void Dispose()
        {
            if (null != _instance)
            {
                _instance.OnDispose();
            }

            _instance = null;
        }

        protected virtual void OnInstance()
        {
        }

        protected virtual void OnDispose()
        {
        }
    }
