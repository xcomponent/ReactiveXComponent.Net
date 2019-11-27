using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using XComponent.HelloWorld.UserObject;
using ReactiveXComponent;
using ReactiveXComponent.Common;
using ReactiveXComponent.Connection;
using System.Reactive.Linq;


namespace CreateInstances
{
    class ProgramInBatchWithReactiveApi
    {
        private const int NumberOfInstances = 10;
        private const int StateGoneCode = 0;
        private const int StatePublisherCode = 1;
        private const int StateMachineResponseCode = -343862282;
        private const int MaxNumberOfTries = 3;
        private const int TooManyTriesExitCode = -2;
        private const string HelloWorldComponentName = "HelloWorld";
        private const string EntryPointStateMachineName = "HelloWorld";
        private const string ResponseStateMachineName = "HelloWorldResponse";
        private static TimeSpan TimeToWaitForInstanceState = TimeSpan.FromSeconds(10);

        static int Main()
        {
            var triesCount = 0;

            while (triesCount < MaxNumberOfTries)
            {
                try
                {
                    triesCount++;
                    int result = TryToRunTest();
                    return result;
                }
                catch (Exception e)
                {
                    if (triesCount == MaxNumberOfTries)
                    {
                        throw new ReactiveXComponentException("Error while running test: " + e.Message, e);
                    }
                }
            }

            Console.WriteLine("Exited test, failed to run");
            return TooManyTriesExitCode;
        }

        private static int TryToRunTest()
        {
            IXComponentApi xcApi = XComponentApi.CreateFromXCApi(@"./xcassemblies/HelloWorldV5Api.xcApi");

            using (IXCSession xcSession = xcApi.CreateSession())
            {
                IXCSubscriber subscriber = xcSession.CreateSubscriber(HelloWorldComponentName);
                IXCPublisher publisher = xcSession.CreatePublisher(HelloWorldComponentName);

                subscriber.Subscribe(ResponseStateMachineName, arg => { });

                ConcurrentDictionary<string, MessageEventArgs> createdInstanceIds = new ConcurrentDictionary<string, MessageEventArgs>();
                try
                {
                    TestCreateInstanceReactiveApi(publisher, createdInstanceIds, subscriber);

                    TestGetSnapshotWithFilter(publisher, createdInstanceIds);

                    TestDestroyInstanceReactiveApi(subscriber, createdInstanceIds, publisher);
                    
                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception message: " + e.Message + e);
                    return 1;
                }


            }
        }

        private static void TestDestroyInstanceReactiveApi(IXCSubscriber subscriber, ConcurrentDictionary<string, MessageEventArgs> createdInstanceIds,
            IXCPublisher publisher)
        {
            Console.WriteLine("Test Destroy Instance ReactiveApi");
            AutoResetEvent waitForInstanceDestruction = new AutoResetEvent(false);
            int counter = 0;
            var observer = Observer.Create<MessageEventArgs>(instance =>
            {
                Interlocked.Increment(ref counter);
                ObserverStateMachineExecuted(counter, waitForInstanceDestruction, "destroy");
            });

            using (subscriber.StateMachineUpdatesStream
                .Where(e => e.StateMachineRefHeader.StateMachineCode == StateMachineResponseCode &&
                            e.StateMachineRefHeader.StateCode == StateGoneCode)
                .Subscribe(observer))
            {
                List<MessageEventArgs> instanceList = createdInstanceIds.Values.ToList();
                foreach (var helloWorldResponseInstance in instanceList)
                {
                    publisher.SendEvent(helloWorldResponseInstance.StateMachineRefHeader, new SayGoodbye());
                }

                if (!waitForInstanceDestruction.WaitOne(TimeToWaitForInstanceState))
                {
                    Console.WriteLine("Error all instance created are not destroy");
                    throw new Exception("Error all instance created are not destroy");
                }
            }
        }

        private static void TestCreateInstanceReactiveApi(IXCPublisher publisher, ConcurrentDictionary<string, MessageEventArgs> createdInstanceIds,
            IXCSubscriber subscriber)
        {
            Console.WriteLine("Test Create Instance ReactiveApi");
            MessageEventArgs entryPointSnapshot = publisher.GetSnapshot(EntryPointStateMachineName)[0];
            AutoResetEvent waitForInstanceCreation = new AutoResetEvent(false);
            int instanceCount = 0;
            var observer = Observer.Create<MessageEventArgs>(instance =>
            {
                if (createdInstanceIds.TryAdd(instance.StateMachineRefHeader.StateMachineId, instance))
                {
                    Interlocked.Increment(ref instanceCount);
                    ObserverStateMachineExecuted(instanceCount, waitForInstanceCreation, "created");
                }
                else
                {
                    Console.WriteLine($"Duplicated instance created {instance.StateMachineRefHeader.StateMachineId}");
                    throw new Exception($"Error Duplicate instance created {instance.StateMachineRefHeader.StateMachineId}");
                }
            });

            using (subscriber.StateMachineUpdatesStream
                .Where(e => e.StateMachineRefHeader.StateMachineCode == StateMachineResponseCode &&
                            e.StateMachineRefHeader.StateCode == StatePublisherCode)
                .Subscribe(observer))
            {
                for (int i = 0; i < NumberOfInstances; i++)
                {
                    publisher.SendEvent(entryPointSnapshot.StateMachineRefHeader, new SayHello() {Name = $"toto{i}"});
                }

                if (!waitForInstanceCreation.WaitOne(TimeToWaitForInstanceState))
                {
                    Console.WriteLine("Error all instance are not created");
                    throw new Exception("Error all instance are not created");
                }
            }
        }

        private static void TestGetSnapshotWithFilter(
            IXCPublisher publisher, 
            ConcurrentDictionary<string, MessageEventArgs> createdInstanceIds)
        {
            Console.WriteLine("Testing get snapshot with filter");

            for (int i = 0; i < NumberOfInstances; i++)
            {
                var filter = $"OriginatorName == \"toto{i}\"";
                var instances = publisher.GetSnapshot(ResponseStateMachineName, filter);

                if (instances == null)
                {
                    Console.WriteLine($"Received a null snapshot for instance with filter = {filter}");
                    throw new Exception("Error while testing snapshot");
                }

                if (instances.Count != 1)
                {
                    Console.WriteLine($"Received {instances.Count} instances for get snapshot for instance with filter = {filter}. Expected only one.");
                    throw new Exception("Error while testing snapshot");
                }
            }
            
        }

        private static void ObserverStateMachineExecuted(int instanceCount, AutoResetEvent waitForInstanceState, string stateInstance)
        {
            Console.WriteLine($"{instanceCount} instance " + stateInstance);
            if (instanceCount == NumberOfInstances)
            {
                waitForInstanceState.Set();
            }
        }
    }
}
