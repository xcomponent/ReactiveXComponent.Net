using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using PerseusLib.Common;
using PerseusLib.Common.Helpers;
using XCClientLib;
using XCClientLib.Common;
using XComponent.HelloWorld.UserObject;
using XComponent.UserManagement.UserObject;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var myApi = new XClientSender("T4//PerseusApi.xcApi"))
            {
                string password = Md5Helper.CreateMD5Password(String.Empty);
                string login = "yahya.najar";
                var sessionGuid = IdHelper.GetUserSessionId(login, password);
                myApi.InitTopic(sessionGuid);
                myApi.Init();
                string label = String.Empty;
                myApi.UserManagement_Component.UserSession_StateMachine.UserSessionInfoUserSession+=
                     instance =>
                    {
                        label = instance.Status;
                        Console.WriteLine(label);
            
                    };
                myApi.SendOpenSession(new OpenSession {Login = login, Password = password}, Visibility.Private);
            
                Console.ReadLine();
            }
        }
    }
}
