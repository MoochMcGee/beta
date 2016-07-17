using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public class DatabaseService : IDatabase
    {
        private static SHA1 sha1Provider = SHA1.Create();
        private static XmlSerializer serializer = new XmlSerializer(typeof(DatabaseInstance));

        private static DatabaseInstance database;

        public DatabaseService()
        {
            using (var reader = File.OpenText("drivers/fc.sys/db.xml"))
            {
                database = serializer.Deserialize(reader) as DatabaseInstance;
            }
        }

        public Board Find(byte[] data)
        {
            var hash = sha1Provider.ComputeHash(data, 16, data.Length - 16);
            var sha1 = BitConverter.ToString(hash).Replace("-", "");
            var linq = from game in database.Games
                       from cartridge in game.Cartridges
                       where cartridge.Sha1 == sha1
                       select cartridge.Boards[0];

            var result = linq.FirstOrDefault();
            if (result == null)
            {
                return new Board
                {
                    SolderPad = new Pad { H = 0, V = 0 },
                    Chip = new System.Collections.Generic.List<Chip>(),
                    Vram = new System.Collections.Generic.List<Ram>(),
                    Wram = new System.Collections.Generic.List<Ram>(),
                    Chr = new System.Collections.Generic.List<Rom>
                    {
                        new Rom { SizeString = "8k" }
                    },
                    Prg = new System.Collections.Generic.List<Rom>
                    {
                        new Rom { SizeString= "32k" }
                    },
                    Type = "NES-SxROM"
                };
            }

            return result;
        }
    }
}
