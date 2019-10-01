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
            Label_CurrentYear.Content = $"Current Year: {currentYear}";

            int gapInPx = 3;
            double buttonWidth = (Grid_Calendar.Width - 10 + gapInPx) / 32 - gapInPx;
            double buttonHeight = (Grid_Calendar.Height - 10 + gapInPx) / 13 - gapInPx;
            for (int i = 1; i <= 31; i++)
            {
                Label l = GetStandardCalendarLabel(buttonWidth, buttonHeight);
                l.Margin = new Thickness(i * (buttonWidth + gapInPx), 0, 0, 0);
                l.Content = i.ToString();
                Grid_Calendar.Children.Add(l);
            }
            for (Month m = Month.January; m <= Month.December; m++)
            {
                Label l = GetStandardCalendarLabel(buttonWidth, buttonHeight);
                l.Margin = new Thickness(0, ((int)m) * (buttonHeight + gapInPx), 0, 0);
                l.Content = m.ShortName();
                Grid_Calendar.Children.Add(l);

                int init = IndexOfFirstDayInMonth(m, ly);
                for (int index = init; index < init + DaysInMonth(m, ly); index++)
                {
                    DayButton b = GetStandardCalendarButton(index, buttonWidth, buttonHeight);
                    b.Margin = new Thickness((index - IndexOfFirstDayInMonth(m, ly) + 1) * (buttonWidth + gapInPx), ((int)m) * (buttonHeight + gapInPx), 0, 0);
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
            double buttonWidth = (Grid_Calendar.Width - 10 + gapInPx) / 8 - gapInPx;
            double buttonHeight = (Grid_Calendar.Height - 10 + gapInPx) / 6 - gapInPx;

            for (int i = 1; i <= 7; i++)
            {
                Label l = GetStandardCalendarLabel(buttonWidth, buttonHeight);
                l.Margin = new Thickness(i * (buttonWidth + gapInPx), 0, 0, 0);
                l.Content = ((Weekday)(i - 1)).ShortName();
                Grid_Calendar.Children.Add(l);
            }
            for (int i = start; i < start + DaysInMonth(month, ly); i++)
            {
                DayButton b = GetStandardCalendarButton(i, buttonWidth, buttonHeight);
                b.Margin = new Thickness((weekday + 1) * (buttonWidth + gapInPx), (week + 1) * (buttonHeight + gapInPx), 0, 0);
                b.Content = (i - start + 1).ToString();
                Grid_Calendar.Children.Add(b);
                if (++weekday % 7 == 0)
                {
                    weekday %= 7;
                    week++;
                }
            }
            int weeks = (week == 4 && weekday == 0) ? 4 : 5;
            for (int i = 1; i <= weeks; i++)
            {
                Label l = GetStandardCalendarLabel(buttonWidth, buttonHeight);
                l.Margin = new Thickness(0, i * (buttonHeight + gapInPx), 0, 0);
                l.Content = $"Week {ComputeCalendarWeek(start + i * 7, currentYear)}";
                Grid_Calendar.Children.Add(l);
            }
        }

        private Label GetStandardCalendarLabel(double width, double height)
        {
            return new Label
            {
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White,
                Visibility = Visibility.Visible
            };
        }

        private DayButton GetStandardCalendarButton(int index, double width, double height)
        {
            DayButton b = new DayButton
            {
                Index = index,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = calendar[index] ? ColorTrue : ColorFalse,
                Visibility = Visibility.Visible
            };
            b.Click += (o, e) =>
            {
                calendar[b.Index] ^= true;
                b.Background = calendar[b.Index] ? ColorTrue : ColorFalse;
                fileHandler.WriteDcf(calendar, TaskName, currentYear);
            };
            if (DateTime.Today.Year == currentYear && IndexOfDate(DateTime.Today) == index)
            {
                b.BorderBrush = Brushes.White;
                b.BorderThickness = new Thickness(3d);
            }
            return b;
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
