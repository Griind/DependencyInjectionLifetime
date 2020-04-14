using System;
using System.Collections.Generic;
using System.Linq;

namespace DI
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new DependencyContainer();
            container.AddTransient<HelloService>();
            container.AddTransient<ServiceConsumer>();
            container.AddSingleton<MessageService>();

            var resolver = new DependencyResolver(container);

            var service1 = resolver.GetService<ServiceConsumer>();
            service1.Print();

            var service2 = resolver.GetService<ServiceConsumer>();
            service2.Print();

            var service3 = resolver.GetService<ServiceConsumer>();
            service3.Print();

        }
    }
    public class Dependency
    {
        public Dependency(Type type, DependencyLifeTime dependencyLifeTime)
        {
            DependencyLifeTime = dependencyLifeTime;
            Type = type;
        }
        public Type Type { get; set; }
        public DependencyLifeTime DependencyLifeTime { get; set; }
        public object Implementation { get; set; }
        public bool Implemented { get; set; }
        public void AddImplementation(object implementation)
        {
            Implementation = implementation;
            Implemented = true;
        }
    }
    public enum DependencyLifeTime
    {
        Singleton = 0,
        Transient = 1
    }
    public class DependencyResolver
    {
        private readonly DependencyContainer _container;

        public DependencyResolver(DependencyContainer container)
        {
            _container = container;
        }
        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }
        public object GetService(Type type)
        {
            var dependency = _container.GetDependency(type);
            var constructor = dependency.Type.GetConstructors().Single();
            var parameters = constructor.GetParameters().ToArray();
            if (parameters.Length > 0)
            {
                var parameterIplementations = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameterIplementations[i] = GetService(parameters[i].ParameterType);
                }
                return CreateImplementation(dependency, t => Activator.CreateInstance(t, parameterIplementations));
            }
            return CreateImplementation(dependency, t => Activator.CreateInstance(t));
        }
        public object CreateImplementation(Dependency dependency, Func<Type, object> factory)
        {
            if (dependency.Implemented)
            {
                return dependency.Implementation;
            }
            var implementation = factory(dependency.Type);
            if (dependency.DependencyLifeTime == DependencyLifeTime.Singleton)
            {
                dependency.AddImplementation(implementation);
            }
            return implementation;
        }
    }
    public class DependencyContainer
    {
        List<Dependency> _dependencies;
        public DependencyContainer()
        {
            _dependencies = new List<Dependency>();
        }
        public void AddSingleton<T>()
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifeTime.Singleton));
        }
        public void AddTransient<T>()
        {
            _dependencies.Add(new Dependency(typeof(T), DependencyLifeTime.Transient));
        }

        public Dependency GetDependency(Type type)
        {
            return _dependencies.First(x => x.Type.Name == type.Name);
        }
    }
    public class HelloService
    {
        private readonly MessageService _messageService;
        int _random;
        public HelloService(MessageService messageService)
        {
            _messageService = messageService;
            _random = new Random().Next();
        }
        public void Print()
        {
            Console.WriteLine($"Printing {_messageService.Message()} {_random}");

        }

    }
    public class ServiceConsumer
    {
        private readonly HelloService _hello;

        public ServiceConsumer(HelloService hello)
        {
            _hello = hello;
        }
        public void Print()
        {
            _hello.Print();
        }
    }
    public class MessageService
    {
        int _random;
        public MessageService()
        {
            _random = new Random().Next();
        }
        public string Message()
        {
            return $"Yo {_random}";
        }
    }
}
