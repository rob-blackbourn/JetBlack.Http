namespace JetBlack.HttpServer.Routing
{
    public class Route
    {
        public PathDefinition Path { get; }
        public HttpController Controller { get; }

        public Route(PathDefinition path, HttpController controller)
        {
            Path = path;
            Controller = controller;
        }
    }
}