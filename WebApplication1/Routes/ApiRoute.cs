namespace WebApplication1.Routes
{
    public class ApiRoute
    {
        public static class Products
        {
            public const string Base = "api/products";
            public const string GetAll = Base;
            public const string GetById = Base + "/Id";
            public const string Create = Base;
            public const string Update = Base + "/Id";
            public const string Delete = Base + "/Id";
        }

        public static class Orders
        {
            public const string Base = "api/orders";
            public const string GetById = Base + "/Id";
            public const string Create = Base;
            public const string Delete = Base + "/Id";
        }
    }
}
