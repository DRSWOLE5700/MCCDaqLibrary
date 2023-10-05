using ClassLibraryMCCDAQ;
using System.Xml.Serialization;


namespace ClassLibraryTestApp
{
    internal class Program
    {

        // at 500 pounds the load cell reads 24.331 mV or 0.024331 V
        /// <summary>
        /// 500lb/24.331mV/100 = 
        /// 500lb/0.024331V/100
        /// 5/0.024331 lb/v
        /// </summary>


        //                                             This is a random constant calculated to get the right value
        float lastTworeadings = 0;


        static void Main()
        {
            //declare read and write timers and timer intervals
            System.Timers.Timer timerRead = new(interval: 125);
            ClassLibraryMCCDAQ.Library TestClass = new();

            //Initialize all variables as 0
            /*
            Offset whoa = new Offset
            {
                cell0 = 0,
                cell1 = 0,
                cell2 = 0
            };
            Multiplier mhoa = new Multiplier
            {
                cell0 = 0,
                cell1 = 0,
                cell2 = 0
            };

            Config Setconfig = new Config
            {
                kgorlb = false,
                OffsetValues = whoa,
                MultValues = mhoa

            };
            */

            Config Setconfig = new Config();

            //initialize load cell offsets
            Setconfig.GetConfig(ref Setconfig);

            //nullable string for the input
            string? input;



            //Counter for logging files.  kinda ghetto
            int writeCounter = -1;

            //Declare the GetReading option
            timerRead.Elapsed += (source, e) => GetReading(TestClass, Setconfig);

            Console.WriteLine("\nWhat range of voltage would you like to measure? (1/2/5/10)");
            input = Console.ReadLine();
            Int16 value = Convert.ToInt16(input);

            TestClass.GetVoltageBIP(value);
            bool boardConnected = false;
            CheckBoard(TestClass, ref boardConnected);

            do
            {
                Console.WriteLine("\n\n******************************************************************"
                + "\nEnter a command:"
                + "\n[1] Read Weights"
                + "\n[2] Read and Log Weights"
                + "\n[3] Check for board"
                + "\n[4] Zero load cell"
                + "\n[5] Update Multiplier"
                + "\n[6] Exit\n");


                input = Console.ReadLine();
                try
                {
                    value = Convert.ToInt16(input);

                }
                catch
                {
                    value = 0;
                }

                switch (value)
                {
                    case 1:
                        //to avoid error if user decides not to connect the board in the beginning
                        if (!boardConnected)
                        {
                            Console.WriteLine("\nConnect board before reading.");
                            break;
                        }
                        do
                        {
                            Console.WriteLine("\nPress enter to continue to reading... \n");
                            input = Console.ReadLine();
                            Console.WriteLine("Voltage: \n");


                            timerRead.Start();

                            //Pressing enter causes the timer and thus the reading to stop
                            Console.ReadLine();
                            timerRead.Stop();

                            Console.WriteLine("\n\nReading paused.  Continue reading? (Y/N)");
                            input = Console.ReadLine();

                        } while (input == "y" || input == "Y" || input == "Yes" || input == "yes");
                        break;

                    case 2:
                        //to avoid error if user decides not to connect the board in the beginning
                        if (!boardConnected)
                        {
                            Console.WriteLine("\nConnect board before reading.");
                            break;
                        }

                        writeCounter++;
                        string fileName = "temp" + writeCounter + ".txt";
                        System.IO.StreamWriter streamWriterOutput = new(fileName);

                        System.Timers.Timer timerWrite = new(interval: 125);

                        //These MUST be declared in the switch statement because each time this option is selected, a new file is created.
                        streamWriterOutput.Write("Channel 1" + "\t\t" + "Channel 2" + "\t\t" + "Channel 3");
                        timerWrite.Elapsed += (source, e) => GetReadingAndWrite(TestClass, streamWriterOutput, Setconfig);

                        do
                        {
                            Console.WriteLine("\nPress enter to continue to reading... \n");
                            input = Console.ReadLine();
                            Console.WriteLine("Voltage: \n");


                            timerWrite.Start();

                            //Pressing enter causes the timer and thus the reading to stop
                            Console.ReadLine();
                            timerWrite.Stop();

                            Console.WriteLine("\n\nReading paused.  Continue reading? (Y/N)");
                            input = Console.ReadLine();

                        } while (input == "y" || input == "Y" || input == "Yes" || input == "yes");

                        //make way for a new output file
                        streamWriterOutput.Close();

                        //may fix bug where output files are not created on subsequent readings
                        //timerWrite.Dispose();

                        break;

                    case 3:
                        CheckBoard(TestClass, ref boardConnected);
                        break;

                    case 4:
                        if (!boardConnected)
                        {
                            Console.WriteLine("\nConnect board before zeroing.");
                            break;
                        }
                        Console.WriteLine("Which channel do you want to zero?: ");
                        input = Console.ReadLine();
                        if (input == "0")
                            Setconfig.OffsetValues.cell0 = TestClass.ZeroLoadCell(0);
                        else if (input == "1")
                            Setconfig.OffsetValues.cell1 = TestClass.ZeroLoadCell(1);
                        else if (input == "2")
                            Setconfig.OffsetValues.cell2 = TestClass.ZeroLoadCell(2);
                        else
                            Console.WriteLine("Please input a valid channel");

                        break;

                    case 5:
                        Console.WriteLine("For which channel do you want to update the multiplier?: ");
                        input = Console.ReadLine();

                        try
                        {
                            if (input == "0")
                                Setconfig.MultValues.cell0 = UpdateMultiplier();
                            else if (input == "1")
                                Setconfig.MultValues.cell1 = UpdateMultiplier();
                            else if (input == "2")
                                Setconfig.MultValues.cell2 = UpdateMultiplier();
                            else
                                Console.WriteLine("Please input a valid channel.");
                            break;
                        }
                        catch
                        {
                            Console.WriteLine("Invaild Input");
                            break;
                        }
                        
                        

                    case 6:
                        //First check to see if user wants to save their updates to the config file

                        //if (CheckForChange(Setconfig))
                        //{
                        //    string? choice2;
                        //    Console.WriteLine("Changes were detected in the configuration.  "
                        //        + "Would you like to save those changes? (y/n)");
                        //    choice2 = Console.ReadLine();
                        //    if (choice2 == "y" || choice2 == "Y")
                                Setconfig.SaveConfig(Setconfig);
                        //}
                        return;

                    default:
                        Console.WriteLine("\nInvalid input.  Enter input again:");
                        break;
                }
            } while (true);


        }

