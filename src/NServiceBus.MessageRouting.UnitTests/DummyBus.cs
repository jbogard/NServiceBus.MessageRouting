//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Threading;
//using System.Threading.Tasks;
//using FakeItEasy;

//namespace NServiceBus.MessageRouting.UnitTests
//{
//    // Infrastructure

//    public class Bus : IBusContext
//    {
//        private readonly Queue<object> publishedMessages = new Queue<object>();

//        private readonly Queue<object> sentMessages = new Queue<object>();

//        private readonly Queue<object> localMessages = new Queue<object>();

//        private readonly Queue<object> repliedMessages = new Queue<object>();

//        private readonly Queue<Tuple<string, object>> _explicitlySent = new Queue<Tuple<string, object>>(); 

//        private readonly Queue<DeferredMessage> deferredMessages = new Queue<DeferredMessage>();

//        private readonly List<string> forwardedMessages = new List<string>();

//        private readonly List<Tuple<Func<object>, Func<object, bool>>> sentLocalHandlers = new List<Tuple<Func<object>, Func<object, bool>>>();

//        private readonly List<Tuple<Func<object>, Func<object, bool>>> deferHandlers = new List<Tuple<Func<object>, Func<object, bool>>>();

//        public Bus()
//        {
//            this.OutgoingHeaders = new Dictionary<string, string>();
//            this.CurrentMessageContext = new MessageContext();

//            // here we manuelly hook up our conventions in order to get passed the saga checks
//            //MessageConventionExtensions.IsMessageTypeAction = Conventions.Messages;
//            //MessageConventionExtensions.IsCommandTypeAction = Conventions.Commands;
//            //MessageConventionExtensions.IsEventTypeAction = Conventions.Events;
//        }

//        public void Shutdown()
//        {
//        }

//        public IDictionary<string, string> OutgoingHeaders { get; private set; }

//        public IMessageContext CurrentMessageContext
//        {
//            get;
//            private set;
//        }

//        public IInMemoryOperations InMemory { get; private set; }

//        //public IInMemoryOperations InMemory { get; private set; }

//        public IEnumerable<object> Published
//        {
//            get
//            {
//                return this.publishedMessages;
//            }
//        }

//        public IEnumerable<object> Sent
//        {
//            get
//            {
//                return this.sentMessages;
//            }
//        }

//        public IEnumerable<object> Local
//        {
//            get
//            {
//                return this.localMessages;
//            }
//        }

//        public IEnumerable<object> Replied
//        {
//            get
//            {
//                return this.repliedMessages;
//            }
//        }

//        public IEnumerable<string> Forwarded { get { return forwardedMessages; } } 

//        public IEnumerable<Tuple<string, object>> ExplicitlySent
//        {
//            get { return _explicitlySent; }
//        } 

//        public IEnumerable<DeferredMessage> DeferredMessages
//        {
//            get { return this.deferredMessages; }
//        }

//        public bool HandleCurrentMessageLaterWasCalled { get; set; }

//        public bool DoNotContinueDispatchingCurrentMessageToHandlersWasCalled { get; set; }

//        public void SetupForSendLocal<TMessage>(
//            Func<IHandleMessages<TMessage>> handlerCreator)
//            where TMessage : class
//        {
//            this.sentLocalHandlers.Add(
//                new Tuple<Func<object>, Func<object, bool>>(
//                    handlerCreator, msg => (msg as TMessage) != null));
//        }

//        public void SetupForDefer<TMessage>(
//            Func<IHandleMessages<TMessage>> handlerCreator)
//            where TMessage : class
//        {
//            this.deferHandlers.Add(
//                new Tuple<Func<object>, Func<object, bool>>(
//                    handlerCreator, msg => (msg as TMessage) != null));
//        }

//        public T CreateInstance<T>()
//        {
//            return A.Fake<T>();
//        }

//        public T CreateInstance<T>(Action<T> action)
//        {
//            var message = A.Fake<T>();
//            action(message);
//            return message;
//        }

//        public object CreateInstance(Type messageType)
//        {
//            return this.GetType().GetMethod("CreateInstance", new[] { messageType }).Invoke(this, null);
//        }

//        public void Publish<T>(params T[] messages)
//        {
//            foreach (var message in messages)
//            {
//                this.publishedMessages.Enqueue(message);
//            }
//        }

//        public void Publish<T>(T message)
//        {
//            publishedMessages.Enqueue(message);
//        }

//        public void Publish<T>()
//        {
//            var message = Activator.CreateInstance<T>();
//            publishedMessages.Enqueue(message);
//        }

