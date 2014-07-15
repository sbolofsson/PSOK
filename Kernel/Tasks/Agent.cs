using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using PSOK.Kernel.Configuration;
using PSOK.Kernel.Exceptions;
using PSOK.Kernel.Reflection;
using log4net;
using Exception = System.Exception;
using Thread = PSOK.Kernel.Threads.Thread;

namespace PSOK.Kernel.Tasks
{
    /// <summary>
    /// Defines a scheduled task.
    /// </summary>
    internal class Agent : Thread
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(Agent));

        private static readonly ConcurrentDictionary<string, Lazy<Agent>> Agents =
            new ConcurrentDictionary<string, Lazy<Agent>>();

        // Instance fields
        private readonly string _type;
        private readonly string _method;
        private readonly TimeSpan _interval;
        private readonly IEnumerable<Param> _parameters;
        private readonly SemaphoreSlim _handleLock = new SemaphoreSlim(0);

        private DateTime _lastRun;
        private volatile bool _hasRun;

        private readonly Lazy<object> _instanceLazy;
        private readonly Lazy<MethodInfo> _runDelegateLazy;
        private readonly Lazy<Action> _runActionLazy;
        private readonly Lazy<object[]> _parametersLazy;

        /// <summary>
        /// Constructs a new <see cref="Agent" />.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="interval"></param>
        /// <param name="parameters"></param>
        private Agent(string type, string method, TimeSpan interval, IEnumerable<Param> parameters)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");

            if (string.IsNullOrEmpty(method))
                throw new ArgumentNullException("method");

            if (interval == null)
                throw new ArgumentNullException("interval");

            if (parameters == null)
                throw new ArgumentNullException("parameters");

            // Set fields
            _type = type;
            _method = method;
            _interval = interval;
            _parameters = parameters;

            // Initialize lazy properties
            _instanceLazy = new Lazy<object>(GetInstance, LazyThreadSafetyMode.ExecutionAndPublication);
            _runDelegateLazy = new Lazy<MethodInfo>(() => Instance.GetType().GetMethod(_method),
                LazyThreadSafetyMode.ExecutionAndPublication);
            _runActionLazy = new Lazy<Action>(() => !Parameters.Any()
                ? Expression.Lambda<Action>(Expression.Call(Expression.Constant(Instance), Delegate)).Compile()
                : null, LazyThreadSafetyMode.ExecutionAndPublication);
            _parametersLazy = new Lazy<object[]>(GetParams, LazyThreadSafetyMode.ExecutionAndPublication);
            _lastRun = DateTime.Now;
        }

        /// <summary>
        /// The type of the <see cref="Agent" />.
        /// </summary>
        public string Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Indicates if the <see cref="Agent" /> is due to be executed.
        /// </summary>
        public bool IsDue
        {
            get
            {
                DateTime now = DateTime.Now;
                return !_hasRun || (now > _lastRun && now.Subtract(_lastRun) > Interval);
            }
        }

        /// <summary>
        /// Indicates the interval which the <see cref="Agent" /> should be executed.
        /// </summary>
        public TimeSpan Interval
        {
            get { return _interval; }
        }

        /// <summary>
        /// Indicates the time at which the <see cref="Agent" /> was last executed.
        /// </summary>
        public DateTime LastRun
        {
            get { return _lastRun; }
        }

        /// <summary>
        /// The actual instance represented by the current <see cref="Agent" />.
        /// </summary>
        private object Instance
        {
            get { return _instanceLazy.Value; }
        }

        /// <summary>
        /// Value factory for setting the <see cref="Instance" /> property.
        /// </summary>
        /// <returns></returns>
        private object GetInstance()
        {
            Type type = TypeHelper.GetType(Type);

            if (type == null)
                throw new AgentException(string.Format("Could not create agent instance of type {0}.", Type));

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// A reference to the entry method of the actual <see cref="Agent" /> instance.
        /// </summary>
        private MethodInfo Delegate
        {
            get { return _runDelegateLazy.Value; }
        }

        /// <summary>
        /// A reference to the entry method of the actual <see cref="Agent" /> instance.
        /// Is only set if it is a parameterless method.
        /// </summary>
        private Action DelegateAction
        {
            get { return _runActionLazy.Value; }
        }

        /// <summary>
        /// The parameter values of the entry method.
        /// </summary>
        private object[] Parameters
        {
            get { return _parametersLazy.Value; }
        }

        /// <summary>
        /// Value factory for setting the <see cref="Parameters" /> property.
        /// </summary>
        /// <returns></returns>
        private object[] GetParams()
        {
            ParameterInfo[] delegateParameters =
                Delegate.GetParameters().ToArray();
            Dictionary<string, string> parameters = _parameters.ToDictionary(
                x => x.Name.ToLower(CultureInfo.InvariantCulture),
                x => x.Value);

            object[] sortedParams = new object[delegateParameters.Length];

            for (int i = 0; i < delegateParameters.Length; i++)
            {
                ParameterInfo delegateParameter = delegateParameters[i];
                string parameterName = delegateParameter.Name;
                string parameterValue = parameters.ContainsKey(parameterName)
                    ? parameters[parameterName]
                    : null;

                sortedParams[i] = GetParamValue(delegateParameter.ParameterType, parameterValue);
            }

            return sortedParams;
        }

        private static object GetParamValue(Type parameterType, string parameterValue)
        {
            if (parameterType == null)
                throw new ArgumentNullException("parameterType");

            if (parameterValue == null)
                return null;

            if (parameterType == typeof(string))
                return parameterValue;

            // Check for enumerable
            if (!parameterType.IsEnumerable())
            {
                try
                {
                    // Try to change type
                    return Convert.ChangeType(parameterValue, parameterType);
                }
                catch (Exception) { { } }

                // Fallback to string
                return parameterValue;
            }

            IEnumerable<string> enumerable = parameterValue.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            Type entriesType = parameterType.IsGenericType ? parameterType.GetGenericArguments().FirstOrDefault() : null;

            // Try to change type of all of the entries in the enumerable
            if (entriesType != null && entriesType != typeof(string))
            {
                try
                {
                    // Convert type of all objects in collection
                    IEnumerable objects = enumerable.Select(x => Convert.ChangeType(x, entriesType));

                    // Cast it to the correct IEnumerable<T>
                    objects = ((Func<IEnumerable, IEnumerable<object>>)(Enumerable.Cast<object>)).GetMethodInfo()
                        .GetGenericMethodDefinition().MakeGenericMethod(entriesType).Invoke(null, new object[] { objects }) as IEnumerable;

                    // Enumerate the CastIterator<T> by invoking ToList<T>
                    return ((Func<List<object>, IEnumerable<object>>)(Enumerable.ToList)).GetMethodInfo()
                        .GetGenericMethodDefinition().MakeGenericMethod(entriesType)
                        .Invoke(null, new object[] { objects });
                }
                catch (Exception) { { } }
            }

            return enumerable;
        }

        /// <summary>
        /// Retrieves an <see cref="Agent" /> of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Agent GetAgent(string type, string method)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");

            string key = string.Format("{0}#{1}", type, method);

            return Agents.GetOrAdd(key, x => new Lazy<Agent>(() =>
            {
                Config config = Config.ReadConfig();
                Configuration.Scheduling.Agent agentConfig =
                    config.Scheduling.FirstOrDefault(y => string.Equals(y.Type, type, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(y.Method, method, StringComparison.InvariantCultureIgnoreCase));

                if (agentConfig == null)
                    throw new ArgumentException(string.Format("Could not find agent of type '{0}' with method '{1}'.", type, method));

                // Read configuration attributes
                TimeSpan interval = TimeSpan.Parse(agentConfig.Interval);
                IEnumerable<Param> param = agentConfig.Select(y => new Param { Name = y.Name, Value = y.Value });
                return new Agent(agentConfig.Type, agentConfig.Method, interval, param);
            }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        /// <summary>
        /// Schedules the <see cref="Agent"/> to be run.
        /// </summary>
        public void Schedule()
        {
            // Update last run
            _hasRun = true;
            _lastRun = DateTime.Now;
            _handleLock.Release();
        }

        /// <summary>
        /// Executes the <see cref="Agent" />.
        /// </summary>
        protected override void Run()
        {
            while (true)
            {
                _handleLock.Wait();

                try
                {
                    if (DelegateAction != null)
                    {
                        DelegateAction();
                    }
                    else
                    {
                        Delegate.Invoke(Instance, Parameters);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.InnerException ?? ex);
                    throw;
                }
            }
        }
    }
}