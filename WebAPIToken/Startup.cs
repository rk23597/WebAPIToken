using System;
using System.Configuration;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Owin;
using WebAPIToken.Providers;

[assembly: OwinStartup(typeof(WebAPIToken.Startup))]

namespace WebAPIToken
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

          //  app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            var httpConfig = new HttpConfiguration();


            ConfigureOAuthTokenGeneration(app);

            //remove this we you dont want i have use for jwt but still i getting error if you can resolve the error the go ahead
            //and please update me as well 

           // ConfigureOAuthTokenConsumption(app);

           
            ConfigureWebApi(httpConfig);

            app.UseCors(CorsOptions.AllowAll);

            app.UseWebApi(httpConfig);


        }

     

        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat("http://localhost:49543")
               

            };
            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

        //this is for jwt consumption . not yet working
        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {

            const string issuer = "http://localhost:49543";
            var audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            var audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);


            app.UseJwtBearerAuthentication(
              new JwtBearerAuthenticationOptions
              {
                  AuthenticationMode = AuthenticationMode.Active,
                  AllowedAudiences = new[] { audienceId },
                  IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                  {
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                  }

              });

            

         //   app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

        private void ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
