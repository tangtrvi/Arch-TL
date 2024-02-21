namespace Arch_TL.BLL
{
    public class ArchMath
    {
        public static int Sum(params int[] elements)
        {
            if (elements == null || elements.Length == 0) 
                return 0;
            return elements.Sum();
        }
    }
}