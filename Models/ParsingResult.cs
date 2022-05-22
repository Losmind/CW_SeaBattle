namespace Course_work.Models
{
    public class ParsingResult
    {
        public ParsingResult(Field playerField, Field aiField, int size, string currentMove)
        {
            PlayerField = playerField;
            AiField = aiField;
            Size = size;
            CurrentMove = currentMove;
        }

        public Field PlayerField { get; }
        public Field AiField { get; }
        public int Size { get; }
        public string CurrentMove { get; }

    }
}