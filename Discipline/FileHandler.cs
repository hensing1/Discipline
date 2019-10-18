using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static HenrysDevLib.Misc.Time;

namespace Discipline
{
    public class FileHandler
    {
        static string RootPath { get { return Directory.GetCurrentDirectory(); } }
        static string XmlURI { get { return Path.Combine(RootPath, "Tasks.xml"); } }
        private XmlDocument tasksDoc;

        public FileHandler()
        {
            try
            {
                tasksDoc = new XmlDocument();
                tasksDoc.Load(XmlURI);
            }
            catch (FileNotFoundException)
            {
                tasksDoc = new XmlDocument();
                tasksDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                tasksDoc.AppendChild(tasksDoc.CreateElement("Tasks"));
                tasksDoc.Save(XmlURI);
            }
        }

        public void AddTask(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Task name must not be empty.");

            char[] illegal = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '_'};
            foreach (char c in illegal)
                if (name.Contains(c))
                    throw new FormatException("Task name contains an illegal character.");

            name = name.Trim();

            foreach (XmlNode node in tasksDoc.DocumentElement.ChildNodes)
                if (node.Attributes.GetNamedItem("Name").Value.ToLower() == name.ToLower())
                    throw new ArgumentException("Task name already exists.");

            XmlNode taskNode = tasksDoc.CreateElement("Task");

            XmlAttribute taskName = tasksDoc.CreateAttribute("Name");
            taskName.Value = name;
            taskNode.Attributes.Append(taskName);

            //XmlAttribute taskFile = tasksDoc.CreateAttribute("File");
            //taskFile.Value = taskNode.Attributes.GetNamedItem("Name").Value.ToLower().Replace(' ', '_') + ".xml";
            //taskNode.Attributes.Append(taskFile);
            int year = DateTime.Now.Year;
            XmlNode fileNode = tasksDoc.CreateElement("File");
            XmlAttribute fileYear = tasksDoc.CreateAttribute("Year");
            fileYear.Value = year.ToString();
            XmlAttribute fileName = tasksDoc.CreateAttribute("FileName");
            fileName.Value = ConvertToFileName(taskNode.Attributes.GetNamedItem("Name").Value, year);
            fileNode.Attributes.Append(fileName);
            fileNode.Attributes.Append(fileYear);

            taskNode.AppendChild(fileNode);

            tasksDoc.DocumentElement.AppendChild(taskNode);

            tasksDoc.Save(XmlURI);

            WriteDcf(new bool[DaysInYear(year)], name, year);
        }

        private static string ConvertToFileName(string taskName, int year)
        {
            return $"{taskName.ToLower().Replace(' ', '_')}_{year}.dcf"; // .dcf = discipline calendar file
        }

        public string[] GetTasks()
        {
            tasksDoc.Load(XmlURI);
            List<string> tasks = new List<string>();
            foreach (XmlNode node in tasksDoc.DocumentElement.ChildNodes)
            {
                if (node.Name == "Task")
                    tasks.Add(node.Attributes.GetNamedItem("Name").Value);
            }
            return tasks.ToArray();
        }

        public bool[] ReadDcf(string taskName, int year)
        {
            tasksDoc.Load(XmlURI);
            XmlNode taskNode = null;
            try
            {
                taskNode = tasksDoc.GetElementsByTagName("Task").Cast<XmlNode>().Single(e => e.Attributes.GetNamedItem("Name").Value == taskName);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("The specified task name does not exist.");
            }

            string dcfPath = string.Empty;
            try
            {
                XmlNode fileNode = taskNode.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "File")
                                            .Single(e => e.Attributes.GetNamedItem("Year").Value == year.ToString());
                dcfPath = fileNode.Attributes.GetNamedItem("FileName").Value;
            }
            catch (InvalidOperationException)
            {
                int numdays = HenrysDevLib.Misc.Time.DaysInYear(year);
                WriteDcf(new bool[numdays], taskName, year);
                return ReadDcf(taskName, year);
            }

            string dcfContent = File.ReadAllText(dcfPath);
            bool[] calendar = new bool[HenrysDevLib.Misc.Time.DaysInYear(year)];
            int cIndex = 0;
            bool b = dcfContent[0] == 'T';
            dcfContent = dcfContent.Substring(1);
            int[] a = dcfContent.Split('_').Select<string, int>(e => int.Parse(e)).ToArray();
            try
            {
                for (int i = 0; i < a.Length; i++)
                {
                    for (; a[i] > 0; a[i]--)
                    {
                        calendar[cIndex] = b;
                        cIndex++;
                    }
                    b ^= true;
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new FileFormatException("The .dcf is fucked. I'm sorry.");
            }

            return calendar;
        }

        /// <summary>
        /// The discipline calendar format (.dcf) is a way to compactly store the boolean value for every day of an entire year. 
        /// </summary>
        public void WriteDcf(bool[] calendar, string taskName, int year)
        {
            if (!GetTasks().Contains(taskName))
                throw new ArgumentException("The specified task name does not exist.");

            int numdays = IsLeapYear(year) ? (int)LeapYear.Days : (int)Year.Days;
            if (numdays != calendar.Length)
                throw new ArgumentException("The calendar provided has an incorrect number of days.");

            string fileString = string.Empty;
            bool lastValue = calendar[0];
            fileString += lastValue ? 'T' : 'F';
            int counter = 1;
            for (int i = 1; i < calendar.Length; i++)
            {
                if (calendar[i] == lastValue)
                    counter++;
                else
                {
                    fileString += counter.ToString();
                    fileString += "_";
                    lastValue ^= true;
                    counter = 1;
                }
            }
            fileString += counter.ToString();

            //oh god
            XmlNode fileNode = tasksDoc.DocumentElement.GetElementsByTagName("Task").Cast<XmlNode>().Single(e => e.Attributes.GetNamedItem("Name").Value == taskName)
                .ChildNodes.Cast<XmlNode>().Where(node => node.Name == "File")
                .SingleOrDefault(e => e.Attributes.GetNamedItem("Year").Value == year.ToString());

            if (fileNode != null)
                File.WriteAllText(fileNode.Attributes.GetNamedItem("FileName").Value, fileString);
            else
            {
                fileNode = tasksDoc.CreateElement("File");
                XmlAttribute fileYear = tasksDoc.CreateAttribute("Year");
                fileYear.Value = year.ToString();
                XmlAttribute fileName = tasksDoc.CreateAttribute("FileName");
                fileName.Value = ConvertToFileName(taskName, year);
                fileNode.Attributes.Append(fileName);
                fileNode.Attributes.Append(fileYear);

                tasksDoc.DocumentElement.GetElementsByTagName("Task").Cast<XmlNode>().Single(e => e.Attributes.GetNamedItem("Name").Value == taskName).AppendChild(fileNode);
                tasksDoc.Save(XmlURI);

                File.WriteAllText(fileNode.Attributes.GetNamedItem("FileName").Value, fileString);
            }
        }
    }
}
