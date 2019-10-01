
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Discipline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_AddTask_Click(object sender, RoutedEventArgs e)
        {
            DialogBoxAddTask dialogBox = new DialogBoxAddTask();
            bool? dialogResult = dialogBox.ShowDialog();
            if ((bool)dialogResult)
            {
                OpenTask(dialogBox.TaskName);
                MessageBox.Show("New task successfully created.");
            }
        }

        private void Button_OpenTask_Click(object sender, RoutedEventArgs e)
        {
            DialogBoxOpenTask db_OpenTask = new DialogBoxOpenTask();
            bool? result = db_OpenTask.ShowDialog();
            if ((bool)result)
                OpenTask(db_OpenTask.TaskName);
        }

        void OpenTask(string taskName)
        {
            TabItem taskTab = new TabItem();
            taskTab.Header = taskName;
            Frame tabFrame = new Frame();
            TaskView view = new TaskView(taskName);
            tabFrame.Content = view;
            taskTab.Content = tabFrame;
            TaskTabs.Items.Add(taskTab);
            TaskTabs.SelectedIndex = TaskTabs.Items.Count - 1;
            view.Button_CloseTask.Click += (o, e) =>
            {
                TaskTabs.Items.Remove(taskTab);
            };
        }
    }
}
