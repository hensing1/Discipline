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
    /// Interaktionslogik für DialogBoxOpenTask.xaml
    /// </summary>
    public partial class DialogBoxOpenTask : Window
    {
        private FileHandler handler;
        public string TaskName { get; private set; } = String.Empty;
        public DialogBoxOpenTask()
        {
            handler = new FileHandler();
            InitializeComponent();
            Console.WriteLine(ListBox_Tasks.SelectedIndex);
            ListBox_Tasks.ItemsSource = handler.GetTasks();
        }

        private void Button_CreateTask_Click(object sender, RoutedEventArgs e)
        {
            DialogBoxAddTask db_AddTask = new DialogBoxAddTask();
            bool? result = db_AddTask.ShowDialog();
            if ((bool)result)
            {
                ListBox_Tasks.ItemsSource = handler.GetTasks();
                ListBox_Tasks.SelectedItem = db_AddTask.TaskName;
            }
        }

        private void Button_OpenTask_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox_Tasks.SelectedIndex != -1)
            {
                TaskName = (string)ListBox_Tasks.SelectedItem;
                this.DialogResult = true;
            }
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ListBox_Tasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Tasks.SelectedIndex != -1)
                Button_OpenTask.IsEnabled = true;
        }
    }
}
