using System;
using System.Configuration;
using System.Xml;

namespace TPS9266xEvaluationModule
{
    public class IndividualTabSettings : ConfigurationElement
    {
        public string currentlySelectedTab = "Tab";

        [ConfigurationProperty("Device", DefaultValue = "518", IsRequired = true)]
        public string Device
        {
            get
            {
                return (string)this["Device"];
            }
            set
            {
                setAttribute("Device", value);
            }
        }

        [ConfigurationProperty("Address", DefaultValue = 0, IsRequired = true)]
        public int Address
        {
            get
            {
                return (int)this["Address"];
            }
            set
            {
                setAttribute("Address", value);
            }
        }

        [ConfigurationProperty("Enable1", DefaultValue = false, IsRequired = false)]
        public bool Enable1
        {
            get
            {
                return (bool)this["Enable1"];
            }
            set
            {
                setAttribute("Enable1", value);
            }
        }

        [ConfigurationProperty("Enable2", DefaultValue = false, IsRequired = false)]
        public bool Enable2
        {
            get
            {
                return (bool)this["Enable2"];
            }
            set
            {
                setAttribute("Enable2", value);
            }
        }

        [ConfigurationProperty("Enable3", DefaultValue = false, IsRequired = false)]
        public bool Enable3
        {
            get
            {
                return (bool)this["Enable3"];
            }
            set
            {
                setAttribute("Enable3", value);
            }
        }

        [ConfigurationProperty("Enable4", DefaultValue = false, IsRequired = false)]
        public bool Enable4
        {
            get
            {
                return (bool)this["Enable4"];
            }
            set
            {
                setAttribute("Enable4", value);
            }
        }

        [ConfigurationProperty("Enable5", DefaultValue = false, IsRequired = false)]
        public bool Enable5
        {
            get
            {
                return (bool)this["Enable5"];
            }
            set
            {
                setAttribute("Enable5", value);
            }
        }

        [ConfigurationProperty("Enable6", DefaultValue = false, IsRequired = false)]
        public bool Enable6
        {
            get
            {
                return (bool)this["Enable6"];
            }
            set
            {
                setAttribute("Enable6", value);
            }
        }

        [ConfigurationProperty("Enable7", DefaultValue = false, IsRequired = false)]
        public bool Enable7
        {
            get
            {
                return (bool)this["Enable7"];
            }
            set
            {
                setAttribute("Enable7", value);
            }
        }

        [ConfigurationProperty("Enable8", DefaultValue = false, IsRequired = false)]
        public bool Enable8
        {
            get
            {
                return (bool)this["Enable8"];
            }
            set
            {
                setAttribute("Enable8", value);
            }
        }

        [ConfigurationProperty("Enable9", DefaultValue = false, IsRequired = false)]
        public bool Enable9
        {
            get
            {
                return (bool)this["Enable9"];
            }
            set
            {
                setAttribute("Enable9", value);
            }
        }

        [ConfigurationProperty("Enable10", DefaultValue = false, IsRequired = false)]
        public bool Enable10
        {
            get
            {
                return (bool)this["Enable10"];
            }
            set
            {
                setAttribute("Enable10", value);
            }
        }

        [ConfigurationProperty("Enable11", DefaultValue = false, IsRequired = false)]
        public bool Enable11
        {
            get
            {
                return (bool)this["Enable11"];
            }
            set
            {
                setAttribute("Enable11", value);
            }
        }

        [ConfigurationProperty("Enable12", DefaultValue = false, IsRequired = false)]
        public bool Enable12
        {
            get
            {
                return (bool)this["Enable12"];
            }
            set
            {
                setAttribute("Enable12", value);
            }
        }

        [ConfigurationProperty("Enable13", DefaultValue = false, IsRequired = false)]
        public bool Enable13
        {
            get
            {
                return (bool)this["Enable13"];
            }
            set
            {
                setAttribute("Enable13", value);
            }
        }

        [ConfigurationProperty("Enable14", DefaultValue = false, IsRequired = false)]
        public bool Enable14
        {
            get
            {
                return (bool)this["Enable14"];
            }
            set
            {
                setAttribute("Enable14", value);
            }
        }

        [ConfigurationProperty("Enable15", DefaultValue = false, IsRequired = false)]
        public bool Enable15
        {
            get
            {
                return (bool)this["Enable15"];
            }
            set
            {
                setAttribute("Enable15", value);
            }
        }

