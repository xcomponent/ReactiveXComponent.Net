Description of the different projects:  
- XClientSender: a project that creates a library used to create XCClientApi from a T4.
- XCClientApiGenerator: a project that creates the XCClientApi. It has two args: a file ".cs" (the clientApi file) and an Xml file (Microservices description)   
- Electryon: a console application that tests the XCClientApi by simulating a login event.
- TestLoginApp: a mobile application that uses XCClientApi to login on Perseus. 
- XCClient: a project that creates a library used to connect the XCClientApi to Perseus. 
 