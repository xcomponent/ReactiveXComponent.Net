
[![](http://slack.xcomponent.com/badge.svg)](http://slack.xcomponent.com/)

# .Net Reactive XComponent API

<img src="logo.png" width="64" height="64" />

ReactiveXComponent.Net is a .Net client API that allows you to interact with microservices generated with XComponent software.

## Install
[![ReactiveXComponent.Net Nuget](https://img.shields.io/nuget/v/ReactiveXComponent.Net.svg)](https://www.nuget.org/packages/ReactiveXComponent.Net) [![ReactiveXComponent.Net Downloads](https://img.shields.io/nuget/dt/ReactiveXComponent.Net.svg)](https://www.nuget.org/packages/ReactiveXComponent.Net)

Use Nuget to install the latest version of the API:
``` nuget install ReactiveXComponent.Net -Pre```

## Usage

Usage example of ReactiveXComponent.Net API
```csharp
// All the info we need is in the xcApi file..
var xcApiStream = new FileStream("HelloWorld.xcApi", FileMode.Open);

// Get a XComponentApi..
IXComponentApi xcApi = XComponentApi.CreateFromXCApi(xcApiStream);

// Create a session..
using (IXCSession _xcSession = xcApi.CreateSession())
{
    var componentName = "HelloWorld";
    var helloWorldManagerStateMachineName = "HelloWorldManager";
    var helloWorldResponseStateMachineCode = 1837059171;

    // Create a subscriber..
    var subscriber = _xcSession.CreateSubscriber(componentName);

    // Create a publisher..
    var publisher = _xcSession.CreatePublisher(componentName);

    // Need to call subscribe for the state machine we are interested in..
    subscriber.Subscribe(helloWorldManagerStateMachineName, arg => {});

    // Subscribe to state machine updates via the IObservable collection..
    var eventReceived = new ManualResetEvent(false);
    var observer = Observer.Create<MessageEventArgs>(args =>
    {
        Console.WriteLine(args.MessageReceived);
        eventReceived.Set();
    });

    var subscription = subscriber.StateMachineUpdatesStream
                                .Where(e => e.StateMachineRefHeader.StateMachineCode == helloWorldResponseStateMachineCode)
                                .Subscribe(observer);

    // Send an event to a state machine..
    var sayHiEvent = new SayHi() { To = "World" };

    publisher.SendEvent(helloWorldManagerStateMachineName, sayHiEvent);

    // Wait for state machine update and dispose subscription..
    eventReceived.WaitOne(5000);

    subscription.Dispose();
}
```

## Build from source
Download the source code and execute the following command in a PowerShell:
``` 
.\build.ps1 -Target <target> -ScriptArgs '-buildConfiguration="<build_configuration>" -buildVersion="<build_version>"'
```
Here is an exmaple:
```
.\build.ps1 -Target Build -ScriptArgs '-buildConfiguration="Debug" -buildVersion="1.0.0.1"'
```


**target** can be one of these values:   
*Clean*  
*RestoreNugetPackages*  
*Build*  
*Test*  
*CreatePackage*  
*All*  

Default value: *Build*

**buildConfiguration** can be either *Debug* or *Release*.  
Default value: *Release* 

**buildVersion** is the build version to set for the assembly or Nuget package.  
Default value: *1.0.0-build1*

## Contributing
1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push -u origin my-new-feature`
5. Submit a pull request

## License
Apache License V2