//        public void Publish<T>(Action<T> messageConstructor)
//        {
//            var message = this.CreateInstance(messageConstructor);
//            this.publishedMessages.Enqueue(message);
//        }

//        public void Subscribe(Type messageType)
//        {
//        }

//        public void Subscribe<T>()
//        {
//        }

//        public void Subscribe(Type messageType, Predicate<object> condition)
//        {
//        }

//        public void Subscribe<T>(Predicate<T> condition)
//        {
//        }

//        public void Unsubscribe(Type messageType)
//        {
//        }

//        public void Unsubscribe<T>()
//        {
//        }

//        public ICallback SendLocal(params object[] messages)
//        {
//            foreach (var message in messages)
//            {
//                this.localMessages.Enqueue(message);

//                this.DynamicInvokeHandle(message, this.sentLocalHandlers);
//            }

//            return new Callback();
//        }

//        public ICallback SendLocal(object message)
//        {
//            localMessages.Enqueue(message);
//            return new Callback();
//        }

//        public ICallback SendLocal<T>(Action<T> messageConstructor)
//        {
//            var message = this.CreateInstance(messageConstructor);
//            this.localMessages.Enqueue(message);

//            this.DynamicInvokeHandle(message, this.sentLocalHandlers);

//            return new Callback();
//        }

//        public ICallback Send(params object[] messages)
//        {
//            foreach (var message in messages)
//            {
//                this.sentMessages.Enqueue(message);
//            }

//            return new Callback();
//        }

//        public ICallback Send(object message)
//        {
//            sentMessages.Enqueue(message);
//            return new Callback();
//        }

//        public ICallback Send<T>(Action<T> messageConstructor)
//        {
//            var message = this.CreateInstance(messageConstructor);
//            this.sentMessages.Enqueue(message);
//            return new Callback();
//        }

//        public ICallback Send(string destination, params object[] messages)
//        {
//            foreach (var item in messages.Select(message => new Tuple<string, object>(destination, message)))
//            {
//                _explicitlySent.Enqueue(item);
//            }

//            return new Callback();
//        }

//        public ICallback Send(string destination, object message)
//        {
//            _explicitlySent.Enqueue(new Tuple<string, object>(destination, message));
//            return new Callback();
//        }

//        public ICallback Send(Address address, params object[] messages)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send(Address address, object message)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send<T>(string destination, Action<T> messageConstructor)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send<T>(Address address, Action<T> messageConstructor)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send(string destination, string correlationId, params object[] messages)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send(string destination, string correlationId, object message)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send(Address address, string correlationId, params object[] messages)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send(Address address, string correlationId, object message)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send<T>(string destination, string correlationId, Action<T> messageConstructor)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Send<T>(Address address, string correlationId, Action<T> messageConstructor)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback SendToSites(IEnumerable<string> siteKeys, params object[] messages)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback SendToSites(IEnumerable<string> siteKeys, object message)
//        {
//            DoNotCallThisMethod();

//            return new Callback();
//        }

//        public ICallback Defer(TimeSpan delay, params object[] messages)
//        {
//            this.deferredMessages.Enqueue(new DeferredMessage(messages.ToList(), delay));

//            foreach (var message in messages)
//            {
//                this.DynamicInvokeTimeout(message, this.deferHandlers);
//            }

//            return new Callback();
//        }

//        public ICallback Defer(TimeSpan delay, object message)
//        {
//            deferredMessages.Enqueue(new DeferredMessage(message, delay));
//            return new Callback();
//        }

//        public ICallback Defer(DateTime processAt, params object[] messages)
//        {
//            this.deferredMessages.Enqueue(new DeferredMessage(messages, processAt));

//            foreach (var message in messages)
//            {
//                this.DynamicInvokeTimeout(message, this.deferHandlers);
//            }

//            return new Callback();
//        }

//        public ICallback Defer(DateTime processAt, object message)
//        {
//            deferredMessages.Enqueue(new DeferredMessage(message, processAt));
//            return new Callback();
//        }

//        public void Reply(params object[] messages)
//        {
//            foreach (var message in messages)
//            {
//                Reply(message);
//            }
//        }

//        public void Reply(object message)
//        {
//            repliedMessages.Enqueue(message);
//        }

//        public void Reply<T>(Action<T> messageConstructor)
//        {
//            var message = this.CreateInstance(messageConstructor);
//            this.repliedMessages.Enqueue(message);
//        }

//        public void Return<T>(T errorEnum)
//        {
//            throw new NotImplementedException();
//        }

//        public void HandleCurrentMessageLater()
//        {
//            this.HandleCurrentMessageLaterWasCalled = true;
//        }

