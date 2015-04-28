using System;
using System.Collections.Generic;
using System.Linq;

namespace Nuwa.Sdk
{
    /// <summary>
    /// RunFrame defines the environment in which a test command is run. The factors 
    /// related to the configuration includes host strategy, trace strategy, security 
    /// level and so on. 
    /// 
    /// A RunFrame is created during NuwaTestClassCommand (NTCC) initialization, but 
    /// it is not actually initialized until the first test command which requires 
    /// this RunFrame is executed. The lazy pattern ensure resources are reserved 
    /// when one test case is run individually. During creation, RunFrame accept a 
    /// collection of RunFrameElements which describes the what needs to be initialied.
    /// The actual meanings of the elements are agnostics to the RunFrame, to which 
    /// only the life cycle is intended to be managed.
    /// 
    /// Once a RunFrame is initialized, it wil be largely reused. But the Initialize 
    /// still needs to be called so as to fill the value to the property in test class.
    /// RunFrame is disposed when the NTCC is running its ClassFinished method. All 
    /// elements are disposed by then.
    /// </summary>
    public class RunFrame
    {
        private bool _initialized;
        private List<IRunElement> _elements;
        private Dictionary<string, object> _states;

        public RunFrame(IEnumerable<IRunElement> elements, string name)
        {
            _elements = new List<IRunElement>(elements);
            _states = new Dictionary<string, object>();
            _initialized = false;
            this.Name = name;
        }

        public string Name
        {
            get;
            private set;
        }

        public void Initialize(object testClass, NuwaTestCommand testCommand)
        {
            if (!_initialized)
            {
                foreach (var elem in _elements)
                {
                    elem.Initialize(this);
                }

                _initialized = true;
            }

            foreach (var elem in _elements)
            {
                elem.Recover(testClass, testCommand);
            }
        }

        public void Cleanup()
        {
            foreach (IRunElement elem in _elements)
            {
                elem.Cleanup(this);
            }
        }

        public IEnumerable<T> GetElements<T>()
            where T : class, IRunElement
        {
            return _elements
                .Where(e => typeof(T).IsAssignableFrom(e.GetType()))
                .Select(e => e as T);
        }

        public T GetFirstElement<T>()
            where T : class, IRunElement
        {
            return _elements.FirstOrDefault(e => typeof(T).IsAssignableFrom(e.GetType())) as T;
        }

        public object GetState(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            object retval;
            _states.TryGetValue(key, out retval);

            return retval;
        }

        public void SetState(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (value == null)
            {
                _states.Remove(key);
            }
            else
            {
                _states[key] = value;
            }
        }
    }
}