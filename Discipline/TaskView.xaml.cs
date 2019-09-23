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
using static HenrysDevLib.Misc.Time;

namespace Discipline
{
    /// <summary>
    /// Interaktionslogik für TaskView.xaml
    /// </summary>
    public partial class TaskView : Page
    {
        private FileHandler fileHandler;
        private bool[] calendar;
        private int currentYear;
        private string TaskName { get; }

        public TaskView(string taskName)
        {
            InitializeComponent();

            fileHandler = new FileHandler();
            currentYear = DateTime.Today.Year;
            this.TaskName = taskName;
            DrawEntireYear(currentYear);
        }

        private void DrawEntireYear(int year)
        {
            Grid_Calendar.Children.RemoveRange(0, Grid_Calendar.Children.Count);
            bool ly = IsLeapYear(year);
            calendar = fileHandler.ReadDcf(TaskName, year);

            int gapInPx = 3;
            double buttonWidth = (Grid_Calendar.Width - 10 + gapInPx) / 31 - gapInPx;
            double buttonHeight = (Grid_Calendar.Height - 10 + gapInPx) / 12 - gapInPx;
            for (Month m = Month.January; m <= Month.December; m++)
            {
                int init = IndexOfFirstDayInMonth(m, ly);
                for (int index = init; index < init + GetDaysMonth(m, ly); index++)
                {
                    Button b = new Button
                    {
                        Width = buttonWidth,
                        Height = buttonHeight,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness((index - IndexOfFirstDayInMonth(m, ly)) * (buttonWidth + gapInPx), ((int)m - 1) * (buttonHeight + gapInPx), 0, 0),
                        Background = calendar[index] ? Brushes.Green : Brushes.Red,
                        Visibility = Visibility.Visible
                    };
                    Grid_Calendar.Children.Add(b);
                }
            }
        }

        private void DrawMonth(Month month)
        {
            throw new NotImplementedException();
        }

        private void Button_PreviousYear_Click(object sender, RoutedEventArgs e)
        {
            currentYear--;
            DrawEntireYear(currentYear);
        }

        private void Button_NextYear_Click(object sender, RoutedEventArgs e)
        {
            currentYear++;
            DrawEntireYear(currentYear);
        }
    }
}
