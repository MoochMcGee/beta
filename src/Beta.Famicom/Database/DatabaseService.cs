using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace Beta.Famicom.Database
{
    public static class DatabaseService
    {
        private static SHA1 sha1Provider = SHA1.Create();
        private static XmlSerializer serializer = new XmlSerializer(typeof(DatabaseInstance));

        private static DatabaseInstance database;

        static DatabaseService()
        {
            using (var reader = File.OpenText("drivers/fc.sys/db.xml"))
            {
                database = serializer.Deserialize(reader) as DatabaseInstance;
            }
        }

        public static Board find(byte[] data)
        {
            var hash = sha1Provider.ComputeHash(data, 16, data.Length - 16);
            var sha1 = BitConverter.ToString(hash).Replace("-", "");
            var linq = from game in database.games
                       from cartridge in game.cartridges
                       where cartridge.sha1 == sha1
                       select cartridge.boards[0];

            var result = linq.FirstOrDefault();
            if (result == null)
            {
                return new Board
                {
                    solderPad = new Pad { h = 0, v = 0 },
                    chip = new System.Collections.Generic.List<Chip>(),
                    vram = new System.Collections.Generic.List<Ram>(),
                    wram = new System.Collections.Generic.List<Ram>(),
                    chr = new System.Collections.Generic.List<Rom>
                    {
                        new Rom { sizeString = "8k" }
                    },
                    prg = new System.Collections.Generic.List<Rom>
                    {
                        new Rom { sizeString= "32k" }
                    },
                    type = "NES-SxROM"
                };
            }

            return result;
        }
    }
}
