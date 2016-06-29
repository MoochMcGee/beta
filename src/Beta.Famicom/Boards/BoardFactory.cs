using System;
using System.Linq;
using System.Reflection;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards
{
    public sealed class BoardFactory : IBoardFactory
    {
        private readonly ICartridgeFactory factory;

        public BoardFactory(ICartridgeFactory factory)
        {
            this.factory = factory;
        }

        public Board GetBoard(GameSystem gameSystem, byte[] cart)
        {
            var info = factory.Create(cart);
            var type = GetBoardType(info.Mapper);

            var board = (Board)Activator.CreateInstance(type, info);
            board.Cpu = gameSystem.Cpu;

            return board;
        }

        private Type GetBoardType(string boardType)
        {
            var linq = from type in typeof(Board).Assembly.GetTypes()
                       where typeof(Board).IsAssignableFrom(type)
                       from attribute in type.GetCustomAttributes<BoardNameAttribute>()
                       where attribute.Name == boardType
                       select type;

            var match = linq.FirstOrDefault();
            if (match == null)
            {
                throw new NotSupportedException($"Mapper \"{boardType}\" isn't supported.");
            }

            return match;
        }
    }
}
