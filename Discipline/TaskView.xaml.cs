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
        private Brush ColorTrue { get; } = Brushes.GreenYellow;
        private Brush ColorFalse { get; } = Brushes.Red;
        private int lastSelectedMonth = 0;

        public TaskView(string taskName)
        {
            InitializeComponent();

            fileHandler = new FileHandler();
            currentYear = DateTime.Today.Year;
            this.TaskName = taskName;
            DrawEntireYear(currentYear);

            //TODO: Create Task to save .dcf every minute or so, remove fileHandler business from draw functions
        }

        private void DrawEntireYear(int year)
        {
            Grid_Calendar.Children.RemoveRange(0, Grid_Calendar.Children.Count);
            bool ly = IsLeapYear(year);
            calendar = fileHandler.ReadDcf(TaskName, year);
            currentYear = year;

            int gapInPx = 3;
            double buttonWidth = (Grid_Calendar.Width - 10 + gapInPx) / 31 - gapInPx;
            double buttonHeight = (Grid_Calendar.Height - 10 + gapInPx) / 12 - gapInPx;
            for (Month m = Month.January; m <= Month.December; m++)
            {
                int init = IndexOfFirstDayInMonth(m, ly);
                for (int index = init; index < init + DaysInMonth(m, ly); index++)
                {
                    DayButton b = new DayButton
                    {
                        Index = index,
                        Width = buttonWidth,
                        Height = buttonHeight,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness((index - IndexOfFirstDayInMonth(m, ly)) * (buttonWidth + gapInPx), ((int)m - 1) * (buttonHeight + gapInPx), 0, 0),
                        Background = calendar[index] ? ColorTrue : ColorFalse,
                        Visibility = Visibility.Visible
                    };
                    b.Click += (o, e) =>
                    {
                        calendar[b.Index] ^= true;
                        b.Background = calendar[b.Index] ? ColorTrue : ColorFalse;
                        fileHandler.WriteDcf(calendar, TaskName, currentYear);
                    };
                    Grid_Calendar.Children.Add(b);
                }
            }
        }

        private void DrawMonth(Month month)
        {
            int start = IndexOfFirstDayInMonth(month, IsLeapYear(currentYear));
            string datestring = $"1/{(int)month}/{currentYear}";
            int weekday = (int)ConvertToNonRetardedWeekdayEnum(DateTime.Parse(datestring).DayOfWeek);
            int week = 0;

            Grid_Calendar.Children.RemoveRange(0, Grid_Calendar.Children.Count);
            bool ly = IsLeapYear(currentYear);
            calendar = fileHandler.ReadDcf(TaskName, currentYear);

            int gapInPx = 3;
            double buttonWidth = (Grid_Calendar.Width - 10 + gapInPx) / 7 - gapInPx;
            double buttonHeight = (Grid_Calendar.Height - 10 + gapInPx) / 5 - gapInPx;
            for (int i = start; i < start + DaysInMonth(month); i++)
            {
                DayButton b = new DayButton
                {
                    Index = i,
                    Width = buttonWidth,
                    Height = buttonHeight,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(weekday * (buttonWidth + gapInPx), week * (buttonHeight + gapInPx), 0, 0),
                    Background = calendar[i] ? ColorTrue : ColorFalse,
                    Visibility = Visibility.Visible
                };
                b.Click += (o, e) =>
                {
                    calendar[b.Index] ^= true;
                    b.Background = calendar[b.Index] ? ColorTrue : ColorFalse;
                    fileHandler.WriteDcf(calendar, TaskName, currentYear);
                };
                Grid_Calendar.Children.Add(b);
                if (++weekday % 7 == 0)
                {
                    weekday %= 7;
                    week++;
                }
            }
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

        private void ComboBox_SelectMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_SelectMonth.SelectedIndex == -1)
                return;
            CheckBox_ViewEntireYear.IsChecked = false;
            DrawMonth((Month)(ComboBox_SelectMonth.SelectedIndex + 1));
        }

        private void CheckBox_ViewEntireYear_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckBox_ViewEntireYear.IsChecked)
            {
                lastSelectedMonth = ComboBox_SelectMonth.SelectedIndex;
                ComboBox_SelectMonth.SelectedIndex = -1;
                DrawEntireYear(currentYear);
            }
            else
            {
                if (ComboBox_SelectMonth.SelectedIndex == -1)
                    ComboBox_SelectMonth.SelectedIndex = lastSelectedMonth;
            }
        }
    }
}
