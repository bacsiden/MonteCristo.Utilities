using AspNetCore.Identity.Mongo.Model;

namespace MonteCristo.MongoDB.Framework.Models
{
    public class ApplicationRole : MongoRole
    {
        public ApplicationRole():base()
        {

        }

        public ApplicationRole(string name) : base(name)
        {

        }
    }
}
