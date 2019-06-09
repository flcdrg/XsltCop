using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.Shell;

namespace Gardiner.XsltTools
{
    public class Options : DialogPage, INotifyPropertyChanged
    {
        private bool _feedbackAllowed;

        [Category("General")]
        [DisplayName("Allow Feedback")]
        [Description("Determines whether crashes and de-identified usage data is submitted. Changing this value takes effect after restarting Visual Studio")]
        [DefaultValue(false)]
        public bool FeedbackAllowed
        {
            get { return _feedbackAllowed; }
            set
            {
                if (value == _feedbackAllowed)
                {
                    return;
                }

                _feedbackAllowed = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}