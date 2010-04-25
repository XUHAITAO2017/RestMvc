using System.Web;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using RestMvc.Conneg;

namespace RestMvc.UnitTests.Conneg
{
    [TestFixture]
    public class SimpleContentNegotiationRouteProxyTest
    {
        [Test]
        public void ShouldMapMediaTypeToFormat()
        {
            var map = new MediaTypeFormatMap();
            map.Add("text/xml", "xml");
            var router = new SimpleContentNegotiationRouteProxy(null, map);
            var route = new RouteData();

            router.AddFormat(route, new[] { "*/*" });

            Assert.That(route.Values["format"], Is.EqualTo("xml"));
        }

        [Test]
        public void ShouldNotSetFormatIfRoutingSystemAlreadyDetectedIt()
        {
            var map = new MediaTypeFormatMap();
            map.Add("text/xml", "xml");
            var router = new SimpleContentNegotiationRouteProxy(null, map);
            var route = new RouteData();
            route.Values["format"] = "html";

            router.AddFormat(route, new[] { "*/*" });

            Assert.That(route.Values["format"], Is.EqualTo("html"));
        }

        [Test]
        public void ShouldUseDefaultFormatIfNoAcceptTypesProvided()
        {
            var map = new MediaTypeFormatMap();
            map.Add("text/xml", "xml");
            var router = new SimpleContentNegotiationRouteProxy(null, map);
            var route = new RouteData();

            router.AddFormat(route, new string[0]);

            Assert.That(route.Values["format"], Is.EqualTo("xml"));
        }

        [Test]
        public void ShouldUseDefaultFormatIfNullAcceptTypesProvided()
        {
            var map = new MediaTypeFormatMap();
            map.Add("text/xml", "xml");
            var router = new SimpleContentNegotiationRouteProxy(null, map);
            var route = new RouteData();

            router.AddFormat(route, null);

            Assert.That(route.Values["format"], Is.EqualTo("xml"));
        }

        [Test]
        public void ShouldPrioritizeFormatSelectionByAcceptTypeOrdering()
        {
            var map = new MediaTypeFormatMap();
            map.Add("text/xml", "xml");
            map.Add("text/html", "html");
            var router = new SimpleContentNegotiationRouteProxy(null, map);
            var route = new RouteData();

            router.AddFormat(route, new[] { "text/html", "text/xml" });

            Assert.That(route.Values["format"], Is.EqualTo("html"));
        }

        [Test]
        public void UnsupportedAcceptTypeMapsToDefaultFormat()
        {
            var map = new MediaTypeFormatMap();
            map.Add("text/xml", "xml");
            var router = new SimpleContentNegotiationRouteProxy(null, map);
            var route = new RouteData();

            router.AddFormat(route, new[] { "audio/*" });

            Assert.That(route.Values["format"], Is.EqualTo("xml"));
        }

        [Test]
        public void GetHandlerProxiesToPassedInHandler()
        {
            var proxiedHandler = new Mock<IRouteHandler>();
            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(ctx => ctx.Request.AcceptTypes).Returns(new string[0]);
            var request = new RequestContext(httpContext.Object, new RouteData());
            var router = new SimpleContentNegotiationRouteProxy(proxiedHandler.Object, new MediaTypeFormatMap());

            router.GetHttpHandler(request);

            proxiedHandler.Verify(h => h.GetHttpHandler(request));
        }
    }
}