        static void CheckBoard(Library TestClass, ref bool connected)
        {
            string? input;
            do
            {
                if (TestClass.CheckForUSB1608G())
                {
                    Console.WriteLine("Board connected!");
                    connected = true;
                    return;
                }
                else
                {
                    Console.WriteLine("Board not connected.");
                    connected = false;
                    Console.WriteLine("Check again? (Y/N)");
                    input = Console.ReadLine();
                }



            } while (input == "y" || input == "Y" || input == "Yes" || input == "yes");
        }

#pragma warning disable IDE0060 //unused parameters reserved for future use
        static void GetReadingAndWrite(Library TestClass,
            System.IO.StreamWriter stream, Config config)
        {

            double read = Math.Round(TestClass.GetReading(0), 7);
            //double output0 = loadCellConst * (read - offset0);
            double output0 = Math.Round((TestClass.GetReading(0) - config.OffsetValues.cell0) * config.MultValues.cell0, 3);
            double output1 = Math.Round((TestClass.GetReading(1) - config.OffsetValues.cell1) * config.MultValues.cell1, 3);
            double output2 = Math.Round((TestClass.GetReading(2) - config.OffsetValues.cell2) * config.MultValues.cell2, 3);

            stream.Write("\r" + output0 + "\t\t" + output1 + "\t\t" + output2);
            Console.Write("\r" + output0 + "\t\t" + output1 + "\t\t" + output2);
        }

        static void GetReading(Library TestClass, Config config)
        {

            //double read = Math.Round(TestClass.GetReading(0), 7);
            //double output0 = loadCellConst * (read - offset0);
            double output0 = Math.Round((TestClass.GetReading(0) - config.OffsetValues.cell0) * config.MultValues.cell0, 3);
            double output1 = Math.Round((TestClass.GetReading(1) - config.OffsetValues.cell1) * config.MultValues.cell1, 3);
            double output2 = Math.Round((TestClass.GetReading(2) - config.OffsetValues.cell2) * config.MultValues.cell2, 3);

            Console.Write("\r" + output0 + "\t\t" + output1 + "\t\t" + output2);
            //Console.Write("\r" + Math.Round(TestClass.GetReading(0), 4)
            //    + "\t\t" + Math.Round(TestClass.GetReading(1), 4)
            //    + "\t\t" + Math.Round(TestClass.GetReading(2), 4));
        }
#pragma warning restore IDE0060

        static double UpdateMultiplier()
        {
            string? input;
            double value;
            Console.WriteLine("Enter the float value of the new multiplier: \n");
            input = Console.ReadLine();
            if (input != null)
            {
                value = Convert.ToDouble(input);
                return value;
            }
            else
            {
                Console.WriteLine("What");
                return 1;
            }
        }


        static bool CheckForChange(Config curr)
        {
            Config old = new();
            old.GetConfig(ref old);

            if (old.Equals(curr))
                return true;
            else
                return false;
        }


        static void LoadConfig(string filepath, ref Config configuration)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(Config));

            // If the XML document has been altered with unknown
            // nodes or attributes, handles them with the
            // UnknownNode and UnknownAttribute events.
            //serializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
            //serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);


            using var reader = new StreamReader(filepath);
            configuration = (Config)new XmlSerializer(typeof(Config)).Deserialize(reader);


        }

        static protected void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        static protected void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " +
            attr.Name + "='" + attr.Value + "'");
        }
        /*
        static void Read(Library TestClass)
        {

        }
        */

    }
}