using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LunchMenuLogger
{
    class LunchMenu
    {
        private string rawText;

        private DateTime validFrom;
        private DateTime validTo;

        private List<LunchMenuItem> menuItems = new List<LunchMenuItem>();

        public DateTime ValidFrom { get => validFrom; }
        public DateTime ValidTo { get => validTo; }
        public List<LunchMenuItem> MenuItems { get => menuItems; }

        public LunchMenu(string txt)
        {
            rawText = txt;

            FindDateTimeRange();

            FindItemsPerDay(ValidFrom, "Pondělí:", "Úterý:");
            FindItemsPerDay(ValidFrom.AddDays(1), "Úterý:", "Středa:");
            FindItemsPerDay(ValidFrom.AddDays(2), "Středa:", "Čtvrtek:");
            FindItemsPerDay(ValidFrom.AddDays(3), "Čtvrtek:", "Pátek:");
            FindItemsPerDay(ValidFrom.AddDays(4), "Pátek:", "Tabulka");

        }

        private void FindDateTimeRange()
        {
            Regex rgx = new Regex(@"[0-9]{1,2}\..[0-9]{1,2}\..[0-9]{4}");
            Match match = rgx.Match(rawText);

            if (match.Success)
            {
                validTo = DateTime.Parse(match.Value, new CultureInfo("cs-CZ", false));
                validFrom = validTo.Subtract(new TimeSpan(4, 0, 0, 0));
            }

        }



        private void FindItemsPerDay(DateTime dt, string fromDay, string toDay)
        {
            Regex rgx = new Regex(string.Format(@"{0}.+{1}", fromDay, toDay));
            Match match = rgx.Match(rawText);

            if (match.Success)
            {
                string[] items = match.Value.Replace(fromDay, "").Replace(toDay, "").Trim().Split("Kč");

                foreach (string item in items)
                {
                    if (item.Length > 0)
                    {
                        LunchMenuItem lmi = new LunchMenuItem(dt, item);
                        if (lmi.Name != null) menuItems.Add(lmi);
                    }
                }
            }

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Validity: {0} - {1}", ValidFrom, ValidTo));

            foreach (LunchMenuItem lmi in menuItems) sb.AppendLine(lmi.ToString());
            
            return sb.ToString();
        }

    }
}
