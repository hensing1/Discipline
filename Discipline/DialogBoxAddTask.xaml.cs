using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Discipline
{
    /// <summary>
    /// Interaction logic for DialogBoxAddTask.xaml
    /// </summary>
    public partial class DialogBoxAddTask : Window
    {
        private FileHandler handler;
        public string TaskName { get; private set; } = string.Empty;
        public DialogBoxAddTask()
        {
            InitializeComponent();
            this.handler = new FileHandler();
        }

        private void Button_CreateTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string taskName = TextBox_NewTaskName.Text;
                handler.AddTask(taskName);
                TaskName = taskName;
                this.DialogResult = true;
            }
            catch(ArgumentException ae)
            {
                MessageBox.Show(ae.Message);
            }
            catch(FormatException fe)
            {
                MessageBox.Show(fe.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
