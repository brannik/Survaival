public static class CustomLoggerFactory
{
    public static ICustomLoggerService GetLogger()
    {
#if UNITY_EDITOR
        return new EditorLoggerService();
#else
        return new RuntimeLoggerService();
#endif
    }
}
