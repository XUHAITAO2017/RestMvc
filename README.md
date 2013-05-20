# What is RestMVC

RestMvc is a simple library for building RESTful services in ASP.NET MVC.
It's primary purpose is to provide routing and content negotiation (conneg).
The routing differs from other RESTful routing libraries, like SimplyRestful,
in that the route is defined alongside the action that receives the route.
It was largely inspired by the Ruby framework Sinatra (http://www.sinatrarb.com/).

# Examples

    public class OrdersController : Controller
    {
        [Get("/orders")]
        public ActionResult Index() { ... }

        [Post("/orders"]
        public ActionResult Create() { ... }

        [Get("/orders/{id}.format", "/orders/{id}")]
        public ActionResult Show(string id) { ... }

        [Put("/orders/{id}")]
        public ActionResult Edit(string id) { ... }

        [Delete("/orders/{id}")]
        public ActionResult Destroy(string id) { ... }
    }

    // In Global.asax.cs
    RouteTable.Routes.Map<OrdersController>();
    // or RouteTable.Routes.MapAssembly(Assembly.GetExecutingAssembly());

The code above will do the following:
* Create the routes defined by the HTTP methods and URI templates in the attributes.
  Even though System.Web.Routing does not allow you to prefix URI templates with either
  / or ~/, I find allowing those prefixes can enhance readability, and thus they are allowed.
* Route HEAD and OPTIONS methods for the two URI templates ("orders" and "orders/{id}")
  to a method capable of handling those methods intelligently.
* Route PUT and DELETE for /orders, and POST for /orders/{id}, to a method
  that knows to return a 405 HTTP status code (Method Not Supported) with an appropriate
  Allow header.  This method and the ones that handle HEAD and OPTIONS are defined as virtual
  on RestfulController.  If you want them mapped by RestMvc, but want to customize them
  (e.g., adding a body on OPTIONS), you can subclass RestfulController and override the
  appropriate methods.
* Add routes for tunnelling PUT and DELETE through POST for HTML browser support.
  Creating a form with a hidden field called _method set to either PUT or DELETE
  will route to either Edit or Destroy.
* Notice the optional format parameter on the GET method actions (Show uses it;
  Index does not).  Routes with an extension are routed such that the extension
  gets passed as the format parameter, if the resource supports multiple representations
  (e.g. /orders/1.xml routes to Show with a format of xml).  The ordering of the URI templates
  in the Get attribute is important.  Had I reversed the order, /orders/1.xml would have
  matched with an id of "1.xml" and an empty format
  
The last point is a convenient way to handle multiple formats for a resource.  Since
it's in the URL, it can be bookmarked and emailed, with the same representation
regardless of the HTTP headers.  Even if content negotiation is used, it allows
you to bypass the standard negotiation process.  RestMvc does not automatically
provide these routes for you - notice that two URIs are specified on the Show method.

Content negotiation is provided as a decorator to the standard RouteHandler.
One of the problems I've seen with some other RESTful routing libraries is that
they define the IRouteHandler internally, which removes your ability to add
any custom hooks into the routing process.  My hope is that providing the
functionality as a decorator allows for more flexibility.

    // In Global.asax.cs
    var map = new MediaTypeFormatMap();
    map.Add(MediaType.Html, "html");
    map.Add(MediaType.Xhtml, "html");
    map.Add(MediaType.Xml, xml");

    var connegRouter = new ContentNegotiationRouteProxy(new MvcRouteHandler(), map);

    RouteTable.Routes.Map<OrdersController>(connegRouter);
    // or RouteTable.Routes.MapAssembly(Assembly.GetExecutingAssembly(), connedRouter);

In the absence of a route URI template specifying the format explicitly,
the connegDecorator will examine the Accept request header and pick the
first media type supported in the map.  Wildcard mathes are supported
(e.g. text/* matches text/html).

The content negotiation is quite simple at the moment.  The q parameter in
the Accept header is completely ignored.  By default, it tries to abide by
the Accept header prioritization inferred from the order of the MIME types
in the header.  However, you can change it to allow the server ordering,
as defined by the order MIME types are added to the MediaTypeFormatMap,
to take priority.  This was added to work around what I consider to be a bug
in Google Chrome - despite being unable to natively render XML, it prioritizes
XML over HTML in its Accept header.

# Building RestMVC

Hopefully, build.bat should do the trick.  RestMVC uses CM.NET,
a build library I've developed hosted at http://github.com/bbyars/CM.NET.
The output should be placed in the build directory.

The build will install the RestMvc.Example project as a virtual directory
under the Default Web Site and run some functional tests against it.
I believe the functional tests will only work on IIS 7 - I think
we would need to add a wildcard script map accepting all HTTP verbs
on previous versions of IIS.

build.bat /t:Coverage should spit out test coverage in the build directory.

4. Contributing

Patches and suggestions are always welcome.  You can reach me at brandon.byars@gmail.com.
Feel free to fork the repository at http://github.com/bbyars/RestMvc.
