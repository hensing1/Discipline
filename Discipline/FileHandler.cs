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

            name = name.Trim();

            foreach (XmlNode node in tasksDoc.DocumentElement.ChildNodes)
                if (node.Attributes.GetNamedItem("Name").Value.ToLower() == name.ToLower())
                    throw new ArgumentException("Task name already exists.");

            XmlNode taskNode = tasksDoc.CreateElement("Task");

            XmlAttribute taskName = tasksDoc.CreateAttribute("Name");
            taskName.Value = name;
            taskNode.Attributes.Append(taskName);

            tasksDoc.DocumentElement.AppendChild(taskNode);

            int year = DateTime.Now.Year;
            GenerateEntryNode(name, year);

            tasksDoc.Save(XmlURI);

            WriteYear(new bool[DaysInYear(year)], name, year);
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

            XmlNode entryNode = taskNode.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Entry")
                                            .SingleOrDefault(e => e.Attributes.GetNamedItem("Year").Value == year.ToString());
            string dcfContent = (entryNode ??= GenerateEntryNode(taskName, year)).InnerText;

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
                throw new FileFormatException("Something went terribly wrong");
            }

            return calendar;
        }

        public void WriteYear(bool[] calendar, string taskName, int year)
        {
            if (!GetTasks().Contains(taskName))
                throw new ArgumentException("The specified task name does not exist.");

            //generating the string representing the year
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

            //saving to xml
            XmlNode entryNode = tasksDoc.DocumentElement.GetElementsByTagName("Task").Cast<XmlNode>().Single(e => e.Attributes.GetNamedItem("Name").Value == taskName)
                .ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Entry")
                .SingleOrDefault(e => e.Attributes.GetNamedItem("Year").Value == year.ToString());

            (entryNode ??= GenerateEntryNode(taskName, year)).InnerText = fileString;

            tasksDoc.Save(XmlURI);
        }

        private XmlNode GenerateEntryNode(string taskName, int year)
        {
            XmlNode entryNode = tasksDoc.CreateElement("Entry");
            XmlAttribute fileYear = tasksDoc.CreateAttribute("Year");
            fileYear.Value = year.ToString();
            entryNode.Attributes.Append(fileYear);
            entryNode.InnerText = IsLeapYear(year) ? "F366" : "F355";

            tasksDoc.DocumentElement.GetElementsByTagName("Task").Cast<XmlNode>().Single(e => e.Attributes.GetNamedItem("Name").Value == taskName).AppendChild(entryNode);

            return entryNode;
        }
    }
}
