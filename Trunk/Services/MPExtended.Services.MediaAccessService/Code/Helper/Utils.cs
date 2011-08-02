﻿#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code.Helper
{
    public class DBLocations
    {
        public string Music;
        public string Pictures;
        public string TvSeries;
        public string MovingPictures;
        public string Shares;
        public string Videos;
    }

    public class Utils
    {
        private static string logDir = AppDomain.CurrentDomain.BaseDirectory + "\\logs";
        private static Dictionary<String, WebBannerPath> CachedWebBannerPaths = null;
        private static String CachedMPLocation;
        private static DBLocations CachedDbLocation;

        public static String GetMpConfigPath()
        {
            if (CachedMPLocation == null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Configuration.GetPath("MediaAccess.xml"));
                XmlNode gNode = doc.SelectSingleNode("/appconfig/config/mpconfig");
                CachedMPLocation = gNode.InnerText;
            }

            return CachedMPLocation;
        }

        public static DBLocations GetMPDbLocations()
        {
            if (Utils.CachedDbLocation == null)
            {
                DBLocations dbLocations = new DBLocations();
                XmlDocument doc = new XmlDocument();
                doc.Load(Configuration.GetPath("MediaAccess.xml"));
                XmlNodeList dbNodes = doc.SelectNodes("/appconfig/mpdatabases/database");

                Log.Debug("Reading database paths");

                foreach (XmlNode node in dbNodes)
                {
                    String name = node.Attributes["name"].Value;
                    String value = node.Attributes["filename"].Value;
                    Log.Debug(name + ": " + value);
                    switch (name)
                    {
                        case "music":
                            dbLocations.Music = value;
                            break;
                        case "pictures":
                            dbLocations.Pictures = value;
                            break;
                        case "tvseries":
                            dbLocations.TvSeries = value;
                            break;
                        case "movingpictures":
                            dbLocations.MovingPictures = value;
                            break;
                        case "shares":
                            dbLocations.Shares = value;
                            break;
                        case "videos":
                            dbLocations.Videos = value;
                            break;
                    }
                }
                CachedDbLocation = dbLocations;
            }
            return CachedDbLocation;
        }

        public static void ChangeDbLocation(String _db, String _newPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Configuration.GetPath("MediaAccess.xml"));
            XmlNodeList dbNodes = doc.SelectNodes("/appconfig/mpdatabases/database");

            Log.Debug("Reading database paths");

            foreach (XmlNode node in dbNodes)
            {
                String name = node.Attributes["name"].Value;

                if (name.Equals(_db))
                {
                    node.Attributes["filename"].Value = _newPath;
                }
            }

            doc.Save(AppDomain.CurrentDomain.BaseDirectory + "config.xml");
        }

        public static String[] SplitString(String _stringToSplit)
        {
            if (_stringToSplit != null)
            {
                _stringToSplit = _stringToSplit.Trim(new char[] { '|', ' ' });
                return _stringToSplit.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return null;
           }
        }

        public static string ClearString(String _stringToClean)
        {
            return _stringToClean.Substring(2, (_stringToClean.Length - 5));
        }

        public static string GetBannerPath(string name)
        {
            XElement root = XElement.Load(Configuration.GetPath("MediaAccess.xml"));
            XElement res =
                root.Elements("thumbpaths").First().Elements("thumb").Where(x => (string)x.Attribute("name") == name).First();
            return (string)res.Attribute("path");
        }

        public static bool IsAllowedPath(string path)
        {
            if (Shares.IsAllowedPath(path))
                return true;

            // this checks whether the path is in at least one of the thumb paths
            XElement root = XElement.Load(Configuration.GetPath("MediaAccess.xml"));
            return
                    (from el in
                        (root.Elements("thumbpaths").First().Elements("thumb"))
                     where IsSubdir((string)el.Attribute("path"), path)
                     select el).Count()
                > 0;
        }

        // TODO: check performance and maybe optimize
        public static bool IsSubdir(string root, string testDir)
        {
            DirectoryInfo shareDir = new DirectoryInfo(root);
            DirectoryInfo currentDir = new DirectoryInfo(testDir);

            while (currentDir != null)
            {
                if (currentDir.FullName == shareDir.FullName)
                    return true;

                currentDir = currentDir.Parent;
            }

            return false;
        }
    }
}