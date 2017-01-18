using System;
using PerseusLib.Common;
using PerseusLib.Common.Helpers;
using UIKit;
using XCClientLib;
using XCClientLib.Common;
using XComponent.UserManagement.UserObject;

namespace TestLoginApp
{
    public partial class ViewController : UIViewController
    {
        private XClientSender myApi;
        private string label = String.Empty;
        private User user;

        public ViewController(IntPtr handle) : base(handle)
        {
            myApi = new XClientSender("PerseusApi.xcApi");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            passwordEntry.SecureTextEntry = true;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void waitResponse()
        {
            try
            {
                while (label == user.Username || label == "Checking")
                {
                    myApi.UserManagement_Component.UserSession_StateMachine.UserSessionInfoUserSession +=
                        delegate (UserSessionInfo instance)
                        {
                            label = instance.Status;
                        };
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        partial void SignInBtn_TouchUpInside(UIButton sender)
        {
            user = new User
            {
                Username = usernameEntry.Text,
                Password = passwordEntry.Text
            };

            string password = Md5Helper.CreateMD5Password(user.Password);
            string login = user.Username;
            var sessionGuid = IdHelper.GetUserSessionId(login, password);
            myApi.InitTopic(sessionGuid);
            myApi.Init();
            myApi.SendOpenSession(new OpenSession { Login = login, Password = password }, Visibility.Private);

            label = login;
            waitResponse();

            status.Text = label;
        }
    }
}