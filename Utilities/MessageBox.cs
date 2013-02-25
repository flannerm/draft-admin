/*
 * Author: mukapu (http://geekswithblogs.net/mukapu)
 * Feel free to use it as you want it! 
 * Comments welcome!
 * 
 * One world, one team, one goal - let's win!
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Practices.Composite.Presentation.Commands;
using DraftAdmin.Commands;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace UserPrompt
{
    /// <summary>
    /// MessageBox class to wrap the standard windows message box and expose MVVM friendly.
    /// DesignTimeVisible property is set to false so it doesn't show up in the designer.
    /// </summary>
    [DesignTimeVisible(false)]
    public class MessageBox : Control
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MessageBox()
        {
            // The control doesn't have any specific rendering of its own.
            Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Property to trigger the display of the message box. Whenever this (implicitly the variable it is bound to) is set a true, the message box will display
        /// </summary>
        public bool Trigger
        {
            get
            {
                return (bool)GetValue(TriggerProperty);
            }
            set
            {
                SetValue(TriggerProperty, value);
            }
        }

        /// <summary>
        /// Type of message box. Can take values "Info", "OkCancel", "YesNo", "YesNoCancel". 
        /// The appropriate dependency property should be set as:
        /// If "Info", no action property is required. 
        /// For "OkCancel", OkAction and CancelAction is required.
        /// If "YesNo" and "YesNoCancel", YesAction and NoAction are required.
        /// For "YesNoCancel" in addition, CancelAction should be specified.
        /// </summary>
        public string Type
        {
            get
            {
                return (string)GetValue(TypeProperty);
            }
            set
            {
                SetValue(TypeProperty, value);
            }
        }

        /// <summary>
        /// On a Ok/Cancel dialog, this will Execute the bound DelegateCommand on the user clicking "Ok".
        /// </summary>
        public DelegateCommand<object> OkAction
        {
            get
            {
                return (DelegateCommand<object>)GetValue(OkActionProperty);
            }
            set
            {
                SetValue(OkActionProperty, value);
            }
        }

        /// <summary>
        /// On a Yes/No or Yes/No/Cancel dialog, this will Execute the bound DelegateCommand on the user clicking "Yes".
        /// </summary>
        public DelegateCommand<object> YesAction
        {
            get
            {
                return (DelegateCommand<object>)GetValue(YesActionProperty);
            }
            set
            {
                SetValue(YesActionProperty, value);
            }
        }

        /// <summary>
        /// On a Yes/No or Yes/No/Cancel dialog, this will Execute the bound DelegateCommand on the user clicking "No".
        /// </summary>
        public DelegateCommand<object> NoAction
        {
            get
            {
                return (DelegateCommand<object>)GetValue(NoActionProperty);
            }
            set
            {
                SetValue(NoActionProperty, value);
            }
        }

        /// <summary>
        /// On a Yes/No/Cancel or Ok/Cancel dialog, this will Execute the bound DelegateCommand on the user clicking "Cancel".
        /// </summary>
        public DelegateCommand<object> CancelAction
        {
            get
            {
                return (DelegateCommand<object>)GetValue(NoActionProperty);
            }
            set
            {
                SetValue(NoActionProperty, value);
            }
        }

        /// <summary>
        /// The message to show the user.
        /// </summary>
        public string Message
        {
            get
            {
                return (string)GetValue(MessageProperty);
            }
            set
            {
                SetValue(MessageProperty, value);
            }
        }

        /// <summary>
        /// The message box caption/title to show the user.
        /// </summary>
        public string Caption
        {
            get
            {
                return (string)GetValue(CaptionProperty);
            }
            set
            {
                SetValue(CaptionProperty, value);
            }
        }

        /// <summary>
        /// DependencyProperty for "Trigger". This also overrides the PropertyChangedCallback to trigger the message box display.
        /// </summary>
        public static readonly DependencyProperty TriggerProperty = DependencyProperty.Register("Trigger", typeof(bool), typeof(MessageBox),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTriggerChange)));
        /// <summary>
        /// DependencyProperty for "Type".
        /// </summary>
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(string), typeof(MessageBox));
        /// <summary>
        /// DependencyProperty for "OkAction".
        /// </summary>
        public static readonly DependencyProperty OkActionProperty = DependencyProperty.Register("OkAction", typeof(DelegateCommand<object>), typeof(MessageBox));
        /// <summary>
        /// DependencyProperty for "YesAction".
        /// </summary>
        public static readonly DependencyProperty YesActionProperty = DependencyProperty.Register("YesAction", typeof(DelegateCommand<object>), typeof(MessageBox));
        /// <summary>
        /// DependencyProperty for "NoAction".
        /// </summary>
        public static readonly DependencyProperty NoActionProperty = DependencyProperty.Register("NoAction", typeof(DelegateCommand<object>), typeof(MessageBox));
        /// <summary>
        /// DependencyProperty for "CancelAction".
        /// </summary>
        public static readonly DependencyProperty CancelActionProperty = DependencyProperty.Register("CancelAction", typeof(DelegateCommand<object>), typeof(MessageBox));
        /// <summary>
        /// DependencyProperty for "Message".
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MessageBox));
        /// <summary>
        /// DependencyProperty for "Caption".
        /// </summary>
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(MessageBox));

        /// <summary>
        /// The "Trigger" propery changed override. Whenever the "Trigger" property changes to true or false this will be executed.
        /// When the property changes to true, the message box will be shown.
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void OnTriggerChange(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            MessageBox messageBox = (MessageBox)dependencyObject;
            if (!messageBox.Trigger) return;

            switch (messageBox.Type)
            {
                case "Info":
                    messageBox.ShowInfo();
                    break;
                case "Warning":
                    messageBox.ShowWarning();
                    break;
                case "OkCancel":
                    messageBox.ShowOkCancel();
                    break;
                case "YesNo":
                    messageBox.ShowYesNo();
                    break;
                case "YesNoCancel":
                    messageBox.ShowYesNoCancel();
                    break;
            }
        }

        /// <summary>
        /// Displays the Info message box.
        /// </summary>
        private void ShowInfo()
        {
            System.Windows.MessageBox.Show(Message, Caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowWarning()
        {
            System.Windows.MessageBox.Show(Message, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Displays the Info message box.
        /// </summary>
        private void ShowOkCancel()
        {
            DelegateCommand<object> action;

            if (System.Windows.MessageBox.Show(Message, Caption, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.OK)
                action = OkAction;
            else
                action = CancelAction;

            action.Execute(null);
        }

        /// <summary>
        /// Displays the Ok/Cancel message box and based on user action executes the appropriate command.
        /// </summary>
        private void ShowYesNo()
        {
            DelegateCommand<object> action;

            if (System.Windows.MessageBox.Show(Message, Caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                action = YesAction;
            else
                action = NoAction;

            action.Execute(null);
        }

        /// <summary>
        /// Displays the Yes/No/Cancel message box and based on user action executes the appropriate command.
        /// </summary>
        private void ShowYesNoCancel()
        {
            DelegateCommand<object> action;

            MessageBoxResult result = System.Windows.MessageBox.Show(Message, Caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    action = YesAction;
                    break;
                case MessageBoxResult.No:
                    action = NoAction;
                    break;
                case MessageBoxResult.Cancel:
                default:
                    action = CancelAction;
                    break;
            }

            action.Execute(null);
        }
    }
}