//        public void ForwardCurrentMessageTo(string destination)
//        {
//            forwardedMessages.Add(destination);
//        }

//        public void DoNotContinueDispatchingCurrentMessageToHandlers()
//        {
//            this.DoNotContinueDispatchingCurrentMessageToHandlersWasCalled = true;
//        }

//        private static void DoNotCallThisMethod(string memberName = "")
//        {
//            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "You should not use {0} in production.", memberName));
//        }

//        private void DynamicInvokeHandle(object message, IEnumerable<Tuple<Func<object>, Func<object, bool>>> handlers)
//        {
//            foreach (var tuple in handlers)
//            {
//                if (tuple.Item2(message))
//                {
//                    var handler = tuple.Item1();
//                    var method = handler.GetType().GetMethod("Handle", new[] { message.GetType() });
//                    method.Invoke(handler, new[] { message });
//                }
//            }
//        }

//        private void DynamicInvokeTimeout(object message, IEnumerable<Tuple<Func<object>, Func<object, bool>>> handlers)
//        {
//            foreach (var tuple in handlers)
//            {
//                if (tuple.Item2(message))
//                {
//                    var handler = tuple.Item1();
//                    var method = handler.GetType().GetMethod("Timeout", new[] { message.GetType() });
//                    method.Invoke(handler, new[] { message });
//                }
//            }
//        }

//        private class MessageContext : IMessageContext
//        {
//            public MessageContext()
//            {
//                this.Headers = new Dictionary<string, string>();
//                this.Id = Guid.NewGuid().ToString();
//                this.ReturnAddress = "localhost";
//                this.ReplyToAddress = Address.Parse("localhost");
//                this.TimeSent = DateTime.UtcNow;
//            }

//            public string Id { get; private set; }

//            public string ReturnAddress { get; private set; }

//            public Address ReplyToAddress { get; private set; }

//            public DateTime TimeSent { get; private set; }

//            public IDictionary<string, string> Headers { get; private set; }
//        }

//        /// <summary>
//        /// This is a very naive and first implementation of ICallback
//        /// </summary>
//        private class Callback : ICallback, IAsyncResult
//        {
//            public bool IsCompleted { get; private set; }

//            public WaitHandle AsyncWaitHandle { get; private set; }

//            public object AsyncState { get; private set; }

//            public bool CompletedSynchronously { get; private set; }

//            public Task<int> Register()
//            {
//                throw new NotImplementedException();
//            }

//            public Task<T> Register<T>()
//            {
//                throw new NotImplementedException();
//            }

//            public Task<T> Register<T>(Func<CompletionResult, T> completion)
//            {
//                throw new NotImplementedException();
//            }

//            public Task Register(Action<CompletionResult> completion)
//            {
//                throw new NotImplementedException();
//            }

//            public IAsyncResult Register(AsyncCallback callback, object state)
//            {
//                this.AsyncState = state;
//                this.IsCompleted = true;
//                this.CompletedSynchronously = true;
//                this.AsyncWaitHandle = new AutoResetEvent(true);

//                callback(this);

//                return this;
//            }

//            public void Register<T>(Action<T> callback)
//            {
//                callback(default(T));
//            }

//            public void Register<T>(Action<T> callback, object synchronizer)
//            {
//                callback(default(T));
//            }
//        }

//        public void Dispose()
//        {
            
//        }

//        public Action<object, string, string> SetHeaderAction { get
//        {
//            return (msg, header, value) => CurrentMessageContext.Headers[header] = value;
//        } }

//        public Func<object, string, string> GetHeaderAction { get
//        {
//            return (msg, header) => CurrentMessageContext.Headers[header];
//        } }
//    }

//    public class DeferredMessage : List<object>
//    {
//        public DeferredMessage(IEnumerable<object> message, DateTime excecutionTime)
//        {
//            this.AddRange(message);

//            this.ExecutionDate = excecutionTime;
//        }

//        public DeferredMessage(IEnumerable<object> message, TimeSpan executionTimeSpan)
//        {
//            this.AddRange(message);

//            this.ExecutionTimeSpan = executionTimeSpan;
//        }

//        public DeferredMessage(object message, DateTime excecutionTime)
//        {
//            this.Add(message);

//            this.ExecutionDate = excecutionTime;
//        }

//        public DeferredMessage(object message, TimeSpan executionTimeSpan)
//        {
//            this.Add(message);

//            this.ExecutionTimeSpan = executionTimeSpan;
//        }

//        public DateTime? ExecutionDate { get; set; }

//        public TimeSpan? ExecutionTimeSpan { get; set; }
//    }
//}