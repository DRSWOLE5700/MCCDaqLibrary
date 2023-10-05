using MccDaq;
using System.Reflection.PortableExecutable;
using System.Xml;
using System.Xml.Serialization;

#pragma warning disable IDE0090

namespace ClassLibraryTestApp
{

    public class Config
    {
        [XmlAttribute("name")]
        public string? savename;
        [XmlAttribute("kgorlb")]
        public bool kgorlb;
        [XmlElement("Offset")]
        public Offset OffsetValues;
        [XmlElement("Multiplier")]
        public Multiplier MultValues;

        public Config() { }
        //{
            // Offset OffsetValues  = null!;
           //  Multiplier MultValues  = null!;
        //}

        public void GetConfig(ref Config configuration)
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(Config));

            // If the XML document has been altered with unknown
            // nodes or attributes, handles them with the
            // UnknownNode and UnknownAttribute events.
            //serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            //serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);


            //error handling in case test.xml doesn't exist
            try
            {
                using var reader = new StreamReader("test.xml");
                configuration = (Config)new XmlSerializer(typeof(Config)).Deserialize(reader);
                reader.Close();
            }
            
            catch (System.IO.FileNotFoundException)
            {
                //Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.StackTrace);
                Console.WriteLine("You likely forgot the config file.  Create new one? (y/n)");

                string? input;
                input = Console.ReadLine();

                

                if (input == "y" || input == "Y")
                {
                    Offset whoa = new Offset
                    {
                        cell0 = 0,
                        cell1 = 0,
                        cell2 = 0
                    };
                    Multiplier mhoa = new Multiplier
                    {
                        cell0 = 1,
                        cell1 = 1,
                        cell2 = 1
                    };
                    Config EmptyConfig = new Config
                    {
                        kgorlb = false,
                        OffsetValues = whoa,
                        MultValues = mhoa
                    };
                    SaveConfig(EmptyConfig);
                    GetConfig(ref configuration);
                }    
            }
            catch (System.IO.IOException)
            {
                return;
            }

        }

        public void SaveConfig(Config configuration)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            TextWriter reader = new StreamWriter("test.xml");
            try
            {
                serializer.Serialize(reader, configuration);

                reader.Close ();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                Config temp = new Config();
                temp.OffsetValues.cell0 = 0;
                temp.OffsetValues.cell1 = 0;
                temp.OffsetValues.cell2 = 0;
                temp.MultValues.cell0 = 0;
                temp.MultValues.cell1 = 0;
                temp.MultValues.cell2 = 0;

                serializer.Serialize(reader, temp);
                reader.Close();

            }
        }
    }

    public class Offset
    {
        [XmlElement("cell0")]
        public double cell0;
        [XmlElement("cell1")]
        public double cell1;
        [XmlElement("cell2")]
        public double cell2;
    }

    public class Multiplier
    {
        [XmlElement("cell0")]
        public double cell0;
        [XmlElement("cell1")]
        public double cell1;
        [XmlElement("cell2")]
        public double cell2;
    }


    //INPUT: string filepath = the path to the xml config file


    //0 for kg, 1 for lb


    //require function call to specify kg or lb
    //public LCInfo(bool kgORlb) => _kgORlb = kgORlb;



    /*

    public void NewConfig() 
    { 
        //writes a new config to the file
    }

    public void LoadConfig()
    {
        //reads list of configs and loads the one the user wants
    }

    public void EditConfig()
    {
        //edits the current config
    }

    public void SaveConfig()
    {
        //saves the current config as is including any zeroing
    }

    public void DeleteConfig()
    {
        //deletes the current config
    }

      ______
     /     /
   _oIIIIIo_
  []======[]
    */



}
