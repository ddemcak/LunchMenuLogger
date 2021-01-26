﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.Graphics.Colors;

namespace LunchMenuLogger
{
    class LunchMenuItem
    {
        #region FIELDS
        

        #endregion

        #region PROPERTIES

        public string Name { get; }
        public double Price { get; }
        public FoodType FoodType { get; }
        public DateTime Day { get; }
        public double Volume { get; }
        public double Weight { get; }
        public string Alergens { get; }


        #endregion



        public LunchMenuItem(DateTime dt, string textline)
        {
            Day = dt;

            // Parse line
            Weight = FindWeight(textline);
            Volume = FindVolume(textline);
            Alergens = FindAlergens(textline);
            Price = FindPrice(textline);
            Name = FindName(textline, Volume, Weight);

            if (Volume != 0) FoodType = FoodType.soup;
            if (Weight != 0) FoodType = FoodType.maindish;
        }


        private double FindVolume(string text)
        {
            Regex rgx = new Regex(@"[0-9]{1},[0-9]{1,2}l");
            Match match = rgx.Match(text);

            if (match.Success)
            {
                return double.Parse(match.Value.Replace("l", "").Replace(",", "."), CultureInfo.InvariantCulture);
            }

            return 0;
        }

        private double FindWeight(string text)
        {
            Regex rgx = new Regex(@"[0-9]{1,3}g");
            Match match = rgx.Match(text);

            if (match.Success)
            {
                return double.Parse(match.Value.Replace("g", ""));
            }

            return 0;
        }

        private string FindAlergens(string text)
        {
            Regex rgx = new Regex(@"\([A,\d]+\)");
            Match match = rgx.Match(text);

            if (match.Success)
            {
                return match.Value.Replace("(", "").Replace(")", "");
            }

            return null;
        }

        private double FindPrice(string text)
        {
            Regex rgx = new Regex(@"\–.[0-9]{1,3}");
            Match match = rgx.Match(text);

            if (match.Success)
            {
                return double.Parse(match.Value.Replace("– ", ""));
            }

            return 0;
        }

        private string FindName(string text, double vlm, double wgh)
        {
            if (vlm != 0)
            {
                Regex rgx = new Regex(@"l.*\(");
                Match match = rgx.Match(text);

                if (match.Success) return match.Value[1..^1].Trim();
            }
            else if (wgh != 0)
            {
                Regex rgx = new Regex(@"g.*\(");
                Match match = rgx.Match(text);

                if (match.Success) return match.Value[1..^1].Trim();
            }
            return null;
        }


        public override string ToString()
        {
            if(FoodType == FoodType.soup) return string.Format("{0:dd.MM.yyyy}: {1}l {2} - {3} Kč ({4})", Day, Volume, Name, Price, Alergens);
            else return string.Format("{0:dd.MM.yyyy}: {1}g  {2} - {3} Kč ({4})", Day, Weight, Name, Price, Alergens);
        }

        public string ToSqlQueryValues()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            if (FoodType == FoodType.soup) return string.Format("'{0}','{1}','{2}','{3:yyyy-MM-dd}','{4}','{5}'", Name, Price, FoodType, Day, Volume.ToString(nfi), Alergens);
            else return string.Format("'{0}','{1}','{2}','{3:yyyy-MM-dd}','{4}','{5}'", Name, Price, FoodType, Day, Weight.ToString(nfi), Alergens);
        }
    }

    


    public enum FoodType
    { 
        soup,
        maindish
    }
}
