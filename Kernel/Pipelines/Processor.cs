using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using PSOK.Kernel.Exceptions;
using PSOK.Kernel.Reflection;
using Exception = System.Exception;

namespace PSOK.Kernel.Pipelines
{
    /// <summary>
    /// Defines a processor in a <see cref="Pipeline" />.
    /// </summary>
    internal class Processor : IProcessor<PipelineArgs>
    {
        // Static fields
        private static readonly ConcurrentDictionary<string, Lazy<Processor>> Processors =
            new ConcurrentDictionary<string, Lazy<Processor>>();

        // Instance fields
        private readonly string _type;

        private readonly Lazy<object> _instanceLazy;
        private readonly Lazy<Action<PipelineArgs>> _executeActionLazy;

        /// <summary>
        /// Constructs a new <see cref="Processor" />
        /// </summary>
        /// <param name="type"></param>
        private Processor(string type)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");

            // Set fields
            _type = type;

            // Initialize lazy properties
            _instanceLazy = new Lazy<object>(GetInstance, LazyThreadSafetyMode.ExecutionAndPublication);
            _executeActionLazy = new Lazy<Action<PipelineArgs>>(GetExecuteAction,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// The type of the <see cref="IProcessor{T}" />.
        /// </summary>
        public string Type
        {
            get { return _type; }
        }

        /// <summary>
        /// The actual instance represented by the current <see cref="IProcessor{T}" />.
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
        /// A reference to the Execute method of the actual <see cref="IProcessor{T}" /> instance.
        /// </summary>
        private Action<PipelineArgs> ExecuteAction
        {
            get { return _executeActionLazy.Value; }
        }

        /// <summary>
        /// Retrieves a <see cref="Processor" /> of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IProcessor<PipelineArgs> GetProcessor(string type)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException(type);

            return
                Processors.GetOrAdd(type,
                    x => new Lazy<Processor>(() => new Processor(x), LazyThreadSafetyMode.ExecutionAndPublication))
                    .Value;
        }

        /// <summary>
        /// Executes the <see cref="Processor" />.
        /// </summary>
        /// <param name="args"></param>
        public void Execute(PipelineArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            try
            {
                ExecuteAction(args);
            }
            catch (Exception ex)
            {
                if (ex is PipelineAbortedException)
                {
                    throw;
                }

                throw new ProcessorException(string.Format("An error occurred while invoking processor '{0}'.", _type),
                    ex);
            }
        }

        /// <summary>
        /// Value factory for setting the <see cref="ExecuteAction" /> property.
        /// </summary>
        /// <returns></returns>
        private Action<PipelineArgs> GetExecuteAction()
        {
            // Get execute action from instance
            MethodInfo executeAction =
                MethodHelper.GetMethod<IProcessor<PipelineArgs>, Action<PipelineArgs>>(x => x.Execute,
                    Instance.GetType());

            if (executeAction == null)
                throw new ProcessorException("Could not get execute method from processor instance.");

            // Build expression calling the execute action since this is alot faster than using dynamic invoke
            ParameterExpression inputArgs = Expression.Parameter(typeof (PipelineArgs), "input");
            Type inputArgsType =
                Instance.GetType()
                    .GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof (IProcessor<>))
                    .Select(x => x.GetGenericArguments().First()).FirstOrDefault();

            if (inputArgsType == null)
                throw new ProcessorException("Could not determine pipeline args for execute method of processor.");

            Action<PipelineArgs> execute = Expression.Lambda<Action<PipelineArgs>>(
                Expression.Call(Expression.Constant(Instance), executeAction,
                    new Expression[] {Expression.Convert(inputArgs, inputArgsType)}),
                new[] {inputArgs}).Compile();

            return execute;
        }
    }
}