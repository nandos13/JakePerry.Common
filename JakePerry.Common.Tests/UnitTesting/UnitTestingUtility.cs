namespace JakePerry.UnitTesting
{
    public static class UnitTestingUtility
    {
        public static string? StringifyArgument(object? o)
        {
            if (o is null) return "null";
            if (o is string s) return $"\"{s}\"";
            if (o is char c) return $"\'{c}\'";
            return o.ToString();
        }
    }
}
