namespace HashComparer.Services
{
    public interface IHasher
    {
        string Hash(string data, char[] keyCharArray);
    }
}