        [ConfigurationProperty("Enable16", DefaultValue = false, IsRequired = false)]
        public bool Enable16
        {
            get
            {
                return (bool)this["Enable16"];
            }
            set
            {
                setAttribute("Enable16", value);
            }
        }

        [ConfigurationProperty("Enable17", DefaultValue = false, IsRequired = false)]
        public bool Enable17
        {
            get
            {
                return (bool)this["Enable17"];
            }
            set
            {
                setAttribute("Enable17", value);
            }
        }

        [ConfigurationProperty("Enable18", DefaultValue = false, IsRequired = false)]
        public bool Enable18
        {
            get
            {
                return (bool)this["Enable18"];
            }
            set
            {
                setAttribute("Enable18", value);
            }
        }

        [ConfigurationProperty("Text1", DefaultValue = "", IsRequired = false)]
        public string Text1
        {
            get
            {
                return (string)this["Text1"];
            }
            set
            {
                setAttribute("Text1", value);
            }
        }

        [ConfigurationProperty("Combo0_1", DefaultValue = (byte)0, IsRequired = false)]
        public byte Combo0_1
        {
            get
            {
                return (byte)this["Combo0_1"];
            }
            set
            {
                setAttribute("Combo0_1", value);
            }
        }

        [ConfigurationProperty("Combo0_2", DefaultValue = (byte)0, IsRequired = false)]
        public byte Combo0_2
        {
            get
            {
                return (byte)this["Combo0_2"];
            }
            set
            {
                setAttribute("Combo0_2", value);
            }
        }

        [ConfigurationProperty("Combo1_1", DefaultValue = (byte)0, IsRequired = false)]
        public byte Combo1_1
        {
            get
            {
                return (byte)this["Combo1_1"];
            }
            set
            {
                setAttribute("Combo1_1", value);
            }
        }

        [ConfigurationProperty("Combo1_2", DefaultValue = (byte)0, IsRequired = false)]
        public byte Combo1_2
        {
            get
            {
                return (byte)this["Combo1_2"];
            }
            set
            {
                setAttribute("Combo1_2", value);
            }
        }

        [ConfigurationProperty("Combo0", DefaultValue = (byte)0, IsRequired = false)]
        public byte Combo0
        {
            get
            {
                return (byte)this["Combo0"];
            }
            set
            {
                setAttribute("Combo0", value);
            }
        }

        [ConfigurationProperty("Combo1", DefaultValue = (byte)0, IsRequired = false)]
        public byte Combo1
        {
            get
            {
                return (byte)this["Combo1"];
            }
            set
            {
                setAttribute("Combo1", value);
            }
        }

        [ConfigurationProperty("Combo2", DefaultValue = (byte)0, IsRequired = false)]
        public byte Combo2
        {
            get
            {
                return (byte)this["Combo2"];
            }
            set
            {
                setAttribute("Combo2", value);
            }
        }

        [ConfigurationProperty("Combo3", DefaultValue = (byte)0, IsRequired = false)]
        public byte Combo3
        {
            get
            {
                return (byte)this["Combo3"];
            }
            set
            {
                setAttribute("Combo3", value);
            }
        }

        [ConfigurationProperty("Slider0_1", DefaultValue = (UInt16)0, IsRequired = false)]
        public UInt16 Slider0_1
        {
            get
            {
                return (UInt16)this["Slider0_1"];
            }
            set
            {
                setAttribute("Slider0_1", value);
            }
        }

        [ConfigurationProperty("Slider0_2", DefaultValue = (UInt16)0, IsRequired = false)]
        public UInt16 Slider0_2
        {
            get
            {
                return (UInt16)this["Slider0_2"];
            }
            set
            {
                setAttribute("Slider0_2", value);
            }
        }

        [ConfigurationProperty("Slider0_3", DefaultValue = (UInt16)0, IsRequired = false)]
        public UInt16 Slider0_3
        {
            get
            {
                return (UInt16)this["Slider0_3"];
            }
            set
            {
                setAttribute("Slider0_3", value);
            }
        }

        [ConfigurationProperty("Slider1_1", DefaultValue = (byte)0, IsRequired = false)]
        public byte Slider1_1
        {
            get
            {
                return (byte)this["Slider1_1"];
            }
            set
            {
                setAttribute("Slider1_1", value);
            }
        }

        [ConfigurationProperty("Slider1_2", DefaultValue = (byte)0, IsRequired = false)]
        public byte Slider1_2
        {
            get
            {
                return (byte)this["Slider1_2"];
            }
            set
            {
                setAttribute("Slider1_2", value);
            }
        }

