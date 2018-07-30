﻿using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3415, "[Android] Swipe Command fires twice on Android for each swipe action", PlatformAffected.Android)]
	public class Issue3415 : TestContentPage
	{
		protected override void Init()
		{
			Label rightSwipeFired = new Label();
			Label leftSwipeFired = new Label();

			Content = new StackLayout()
			{
				Children = {
						new Label(){ Text = "Swipe Anywhere on the Screen. Android fires swipe events twice"},
						rightSwipeFired,
						leftSwipeFired
					},
				BackgroundColor = Color.Green
			};

			int right = 0;
			int left = 0;

			Content
				.GestureRecognizers
				.Add(new SwipeGestureRecognizer()
				{
					Direction = SwipeDirection.Right,
					Command = new Command(() =>
					{
						right++;
						rightSwipeFired.Text = $"Right Swipe: {right}";
					})
				});

			Content
				.GestureRecognizers
				.Add(new SwipeGestureRecognizer()
				{
					Direction = SwipeDirection.Left,
					Command = new Command(() =>
					{
						left++;
						leftSwipeFired.Text = $"Left Swipe: {left}";
					})
				});
		}

#if UITEST
		[Test]
		public void Issue3415Test ()
		{
			RunningApp.Screenshot ("I am at Issue 1");
			RunningApp.WaitForElement (q => q.Marked ("IssuePageLabel"));
			RunningApp.Screenshot ("I see the Label");
		}
#endif
	}
}