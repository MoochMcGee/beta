using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Beta.Famicom.Formats;
using SimpleInjector;

namespace Beta.Famicom.Boards
{
    public sealed class BoardFactory
    {
        private readonly CartridgeFactory factory;
        private readonly Container container;

        public BoardFactory(Container container, CartridgeFactory factory)
        {
            this.container = container;
            this.factory = factory;
        }

        public IBoard GetBoard(byte[] binary)
        {
            var info = factory.Create(binary);
            var type = GetBoardType(info.mapper);

            var instance = (IBoard)container.GetInstance(type);
            instance.ApplyImage(info);

            return instance;
        }

        private Type GetBoardType(string boardType)
        {
            var linq =
                from type in typeof(IBoard).Assembly.GetTypes()
                where typeof(IBoard).IsAssignableFrom(type)
                from attribute in type.GetCustomAttributes<BoardNameAttribute>()
                where Regex.IsMatch(boardType, attribute.Pattern)
                select type;

            var match = linq.FirstOrDefault();
            if (match == null)
            {
                throw new NotSupportedException($"Mapper '{boardType}' isn't supported.");
            }

            return match;
        }
    }
}