        [ConfigurationProperty("Slider1_3", DefaultValue = (byte)0, IsRequired = false)]
        public byte Slider1_3
        {
            get
            {
                return (byte)this["Slider1_3"];
            }
            set
            {
                setAttribute("Slider1_3", value);
            }
        }

        [ConfigurationProperty("Slider2_1", DefaultValue = (UInt16)0, IsRequired = false)]
        public UInt16 Slider2_1
        {
            get
            {
                return (UInt16)this["Slider2_1"];
            }
            set
            {
                setAttribute("Slider2_1", value);
            }
        }

        [ConfigurationProperty("Slider2_2", DefaultValue = (UInt16)0, IsRequired = false)]
        public UInt16 Slider2_2
        {
            get
            {
                return (UInt16)this["Slider2_2"];
            }
            set
            {
                setAttribute("Slider2_2", value);
            }
        }

        [ConfigurationProperty("Slider2_3", DefaultValue = (UInt16)0, IsRequired = false)]
        public UInt16 Slider2_3
        {
            get
            {
                return (UInt16)this["Slider2_3"];
            }
            set
            {
                setAttribute("Slider2_3", value);
            }
        }

        [ConfigurationProperty("Slider3_1", DefaultValue = (byte)0, IsRequired = false)]
        public byte Slider3_1
        {
            get
            {
                return (byte)this["Slider3_1"];
            }
            set
            {
                setAttribute("Slider3_1", value);
            }
        }

        [ConfigurationProperty("Slider3_2", DefaultValue = (byte)0, IsRequired = false)]
        public byte Slider3_2
        {
            get
            {
                return (byte)this["Slider3_2"];
            }
            set
            {
                setAttribute("Slider3_2", value);
            }
        }

        private void setAttribute(string attr, string newValue)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

      //      XmlNode node = xmlDoc.SelectSingleNode("//TabControlSettings/" + currentlySelectedTab);
            xmlDoc.SelectSingleNode("//TabControlSettings/" + currentlySelectedTab).Attributes[attr].Value = newValue;
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.RefreshSection("TabControlSettings");
        }

        private void setAttribute(string attr, int newValue)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

       //     XmlNode node = xmlDoc.SelectSingleNode("//TabControlSettings/" + currentlySelectedTab);
            xmlDoc.SelectSingleNode("//TabControlSettings/" + currentlySelectedTab).Attributes[attr].Value = newValue.ToString();
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.RefreshSection("TabControlSettings");
        }

        private void setAttribute(string attr, bool newValue)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            XmlNode node = xmlDoc.SelectSingleNode("//TabControlSettings/" + currentlySelectedTab);
            node.Attributes[attr].Value = newValue.ToString();
     //       xmlDoc.SelectSingleNode("//TabControlSettings/" + currentlySelectedTab).Attributes[attr].Value = newValue.ToString();
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.RefreshSection("TabControlSettings");
        }

        private void setAttribute(string attr, byte newValue)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

    //        XmlNode node = xmlDoc.SelectSingleNode("//TabControlSettings/" + currentlySelectedTab);
            xmlDoc.SelectSingleNode("//TabControlSettings/" + currentlySelectedTab).Attributes[attr].Value = newValue.ToString();
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.RefreshSection("TabControlSettings");
        }
    }

    public class TabControlSettings : ConfigurationSection
    {
        public IndividualTabSettings SelectedTab(string selectedTab)
        {
            if (selectedTab == "Tab0")
                return tab0;
            else if (selectedTab == "Tab1")  // only allowed 6 devices
                return tab1;
            else if (selectedTab == "Tab2")
                return tab2;
            else if (selectedTab == "Tab3")
                return tab3;

            return null;
        }

        [ConfigurationProperty("Tab0")]
        private IndividualTabSettings tab0
        {
            get
            {
                return (IndividualTabSettings)this["Tab0"];
            }
        }

        [ConfigurationProperty("Tab1")]
        private IndividualTabSettings tab1
        {
            get
            {
                return (IndividualTabSettings)this["Tab1"];
            }
        }

        [ConfigurationProperty("Tab2")]
        private IndividualTabSettings tab2
        {
            get
            {
                return (IndividualTabSettings)this["Tab2"];
            }
        }

        [ConfigurationProperty("Tab3")]
        private IndividualTabSettings tab3
        {
            get
            {
                return (IndividualTabSettings)this["Tab3"];
            }
        }
    }
}
