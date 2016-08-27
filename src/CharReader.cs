namespace AppToolkit.Html
{
    sealed class CharReader
    {
        public const char EOF = '\x3';

        public string Source { get; }

        public int Position { get; set; }

        public CharReader(string source)
        {
            Source = source;
        }

        int savedPosition;
        public int SavePosition()
        {
            return savedPosition = Position;
        }

        public int RestorePosition()
        {
            return Position = savedPosition;
        }

        public char Peek()
        {
            if (Position < Source.Length)
                return Source[Position];
            else
                return EOF;
        }

        public char Read()
        {
            if (Position < Source.Length)
                return Source[Position++];
            else
                return EOF;
        }

        public string ReadUntil(char c, bool includeEndChar)
        {
            var position = Position;
            var index = Source.IndexOf(c, position);
            if (index == -1)
            {
                Position = Source.Length - 1;
                return Source.Substring(position);
            }
            else
            {
                Position = index + 1;
                if (includeEndChar)
                    return Source.Substring(position, index - position + 1);
                else
                    return Source.Substring(position, index - position + 1);
            }
        }

        public bool SquenceMatches(string input)
        {
            SavePosition();

            foreach (var c in input)
                if (Read() != c)
                {
                    RestorePosition();
                    return false;
                }

            return true;
        }
    }
}
