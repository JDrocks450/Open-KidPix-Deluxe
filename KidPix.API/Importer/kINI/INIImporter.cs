using KidPix.API.Importer.Mohawk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer.kINI
{
    public abstract record INIObject;

    /// <summary>
    /// A simple *.INI file parser for Kid Pix *.cfg files and *.ini files.
    /// </summary>
    public class INIImporter
    {
        private enum Mode
        {
            InSection,
            SectionNameRead,            
        }

        public static KidPixINIFile Import(string FileName)
        {
            using FileStream fs = File.OpenRead(FileName);
            return Import(fs);
        }

        public static KidPixINIFile Import(Stream Stream)
        {
            KidPixINIFile iniFile = new();
            INISection currentSection = iniFile.Root;

            StreamReader reader = new StreamReader(Stream);

            Mode mode = Mode.InSection;
            string str1 = "";

            while(!reader.EndOfStream)
            {
                char currentChar = (char)reader.Read();                
                switch (mode)
                {
                    case Mode.InSection:
                        switch (currentChar)
                        {
                            case '\0':
                            case ' ':
                            case '\r':
                            case '\t':
                            case '\n':
                                break;
                            case '[':
                                mode = Mode.SectionNameRead;
                                str1 = ""; // state transition to SectionRead
                                break;
                            case ';':
                            case '/':
                            case '#':
                                { // comment
                                    string line = reader.ReadLine();
                                    iniFile.Root.Objects.Add(new INIComment(line));
                                }
                                break;
                            default:
                                { // lazy line parsing strategy
                                    string line = currentChar + reader.ReadLine();
                                    if (!line.Contains('=')) break;
                                    string[] items = line.Split('=');
                                    if (items.Length <= 1) break;
                                    string name = items[0];
                                    string value = items[1];
                                    currentSection.Objects.Add(new INIProperty(name, value));
                                }
                                break;
                        }
                         
                        break;
                    case Mode.SectionNameRead:
                        if (currentChar == ']')
                        {
                            currentSection = new(str1);
                            iniFile.Add(str1, currentSection);
                            str1 = "";
                            mode = Mode.InSection;
                        }
                        else str1 += currentChar;                                                     
                        break;
                }     
            }
            return iniFile;
        }
    }
}
