// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace TestLoginApp
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField passwordEntry { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton signInBtn { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel status { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField usernameEntry { get; set; }

		[Action ("SignInBtn_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void SignInBtn_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (passwordEntry != null) {
				passwordEntry.Dispose ();
				passwordEntry = null;
			}
			if (signInBtn != null) {
				signInBtn.Dispose ();
				signInBtn = null;
			}
			if (status != null) {
				status.Dispose ();
				status = null;
			}
			if (usernameEntry != null) {
				usernameEntry.Dispose ();
				usernameEntry = null;
			}
		}
	}
}
